using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;
using System.Linq;
using System.Collections.Generic;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CompanyController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<CompanyController> _logger;

        public CompanyController(KindContext KindContext, ILogger<CompanyController> logger)
        {
            Context = KindContext;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> GetCompany(int id)
        {
            var item = await Context.Company
               .AsNoTracking()
               .Include(company => company.ProductСategory)
               .SingleOrDefaultAsync(lang => lang.Id == id);

            if (item == null)
            {
                return NotFound($"Not found comapny with id = {id}");
            }

            return Ok(item);
        }

        [HttpGet("{id}/offer")]
        [Produces("application/json")]
        public async Task<ActionResult<System.Collections.Generic.List<Offer>>> GetOfferByCompany(int id)
        {
            var item = await Context.Company
               .AsNoTracking()
               .SingleOrDefaultAsync(company => company.Id == id);

            if (item == null)
            {
                return NotFound($"Not found comapny with id = {id}");
            }

            var offers = await Context.Offer
                .Include(offer => offer.Company)
                .ThenInclude(company => company.ProductСategory)
                .Where(offer => offer.Company.Id == id)
                .ToListAsync();

            return Ok(offers);
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<List<Company>>> GetCompanys()
        {
            return Ok(await Context.Company
                .Include(company => company.ProductСategory)
                .ToListAsync()
            );
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult> CreateCompany(CompanyRequest companyRequest)
        {
            var old = await Context.Company
                .AsNoTracking()
                .SingleOrDefaultAsync(cp => cp.INN == companyRequest.inn || cp.Email == companyRequest.email);

            if (old != null)
            {
                return BadRequest($"Company with INN = '{companyRequest.inn}' or Email = '{companyRequest.email}' already exist.");
            }

            var productCategory = await Context.ProductCategory
                .SingleOrDefaultAsync(cp => cp.Id == companyRequest.productCategoryId);
            if (productCategory == null)
            {
                return NotFound($"Not found productCategory with id = {companyRequest.productCategoryId}");
            }

            Context.Company.Add(new Company(companyRequest, productCategory));
            await Context.SaveChangesAsync();

            var item = await Context.Company
                .AsNoTracking()
                .SingleOrDefaultAsync(cp => cp.INN == companyRequest.inn);

            if (item == null)
            {
                return BadRequest(companyRequest);
            }

            return Ok(item);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCompany(Company company)
        {
            var aliveCompany = await Context.Company.SingleOrDefaultAsync(cp => cp.Id == company.Id);
            if (aliveCompany == null)
            {
                return NotFound(aliveCompany);
            }

            aliveCompany.Name = aliveCompany.Name;
            await Context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCompany(int id)
        {
            var item = await Context.Company.FindAsync(id);
            if (item == null)
            {
                return NotFound(id);
            }

            Context.Company.Remove(item);
            await Context.SaveChangesAsync();

            return Ok(item);
        }
    }
}
