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
                .Include(user => user.LikedPosts)
                .Include(user => user.Favorites)
                    .ThenInclude(company => company.ProductСategory)
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
        public async Task<ActionResult> GetOffersByUser(int id)
        {
            var user = await Context.User.SingleOrDefaultAsync(user => user.Id == id);

            if (user == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            var age = DateTime.Now.Year - user.BirthYear.Year;

            var offers = await Context.Offer
                .Where(
                    offer =>
                        (offer.ForMan == user.isMan || offer.ForWoman == !user.isMan) &&
                        offer.LowerAgeLimit <= age && age <= offer.UpperAgeLimit
                )
                .Include(offer => offer.Company)
                    .ThenInclude(company => company.ProductСategory)
                .ToListAsync();

            var preOffer = offers.Where(offer => offer.TimeStart < DateTime.UtcNow);
            var activeOffer = offers.Where(offer => offer.TimeStart < DateTime.UtcNow && offer.TimeEnd > DateTime.UtcNow);
            var inactiveOffer = offers.Where(offer => offer.TimeEnd <= DateTime.UtcNow);

            return Ok(
                new OfferByUserResponse
                {
                    preOffer = preOffer,
                    activeOffer = activeOffer,
                    inactiveOffer = inactiveOffer
                }
            );
        }

        [HttpGet("{id}/favarite-offer")]
        [Produces("application/json")]
        public async Task<ActionResult> GetFavariteOffersByUser(int id)
        {
            var item = await Context.User
                .Include(offer => offer.Favorites)
                .SingleOrDefaultAsync(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            var favorites = item.Favorites.ToList();

            var offers = await Context.Offer
                .Where(offer => favorites.Contains(offer.Company))
                .Include(offer => offer.Company)
                    .ThenInclude(company => company.ProductСategory)
                .ToListAsync();

            var preOffer = offers.Where(offer => offer.TimeStart > DateTime.UtcNow);
            var activeOffer = offers.Where(offer => offer.TimeStart < DateTime.UtcNow && offer.TimeEnd < DateTime.UtcNow);
            var inactiveOffer = offers.Where(offer => offer.TimeEnd >= DateTime.UtcNow);

            return Ok(
                new OfferByUserResponse
                {
                    preOffer = preOffer,
                    activeOffer = activeOffer,
                    inactiveOffer = inactiveOffer
                }
            );
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult> GetUser()
        {
            return Ok(await Context.User.Include(u => u.Favorites).ToListAsync());
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

            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!System.IO.File.Exists(@$"{filePath}\image\user\{item.AvatarName}"))
            {
                return NotFound($"Not found file with name = '{item.AvatarName}'");
            }

            var stream = System.IO.File.OpenRead(@$"{filePath}\image\user\{item.AvatarName}");
            return new FileStreamResult(stream, "image/jpeg");
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
                item.AvatarName = await Utils.saveFile(user.image, @"\image\user\", item.Id);
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

            Utils.deleteFile(@"\image\user\", item.AvatarName);
            if (oldUser.image != null)
            {
                item.AvatarName = await Utils.saveFile(oldUser.image, @"\image\user\", item.Id);
                await Context.SaveChangesAsync();
            }

            return Ok(item);
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
              .SingleOrDefaultAsync(favorite => favorite.Id == favoriteId);

            if (item == null)
            {
                return NotFound($"Not found company with id = {id}");
            }

            if (item.Favorites == null)
            {
                item.Favorites = new HashSet<Company>();
            }

            item.Favorites.Add(favorite);
            await Context.SaveChangesAsync();

            return Ok(item);
        }


        [HttpDelete("{id}/favorite/{favoriteId}")]
        public async Task<ActionResult> RemoveUserFavorite(int id, int favoriteId)
        {
            var item = await Context.User
                .Include(u => u.Favorites)
                .SingleOrDefaultAsync(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            if (item.Favorites == null)
            {
                item.Favorites = new HashSet<Company>();
                await Context.SaveChangesAsync();
            }

            var favorite = item.Favorites.SingleOrDefault(c => c.Id == favoriteId);

            if (favorite == null)
            {
                return NotFound($"Not found favorite company with id = {id}");
            }

            item.Favorites.Remove(favorite);
            await Context.SaveChangesAsync();

            return Ok(item);
        }
    }


}
