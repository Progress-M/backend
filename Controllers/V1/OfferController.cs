using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;
using System;

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
               .SingleOrDefaultAsync(offer => offer.Id == id);

            if (item == null)
            {
                return NotFound(id);
            }

            return Ok(item);
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult> GetLanguages()
        {
            return Ok(await Context.Offer.ToListAsync());
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult> CreateOffer(OfferRequest offerRequest)
        {
            var company = await Context.Company
              .AsNoTracking()
              .SingleOrDefaultAsync(company => company.Id == offerRequest.companyId);

            if (company == null)
            {
                return NotFound(offerRequest.companyId);
            }

            Console.WriteLine(company.Name);
            var Offer = new Offer(offerRequest.text, DateTime.UtcNow, company);

            Console.WriteLine(Offer.Text);

            Context.Offer.Add(Offer);
            await Context.SaveChangesAsync();

            return Ok(Offer);
        }

    }
}
