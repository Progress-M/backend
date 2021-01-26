using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Reflection;

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


        [HttpGet("image/{id}")]
        public async Task<ActionResult> GeCompanyAvatar(int id)
        {
            var item = await Context.Company
                .AsNoTracking()
                .SingleOrDefaultAsync(company => company.Id == id);

            if (item == null)
            {
                return NotFound($"Not found company with id = {id}");
            }

            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!System.IO.File.Exists(@$"{filePath}\image\company\{item.AvatarName}"))
            {
                return NotFound($"Not found file with name = '{item.AvatarName}'");
            }

            var stream = System.IO.File.OpenRead(@$"{filePath}\image\company\{item.AvatarName}");
            return new FileStreamResult(stream, "image/jpeg");
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
        [DisableRequestSizeLimit]
        public async Task<ActionResult> CreateCompany([FromForm] CompanyRequest companyRequest)
        {
            var exist = await Context.Company
                .AsNoTracking()
                .SingleOrDefaultAsync(cp => cp.INN == companyRequest.inn || cp.Email == companyRequest.email);

            if (exist != null)
            {
                return BadRequest($"Company with INN = '{companyRequest.inn}' or Email = '{companyRequest.email}' already exist.");
            }

            var productCategory = await Context.ProductCategory
                .SingleOrDefaultAsync(cp => cp.Id == companyRequest.productCategoryId);
            if (productCategory == null)
            {
                return NotFound($"Not found productCategory with id = {companyRequest.productCategoryId}");
            }

            var company = new Company(companyRequest, productCategory);
            Context.Company.Add(company);
            await Context.SaveChangesAsync();

            company.AvatarName = await saveCompanyAvatar(companyRequest.image, company.Id);
            await Context.SaveChangesAsync();

            return Ok(company);
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

        [HttpDelete("cascade/{id}")]
        public async Task<ActionResult> CascadeDeleteCompany(int id)
        {
            var item = await Context.Company.FindAsync(id);
            if (item == null)
            {
                return NotFound(id);
            }

            await deleteOfferByCompany(item);

            Context.Company.Remove(item);
            await Context.SaveChangesAsync();

            return Ok(item);
        }

        public async Task deleteOfferByCompany(Company item)
        {
            var items = await Context.Offer
                .Where(offer => offer.Company.Id == item.Id)
                .ToListAsync();

            for (int i = 0; i < items.Count; i++)
            {
                await deleteOfferUserByUser(items[i]);
            }

            Context.Offer.RemoveRange(items);
            await Context.SaveChangesAsync();
        }

        public async Task deleteOfferUserByUser(Offer offer)
        {
            var items = await Context.OfferUser
                .Where(offer => offer.Offer.Id == offer.Id)
                .ToListAsync();

            Context.OfferUser.RemoveRange(items);
            await Context.SaveChangesAsync();
        }

        public async Task<string> saveCompanyAvatar(IFormFile file, int companyId)
        {
            if (file.Length > 0)
            {
                var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Directory.CreateDirectory($@"{filePath}\image\company\");
                using (var stream = System.IO.File.Create($@"{filePath}\image\company\{companyId}-{file.FileName}"))
                {
                    await file.CopyToAsync(stream);
                }

                return $"{companyId}-{file.FileName}";
            }

            return "";
        }
    }
}
