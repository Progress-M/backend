using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Main.PostgreSQL;
using Main.Function;
using Microsoft.AspNetCore.Cors;

using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System;
using Main.Models;
using Microsoft.Extensions.Configuration;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    [Authorize(Policy = "ValidAccessToken")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<UserController> _logger;
        readonly string subfolder = @"/image/user/";
        public IConfiguration Configuration { get; }

        public UserController(KindContext KindContext, ILogger<UserController> logger, IConfiguration configuration)
        {
            Context = KindContext;
            _logger = logger;
            Configuration = configuration;
        }

        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> GetUser(int id)
        {
            var item = await Context.User
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            return Ok(
                new
                {
                    status = AuthStatus.Success,
                    user = item,
                    access_token = Auth.generateToken(Configuration),
                    token_type = "bearer"
                });
        }

        [HttpGet("{id}/offer")]
        [Produces("application/json")]
        public async Task<ActionResult<OfferByUserResponse>> GetOffersByUser(int id)
        {
            var user = await Context.User.SingleOrDefaultAsync(user => user.Id == id);

            if (user == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            var age = DateTime.UtcNow.Year - user.BirthYear.Year;

            var offers = (await Context.Offer
                .Where(
                    offer =>
                        (offer.ForMan == user.isMan || offer.ForWoman == !user.isMan) &&
                        offer.LowerAgeLimit <= age && age <= offer.UpperAgeLimit
                )
                .Include(offer => offer.Company)
                    .ThenInclude(company => company.ProductCategory)
                .ToListAsync())
                .OrderByDescending(it => it.CreateDate)
                .Select(offer =>
                {
                    return new OfferResponse
                    {
                        Id = offer.Id,
                        Text = offer.Text,
                        DateStart = offer.DateStart,
                        DateEnd = offer.DateEnd,
                        TimeStart = offer.TimeStart,
                        TimeEnd = offer.TimeEnd,
                        Percentage = offer.Percentage,
                        Company = offer.Company,
                        ImageName = offer.ImageName,
                        CreateDate = offer.CreateDate,
                        ForMan = offer.ForMan,
                        LikeCounter = offer.LikeCounter,
                        ForWoman = offer.ForWoman,
                        SendingTime = offer.SendingTime,
                        UpperAgeLimit = offer.UpperAgeLimit,
                        LowerAgeLimit = offer.LowerAgeLimit,
                        UserLike = Context.LikedOffer.Any(lc => lc.OfferId == offer.Id && lc.UserId == id)
                    };
                }).ToList();

            var groups = OfferUtils.GroupByRelevance(offers);
            var nearbyOffer = groups.activeOffer.OrderBy(offer => Utils.CalculateDistance(
                new Location { Latitude = offer.Company.Latitude, Longitude = offer.Company.Longitude },
                new Location { Latitude = user.Latitude, Longitude = user.Longitude }
                ));

            return Ok(
                new OfferByUserResponse
                {
                    preOffer = groups.preOffer,
                    activeOffer = groups.activeOffer,
                    nearbyOffer = nearbyOffer,
                    inactiveOffer = groups.inactiveOffer
                }
            );
        }

        [HttpGet("{id}/favarite-offer")]
        [Produces("application/json")]
        public ActionResult<OfferByUserResponse> GetFavariteOffersByUser(int id)
        {
            var item = Context.FavoriteCompany
                .Include(fc => fc.User)
                .Include(fc => fc.Company)
                .Where(fc => fc.User.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            var favorites = item.ToList();

            var offers = Context.Offer
                .Where(offer => favorites.Any(fc => fc.Company.Id == offer.Company.Id))
                .Include(offer => offer.Company)
                    .ThenInclude(company => company.ProductCategory)
                .ToList()
                .Select(offer =>
                {
                    return new OfferResponse
                    {
                        Id = offer.Id,
                        Text = offer.Text,
                        DateStart = offer.DateStart,
                        DateEnd = offer.DateEnd,
                        TimeStart = offer.TimeStart,
                        TimeEnd = offer.TimeEnd,
                        Percentage = offer.Percentage,
                        Company = offer.Company,
                        ImageName = offer.ImageName,
                        CreateDate = offer.CreateDate,
                        ForMan = offer.ForMan,
                        LikeCounter = offer.LikeCounter,
                        ForWoman = offer.ForWoman,
                        SendingTime = offer.SendingTime,
                        UpperAgeLimit = offer.UpperAgeLimit,
                        LowerAgeLimit = offer.LowerAgeLimit,
                        UserLike = Context.LikedOffer.Any(lc => lc.OfferId == offer.Id && lc.UserId == id)
                    };
                }).ToList();


            var groups = OfferUtils.GroupByRelevance(offers);

            return Ok(
                new OfferByUserResponse
                {
                    preOffer = groups.preOffer,
                    activeOffer = groups.activeOffer,
                    inactiveOffer = groups.inactiveOffer
                }
            );
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult> GetUser()
        {
            return Ok(await Context.User.ToListAsync());
        }

        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<ActionResult> GetUserAvatar(int id)
        {
            var item = await Context.User
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                .Replace(Utils.subfolder, "");

            if (!System.IO.File.Exists(@$"{filePath}{subfolder}{item.AvatarName}"))
            {
                return NotFound($"Not found file with name = '{item.AvatarName}'");
            }

            var stream = System.IO.File.OpenRead(@$"{filePath}{subfolder}{item.AvatarName}");
            return new FileStreamResult(stream, "image/jpeg");
        }

        [HttpGet("{id}/stories")]
        public ActionResult GetStories(int id)
        {
            var item = Context.User
                .AsNoTracking()
                .SingleOrDefault(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            var favorites = Context.FavoriteCompany
                .Include(fc => fc.Company)
                .Where(fc => fc.UserId == id);


            var comparer = new CompanyComparer();

            return Ok(
            favorites
            .ToList()
            .Select(f => new
            {
                Company = f.Company,
                Stories = Context.Stories
                    .Include(s => s.Offer)
                    .ThenInclude(offer => offer.Company)
                    .Where(s => s.Offer.Company.Id == f.CompanyId && s.UserId == id)
                    .Select(s => s.Offer)
                    .ToList()
            })
           .OrderByDescending(x => x.Stories.Count));
        }

        [HttpDelete("{id}/stories/company/{companyId}")]
        public ActionResult DeleteStories(int id, int companyId)
        {
            var item = Context.User
                .SingleOrDefault(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            Context.Stories.RemoveRange(
                Context.Stories
                    .Include(s => s.Offer)
                    .ThenInclude(offer => offer.Company)
                    .Where(s => s.Offer.Company.Id == companyId && s.UserId == id)
                    .ToList()
            );

            Context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> CreateUser([FromForm] UserRequest user)
        {
            var old = await Context.User
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.PlayerId == user.playerId);

            if (old != null)
            {
                return BadRequest($"User with playerId = {old.PlayerId} alreadty exist");
            }

            var item = new User(user);
            Context.User.Add(item);
            await Context.SaveChangesAsync();

            if (user.image != null)
            {
                item.AvatarName = await Utils.saveFile(user.image, $"{subfolder}", item.Id);
            }
            else
            {
                item.AvatarName = "";
            }
            await Context.SaveChangesAsync();

            return Ok(
                new CreateUserResponse
                {
                    status = AuthStatus.Success,
                    user = item,
                    access_token = Auth.generateToken(Configuration),
                    token_type = "bearer"
                }
            );
        }

        [HttpPut("{id}")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> UserUpdate(int id, [FromForm] UserRequest oldUser)
        {
            var item = await Context.User
              .SingleOrDefaultAsync(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            item.PlayerId = oldUser.playerId;
            item.Latitude = oldUser.Latitude;
            item.Longitude = oldUser.Longitude;
            item.Name = oldUser.Name;
            item.isMan = oldUser.isMan;
            item.BirthYear = oldUser.BirthYear;

            await Context.SaveChangesAsync();
            return Ok(new
            {
                status = AuthStatus.Success,
                user = item
            });
        }

        [HttpPut("{id}/image")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> UserImageUpdate(int id, [FromForm] UserImageRequest oldUser)
        {
            var item = await Context.User
              .SingleOrDefaultAsync(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            Utils.deleteFile($"{subfolder}", item.AvatarName);
            if (oldUser.image != null)
            {
                item.AvatarName = await Utils.saveFile(oldUser.image, subfolder, item.Id);
                await Context.SaveChangesAsync();
            }

            return Ok(item);
        }

        [HttpGet("{userId}/company/{companyId}/distance")]
        public async Task<ActionResult<double>> Distance(int userId, int companyId)
        {
            var user = await Context.User
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == userId);

            if (user == null)
            {
                return NotFound($"Not found user with id = {userId}");
            }

            var company = await Context.Company
                .AsNoTracking()
                .SingleOrDefaultAsync(company => company.Id == companyId);

            if (company == null)
            {
                return NotFound($"Not found company with id = {companyId}");
            }

            return Ok(
                Utils.CalculateDistance(
                new Location { Latitude = user.Latitude, Longitude = user.Longitude },
                new Location { Latitude = company.Latitude, Longitude = company.Longitude }
                )
            );
        }

        [HttpGet("{id}/favorite")]
        public ActionResult GetUserFavorite(int id)
        {
            var item = Context.FavoriteCompany
                .Include(fc => fc.Company)
                .Where(fc => fc.UserId == id);

            return Ok(item.ToList().Select(fc => fc.Company));
        }

        [HttpPut("{id}/favorite/{favoriteId}")]
        public async Task<ActionResult> AddUserFavorite(int id, int favoriteId)
        {
            var item = await Context.User
              .SingleOrDefaultAsync(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            var favorite = await Context.Company
              .SingleOrDefaultAsync(c => c.Id == favoriteId);

            if (item == null)
            {
                return NotFound($"Not found company with id = {id}");
            }

            if (Context.FavoriteCompany.Any(fc => fc.CompanyId == favoriteId && fc.UserId == id))
            {
                return BadRequest("Already exist");
            }

            Context.FavoriteCompany.Add(new FavoriteCompany { User = item, Company = favorite });
            await Context.SaveChangesAsync();

            return Ok(item);
        }


        [HttpDelete("{id}/favorite/{favoriteId}")]
        public async Task<ActionResult> RemoveUserFavorite(int id, int favoriteId)
        {
            var fc = await Context.FavoriteCompany
              .SingleOrDefaultAsync(fc => fc.UserId == id && fc.CompanyId == favoriteId);

            if (fc == null)
            {
                return Ok();
            }

            Context.Stories.RemoveRange(
                            Context.Stories
                                .Include(s => s.Offer)
                                .ThenInclude(offer => offer.Company)
                                .Where(s => s.Offer.Company.Id == favoriteId && s.UserId == id)
                                .ToList()
                        );
            Context.FavoriteCompany.Remove(fc);
            await Context.SaveChangesAsync();

            return Ok();
        }
    }


}
