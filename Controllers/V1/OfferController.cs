using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;

using System.IO;
using Main.Function;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System;
using Main.Models;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    [Authorize(Policy = "ValidAccessToken")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class OfferController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<OfferController> _logger;

        public OfferController(KindContext KindContext, ILogger<OfferController> logger)
        {
            Context = KindContext;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> GetOffer(int id)
        {
            var item = await Context.Offer
                .AsNoTracking()
                .Include(offer => offer.Company)
                    .ThenInclude(company => company.ProductCategory)
               .SingleOrDefaultAsync(offer => offer.Id == id);

            if (item == null)
            {
                return NotFound($"Not found offer with id = {id}");
            }

            return Ok(item);
        }

        [HttpGet("top")]
        [Produces("application/json")]
        public ActionResult GetOfferTop()
        {
            var RESULT_LIMIT = 3;
            var offers = Context.Offer
                .AsNoTracking()
                .Include(offer => offer.Company)
                    .ThenInclude(company => company.ProductCategory)
               .Where(offer => offer.LikeCounter > 0)
               .OrderByDescending(offer => offer.LikeCounter)
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
                        CreateDate = offer.CreateDate,
                        ForMan = offer.ForMan,
                        LikeCounter = offer.LikeCounter,
                        ForWoman = offer.ForWoman,
                        SendingTime = offer.SendingTime,
                        UpperAgeLimit = offer.UpperAgeLimit,
                        LowerAgeLimit = offer.LowerAgeLimit,
                        UserLike = false
                    };
                }).ToList();

            var items = OfferUtils.GroupByRelevance(offers).activeOffer
               .Take(RESULT_LIMIT)
               .ToList();

            return Ok(items);
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult> GetOffers()
        {
            return Ok(
                (await Context.Offer
                    .Include(offer => offer.Company)
                        .ThenInclude(company => company.ProductCategory)
                    .ToListAsync())
                    .Where(offer => offer.SendingTime >= DateTime.UtcNow)
                    .OrderBy(order => order.DateStart)
            );
        }

        [HttpGet("category/{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> GetOffersByCategory(int id)
        {
            var category = await Context.ProductCategory.SingleOrDefaultAsync(c => c.Id == id);

            return Ok(
               (await Context.Offer
                    .Include(offer => offer.Company)
                        .ThenInclude(company => company.ProductCategory)
                    .Where(offer => offer.Company.ProductCategory == category)
                    .ToListAsync())
                    .Where(offer => offer.SendingTime >= DateTime.UtcNow)
                    .OrderBy(order => order.DateStart)
            );
        }

        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<ActionResult> GetOfferImage(int id)
        {
            var item = await Context.Offer
                .AsNoTracking()
                .Include(o => o.Image)
                .SingleOrDefaultAsync(offer => offer.Id == id);

            if (item == null)
            {
                return NotFound($"Not found offer with id = {id}");
            }

            if (item.Image == null)
            {
                return NotFound($"Not found offer image with offerId = {id}");
            }

            return File(item.Image.bytes, "image/png");
        }

        [HttpPut("{id}/image")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> UpdateOfferImage(int id, [FromForm] ImageRequest imageRequest)
        {
            var item = await Context.Offer
                .Include(o => o.Image)
                .SingleOrDefaultAsync(offer => offer.Id == id);

            if (item == null)
            {
                return NotFound($"Not found offer with id = {id}");
            }

            if (imageRequest.image != null)
            {
                if (item.Image != null)
                {
                    Context.Files.Remove(item.Image);
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    imageRequest.image.CopyTo(ms);

                    var file = new FileData { bytes = ms.ToArray() };
                    Context.Files.Add(file);
                    await Context.SaveChangesAsync();

                    item.Image = file;
                    await Context.SaveChangesAsync();
                }
            }

            return Ok();
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> CreateOffer([FromForm] OfferRequest offerRequest)
        {
            var company = await Context.Company
                .SingleOrDefaultAsync(company => company.Id == offerRequest.companyId);

            if (company == null)
            {
                return NotFound($"Not found comapny with id = {offerRequest.companyId}");
            }

            var offerByCompany = Context.Offer
                .AsNoTracking()
                .Include(o => o.Company)
                .OrderByDescending(o => o.CreateDate)
                .FirstOrDefault();

            if (offerByCompany != null)
            {
                double durationSeconds = DateTime.UtcNow.Subtract(offerByCompany.CreateDate).TotalSeconds;
                TimeSpan seconds = TimeSpan.FromSeconds(durationSeconds);
                if (seconds.TotalHours < 24)
                {
                    TimeSpan diffTimeSpan = TimeSpan.FromHours(24).Subtract(seconds);
                    string duration = String.Format(@"{0}:{1:mm\:ss\:fff}", diffTimeSpan.Days * 24.0 + diffTimeSpan.Hours, diffTimeSpan);
                    return NotFound(new ErrorResponse
                    {
                        status = ErrorStatus.OfferTimeError,
                        message = $"\"{company.NameOfficial}\"-company already created offer in last 24 hours. " +
                        $"There are {duration} left until the next opportunity to create offer."
                    });
                }
            }

            var favoriteCompanies = await Context.FavoriteCompany
                 .Include(fc => fc.Company)
                 .Include(fc => fc.User)
                 .Where(fc => fc.CompanyId == company.Id)
                 .ToListAsync();

            var offer = new Offer(offerRequest, company);

            if (offerRequest.image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    offerRequest.image.CopyTo(ms);

                    var file = new FileData { bytes = ms.ToArray() };
                    Context.Files.Add(file);
                    await Context.SaveChangesAsync();

                    offer.Image = file;
                }
            }

            await Context.Offer.AddAsync(offer);
            await Context.SaveChangesAsync();

            favoriteCompanies.ForEach(fc => Context.Stories.Add(new Stories { User = fc.User, Offer = offer }));
            await Context.SaveChangesAsync();

            Utils.CreateNotificationToFavorites($"{company.Name}: {offer.Text}", favoriteCompanies.Select(fc => fc.User.PlayerId).ToArray());

            return Ok(offer);
        }

        [HttpPost("like")]
        public async Task<ActionResult> LikeOffer(LikeRequest request)
        {
            var user = await Context.User
                .SingleOrDefaultAsync(user => user.Id == request.userId);

            if (user == null)
            {
                return NotFound($"Not found user with id = {request.userId}");
            }

            var offer = await Context.Offer
                .SingleOrDefaultAsync(offer => offer.Id == request.offerId);

            if (offer == null)
            {
                return NotFound($"Not found offer with id = {request.offerId}");
            }

            Context.LikedOffer.Add(new LikedOffer
            {
                User = user,
                Offer = offer
            });

            offer.LikeCounter++;
            await Context.SaveChangesAsync();

            return Ok(offer);
        }

        [HttpPost("dislike")]
        public async Task<ActionResult> DislikeOffer(LikeRequest request)
        {

            var likes = await Context.LikedOffer
                .Include(like => like.Offer)
                .Where(like => like.UserId == request.userId && like.OfferId == request.offerId)
                .ToListAsync();

            likes.ForEach(it =>
            {
                it.Offer.LikeCounter--;
                Context.LikedOffer.Remove(it);
            });
            await Context.SaveChangesAsync();

            return Ok();
        }
    }
}
