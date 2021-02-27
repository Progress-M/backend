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
using System.Collections.Generic;
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
                    .ThenInclude(company => company.ProductСategory)
               .SingleOrDefaultAsync(offer => offer.Id == id);

            if (item == null)
            {
                return NotFound($"Not found offer with id = {id}");
            }

            return Ok(item);
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult> GetOffers()
        {
            return Ok(
                (await Context.Offer
                    .Include(offer => offer.Company)
                        .ThenInclude(company => company.ProductСategory)
                    .ToListAsync())
                    .Where(offer => offer.SendingTime >= DateTime.UtcNow)
                    .OrderBy(order => order.TimeStart)
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
                        .ThenInclude(company => company.ProductСategory)
                    .Where(offer => offer.Company.ProductСategory == category)
                    .ToListAsync())
                    .Where(offer => offer.SendingTime >= DateTime.UtcNow)
                    .OrderBy(order => order.TimeStart)
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

            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!System.IO.File.Exists(@$"{filePath}\image\offer\{item.ImageName}"))
            {
                return NotFound($"Not found file with name = '{item.ImageName}'");
            }

            var stream = System.IO.File.OpenRead(@$"{filePath}\image\offer\{item.ImageName}");
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

            Utils.deleteFile(@"\image\offer\", item.ImageName);
            item.ImageName = await Utils.saveFile(imageRequest.image, @"\image\offer\", item.Id);
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

            var users = (await Context.User
                .Include(u => u.Favorites)
                .ToListAsync())
                .Where(u => u.Favorites != null && u.Favorites.Contains(company) && u.PlayerId != null);

            var offer = new Offer(offerRequest, company);
            await Context.Offer.AddAsync(offer);

            await Context.SaveChangesAsync();
            if (offerRequest.image != null)
            {
                offer.ImageName = await Utils.saveFile(offerRequest.image, @"\image\offer\", offer.Id);
                await Context.SaveChangesAsync();
            }

            Utils.CreateNotificationToFavorites(offer.Text, users.Select(u => u.PlayerId).ToArray());

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

            if (user.LikedPosts == null)
            {
                user.LikedPosts = new HashSet<Offer>();
            }

            offer.LikeCounter++;
            user.LikedPosts.Add(offer);
            await Context.SaveChangesAsync();

            return Ok(offer);
        }

        [HttpPost("dislike")]
        public async Task<ActionResult> DislikeOffer(LikeRequest request)
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

            if (user.LikedPosts == null)
            {
                user.LikedPosts = new HashSet<Offer>();
            }

            offer.LikeCounter--;
            user.LikedPosts.Remove(offer);
            await Context.SaveChangesAsync();

            return Ok(offer);
        }
    }
}
