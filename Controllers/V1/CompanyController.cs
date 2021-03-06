using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Cors;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Main.Function;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

using Main.Models;
using Main.PostgreSQL;
using System;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    // [Authorize(Policy = "ValidAccessToken")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CompanyController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<CompanyController> _logger;
        readonly string subfolder = @"/image/company/";
        public IConfiguration Configuration { get; }

        public CompanyController(KindContext KindContext, ILogger<CompanyController> logger, IConfiguration configuration)
        {
            Context = KindContext;
            _logger = logger;
            Configuration = configuration;
        }

        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> GetCompany(int id)
        {
            var item = await Context.Company
               .AsNoTracking()
               .Include(company => company.ProductCategory)
               .SingleOrDefaultAsync(lang => lang.Id == id);

            if (item == null)
            {
                return NotFound($"Not found comapny with id = {id}");
            }

            return Ok(item);
        }

        [HttpGet("{id}/notification")]
        [Produces("application/json")]
        public async Task<ActionResult> GetCompanyNotification(int id)
        {
            var items = await Context.CompanyNotification
               .AsNoTracking()
               .Where(cn => cn.company.Id == id)
               .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id}/number-of-favorites")]
        [Produces("application/json")]
        public async Task<ActionResult> GetNumberOfAdditionsToTheFavorites(int id)
        {
            var items = await Context.FavoriteCompany
               .AsNoTracking()
               .Where(c => c.Id == id)
               .ToListAsync();

            return Ok(items.Count);
        }

        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<ActionResult> GeCompanyAvatar(int id)
        {
            var item = await Context.Company
                .AsNoTracking()
                .SingleOrDefaultAsync(company => company.Id == id);

            if (item == null)
            {
                return NotFound($"Not found company with id = {id}");
            }

            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                .Replace(Utils.subfolder, "");

            if (!System.IO.File.Exists($"{filePath}{subfolder}{item.AvatarName}"))
            {
                return NotFound($"Not found file with name = '{item.AvatarName}'");
            }

            var stream = System.IO.File.OpenRead($"{filePath}{subfolder}{item.AvatarName}");
            return new FileStreamResult(stream, "image/jpeg");
        }

        [HttpPut("{id}/image")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> UpdateCompanyAvatar(int id, [FromForm] ImageRequest companyRequest)
        {
            var company = await Context.Company
                .SingleOrDefaultAsync(company => company.Id == id);

            if (company == null)
            {
                return NotFound($"Not found company with id = {id}");
            }

            Utils.deleteFile(@"{subfolder}", company.AvatarName);
            company.AvatarName = await Utils.saveFile(companyRequest.image, $"{subfolder}", company.Id);
            Console.WriteLine(company.AvatarName);
            await Context.SaveChangesAsync();

            return Ok();
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
                .ThenInclude(company => company.ProductCategory)
                .Where(offer => offer.Company.Id == id)
                .ToListAsync();

            var preOffer = offers.Where(offer => offer.TimeStart > DateTime.UtcNow);
            var activeOffer = offers.Where(offer => offer.TimeStart < DateTime.UtcNow && offer.TimeEnd > DateTime.UtcNow);
            var inactiveOffer = offers.Where(offer => offer.TimeEnd < DateTime.UtcNow);

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
        public async Task<ActionResult<List<Company>>> GetCompanys()
        {
            return Ok(await Context.Company
                .Include(company => company.ProductCategory)
                .ToListAsync()
            );
        }

        [HttpPost]
        [AllowAnonymous]
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

            company.AvatarName = await Utils.saveFile(companyRequest.image, $"{subfolder}", company.Id);
            await Context.SaveChangesAsync();


            return Ok(
                new CreateCompanyResponse
                {
                    status = AuthStatus.Success,
                    company = company,
                    access_token = Auth.generateToken(Configuration),
                    token_type = "bearer"
                }
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(int id, [FromForm] CompanyRequest company)
        {
            var aliveCompany = await Context.Company.SingleOrDefaultAsync(cp => cp.Id == id);
            if (aliveCompany == null)
            {
                return NotFound($"Not found company with id = {id}");
            }

            var category = await Context.ProductCategory.SingleOrDefaultAsync(cp => cp.Id == company.productCategoryId);
            if (category == null)
            {
                return NotFound($"Not found category with id = {company.productCategoryId}");
            }

            aliveCompany.INN = company.inn;
            aliveCompany.Name = company.name;
            aliveCompany.Email = company.email;
            aliveCompany.Phone = company.phone;
            aliveCompany.Representative = company.representative;
            aliveCompany.Password = company.password;
            aliveCompany.Address = company.address;
            aliveCompany.TimeOfWork = company.timeOfWork;
            aliveCompany.PlayerId = company.playerId;
            aliveCompany.ProductCategory = category;
            aliveCompany.Latitude = company.Latitude;
            aliveCompany.Longitude = company.Longitude;

            if (company.image != null)
            {
                Utils.deleteFile($"{subfolder}", aliveCompany.AvatarName);
                aliveCompany.AvatarName = await Utils.saveFile(company.image, $"{subfolder}", aliveCompany.Id);
            }

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

            await CompanyUtils.deleteOfferByCompany(Context, item);

            Context.Company.Remove(item);
            await Context.SaveChangesAsync();

            return Ok(item);
        }
    }

    public static class CompanyUtils
    {
        public static async Task deleteOfferByCompany(KindContext Context, Company item)
        {
            var items = await Context.Offer
                .Where(offer => offer.Company.Id == item.Id)
                .ToListAsync();

            Context.Offer.RemoveRange(items);
            await Context.SaveChangesAsync();
        }
    }
}
