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
using Microsoft.Extensions.Configuration;

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
        public IConfiguration _configuration { get; }

        public OfferController(KindContext KindContext, ILogger<OfferController> logger, IConfiguration Configuration)
        {
            Context = KindContext;
            _logger = logger;
            _configuration = Configuration;
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
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.OfferError,
                        message = $"Не найдена акция с id = '{id}'"
                    }
                );
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
                .Select(offer => new OfferResponse(offer, false))
                .ToList();

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
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.OfferError,
                        message = $"Не найдена акция с id = '{id}'"
                    }
                );
            }

            if (item.Image == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.OfferError,
                        message = $"Не найдена картинка акции с id = '{id}'"
                    }
                );
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
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.OfferError,
                        message = $"Не найдена акция с id = '{id}'"
                    }
                );
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

            if ((offerRequest.dateEnd - offerRequest.dateStart).TotalDays >= 30)
            {
                return BadRequest(new BdobrResponse
                {
                    status = ResponseStatus.OfferTimeError,
                    message = $"Нельзя создавать акции с периодом больше 30 дней"
                });
            }

            var company = await Context.Company
                .SingleOrDefaultAsync(company => company.Id == offerRequest.companyId);

            if (company == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Не найдена компания с id = '{offerRequest.companyId}'"
                    }
                );
            }

            // if (!company.SubscriptionActivity)
            // {
            //     return NotFound(
            //         new BdobrResponse
            //         {
            //             status = ResponseStatus.OfferError,
            //             message = $"У компании '{company.NameOfficial}' не активна подписка."
            //         }
            //     );
            // }

            var offerByCompany = Context.Offer
                .AsNoTracking()
                .Include(o => o.Company)
                .OrderByDescending(o => o.CreateDate)
                .Where(o => o.Company == company)
                .FirstOrDefault();

            // if (offerByCompany != null)
            // {
            //     double durationSeconds = DateTime.UtcNow.Subtract(offerByCompany.CreateDate).TotalSeconds;
            //     TimeSpan seconds = TimeSpan.FromSeconds(durationSeconds);
            //     var offerTimeout = Int32.Parse(_configuration["OfferTimeout"]);

            //     if (seconds.TotalHours < offerTimeout)
            //     {
            //         TimeSpan diffTimeSpan = TimeSpan.FromHours(offerTimeout).Subtract(seconds);
            //         string duration = String.Format(@"{0}:{1:mm\:ss\:fff}", diffTimeSpan.Days * offerTimeout + diffTimeSpan.Hours, diffTimeSpan);
            //         return NotFound(new ErrorResponse
            //         {
            //             status = ErrorStatus.OfferTimeError,
            //             message = $"Компания \"{company.NameOfficial}\" уже публиковала акцию за последние {offerTimeout} часа. " +
            //             $"Осталось {duration} до следующей возможности создать акцию."
            //         });
            //     }
            // }

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

            Utils.CreateOfferNotification($"{company.Name}: {offer.Text}", favoriteCompanies.Select(fc => fc.User.PlayerId).ToArray());

            return Ok(offer);
        }

        [HttpPost("like")]
        public async Task<ActionResult> LikeOffer(LikeRequest request)
        {
            var user = await Context.User
                .SingleOrDefaultAsync(user => user.Id == request.userId);

            if (user == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.OfferError,
                        message = $"Не найден пользователь с id = '{request.userId}'"
                    }
                );
            }

            var offer = await Context.Offer
                .SingleOrDefaultAsync(offer => offer.Id == request.offerId);

            if (offer == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.OfferError,
                        message = $"Не найдена акция с id = '{request.offerId}'"
                    }
                );
            }

            var likes = Context.LikedOffer.Where(lc => lc.UserId == request.userId && lc.OfferId == request.offerId);
            Context.LikedOffer.RemoveRange(likes);

            Context.LikedOffer.Add(new LikedOffer
            {
                User = user,
                Offer = offer
            });

            if (likes.ToList().Count == 0)
            {
                offer.LikeCounter++;
            }
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
