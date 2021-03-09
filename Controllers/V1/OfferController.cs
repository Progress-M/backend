using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;

using System.IO;
using System.Reflection;
using Main.Function;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System;

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
        readonly string subfolder = @"/image/offer/";
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
        public async Task<ActionResult> GetOfferTop()
        {
            var RESULT_LIMIT = 3;
            var item = await Context.Offer
                .AsNoTracking()
                .Include(offer => offer.Company)
                    .ThenInclude(company => company.ProductCategory)
               .Where(offer => offer.LikeCounter > 0)
               .OrderByDescending(offer => offer.LikeCounter)
               .Take(RESULT_LIMIT)
               .ToListAsync();

            return Ok(item);
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
                .SingleOrDefaultAsync(offer => offer.Id == id);

            if (item == null)
            {
                return NotFound($"Not found offer with id = {id}");
            }

            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                .Replace(Utils.subfolder, "");

            if (!System.IO.File.Exists($"{filePath}{subfolder}{item.ImageName}"))
            {
                return NotFound($"Not found file with name = '{item.ImageName}'");
            }

            var stream = System.IO.File.OpenRead($"{filePath}{subfolder}{item.ImageName}");
            return new FileStreamResult(stream, "image/jpeg");
        }

        [HttpPut("{id}/image")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> UpdateOfferImage(int id, [FromForm] ImageRequest imageRequest)
        {
            var item = await Context.Offer
                .SingleOrDefaultAsync(offer => offer.Id == id);

            if (item == null)
            {
                return NotFound($"Not found offer with id = {id}");
            }

            Utils.deleteFile($"{subfolder}", item.ImageName);
            item.ImageName = await Utils.saveFile(imageRequest.image, $"{subfolder}", item.Id);
            Console.WriteLine(item.ImageName);
            await Context.SaveChangesAsync();

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

            var favoriteCompanies = await Context.FavoriteCompany
                 .Include(fc => fc.Company)
                 .Include(fc => fc.User)
                    .ThenInclude(u => u.Stories)
                 .Where(fc => fc.CompanyId == company.Id && fc.User.PlayerId != null)
                 .ToListAsync();

            var offer = new Offer(offerRequest, company);
            await Context.Offer.AddAsync(offer);
            await Context.SaveChangesAsync();

            if (offerRequest.image != null)
            {
                offer.ImageName = await Utils.saveFile(offerRequest.image, $"{subfolder}", offer.Id);
            }

            favoriteCompanies.ForEach(fc => fc.User.Stories.Add(offer));
            await Context.SaveChangesAsync();

            Utils.CreateNotificationToFavorites(offer.Text, favoriteCompanies.Select(fc => fc.User.PlayerId).ToArray());

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
