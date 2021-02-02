using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;

using Microsoft.AspNetCore.Http;
using System.IO;
using System.Reflection;
using Main.Function;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
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
            return Ok(await Context.Offer
                .Include(offer => offer.Company)
                    .ThenInclude(company => company.ProductСategory)
                .ToListAsync());
        }

        [HttpGet("image/{id}")]
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

            var users = await Context.User.ToListAsync();

            var offer = new Offer(offerRequest, company);
            await Context.Offer.AddAsync(offer);

            users
                .ConvertAll(user => new OfferUser(offer, user))
                .ForEach(ou => Context.OfferUser.Add(ou));

            await Context.SaveChangesAsync();
            offer.ImageName = await Utils.saveFile(offerRequest.image, @"\image\offer\", offer.Id);
            await Context.SaveChangesAsync();

            Utils.CreateNotification(offer.Text);

            return Ok(offer);
        }
    }
}
