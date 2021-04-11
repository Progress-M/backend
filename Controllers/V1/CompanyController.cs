using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Cors;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Main.Function;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

using Main.Models;
using Main.PostgreSQL;
using System;
using GeoTimeZone;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    [Authorize(Policy = "ValidAccessToken")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CompanyController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<CompanyController> _logger;
        public IConfiguration _configuration { get; }

        public CompanyController(KindContext KindContext, ILogger<CompanyController> logger, IConfiguration Configuration)
        {
            Context = KindContext;
            _logger = logger;
            _configuration = Configuration;
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
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Не найдена компания с id = {id}"
                    }
                );
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

        [HttpGet("top")]
        [Produces("application/json")]
        public ActionResult GetCompanyTop()
        {
            var RESULT_LIMIT = 3;

            var comparer = new CompanyComparer();

            var items = Context.FavoriteCompany
              .AsNoTracking()
              .Include(fc => fc.Company)
              .ToList()
              .GroupBy(fc => fc.Company, comparer)
              .OrderByDescending(fc => fc.ToList().Count)
              .Select(fc => new
              {
                  Company = fc.Key,
                  Count = fc.ToList().Count
              })
              .Take(RESULT_LIMIT);

            return Ok(items);
        }

        [HttpGet("{id}/number-of-favorites")]
        [Produces("application/json")]
        public async Task<ActionResult> GetNumberOfAdditionsToTheFavorites(int id)
        {
            var items = await Context.FavoriteCompany
               .AsNoTracking()
               .Where(c => c.CompanyId == id)
               .ToListAsync();

            return Ok(items.Count);
        }

        [HttpGet("{id}/offer-limit")]
        [Produces("application/json")]
        public async Task<ActionResult> CheckOfferLimit(int companyId)
        {
            var company = await Context.Company
                 .SingleOrDefaultAsync(company => company.Id == companyId);

            if (company == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Не найдена компания с id = '{companyId}'"
                    }
                );
            }

            var lastOffer = Context.Offer
                .AsNoTracking()
                .Include(o => o.Company)
                .OrderByDescending(o => o.CreateDate)
                .Where(o => o.Company == company)
                .FirstOrDefault();

            if (lastOffer != null)
            {
                double durationSeconds = DateTime.UtcNow.Subtract(lastOffer.CreateDate).TotalSeconds;
                TimeSpan seconds = TimeSpan.FromSeconds(durationSeconds);
                var offerTimeout = Int32.Parse(_configuration["OfferTimeout"]);

                if (seconds.TotalHours < offerTimeout)
                {
                    TimeSpan diffTimeSpan = TimeSpan.FromHours(offerTimeout).Subtract(seconds);
                    string duration = String.Format(@"{0}:{1:mm\:ss\:fff}", diffTimeSpan.Days * offerTimeout + diffTimeSpan.Hours, diffTimeSpan);
                    return BadRequest(new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Компания \"{company.NameOfficial}\" уже публиковала акцию за последние {offerTimeout} часа. " +
                        $"Осталось {duration} до следующей возможности создать акцию."
                    });
                }
            }

            return Ok(new BdobrResponse
            {
                status = ResponseStatus.Success,
                message = ""
            });
        }

        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<ActionResult> GeCompanyAvatar(int id)
        {
            var item = await Context.Company
                .AsNoTracking()
                .Include(c => c.Image)
                .SingleOrDefaultAsync(company => company.Id == id);

            if (item == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Не найдена компания с id = {id}"
                    }
                );
            }

            if (item.Image == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Не найден аватар компании с id = {id}"
                    }
                );
            }

            return File(item.Image.bytes, "image/png");
        }

        [HttpPut("{id}/image")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> UpdateCompanyAvatar(int id, [FromForm] ImageRequest imageRequest)
        {
            var company = await Context.Company
                .Include(c => c.Image)
                .SingleOrDefaultAsync(company => company.Id == id);

            if (company == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Не найдена компания с id = {id}"
                    }
                );
            }

            if (imageRequest.image != null)
            {
                if (company.Image != null)
                {
                    Context.Files.Remove(company.Image);
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    imageRequest.image.CopyTo(ms);

                    var file = new FileData { bytes = ms.ToArray() };
                    Context.Files.Add(file);
                    await Context.SaveChangesAsync();

                    company.Image = file;
                    await Context.SaveChangesAsync();
                }
            }

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
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Не найдена компания с id = {id}"
                    }
                );
            }

            var offers = Context.Offer
                .Include(offer => offer.Company)
                .ThenInclude(company => company.ProductCategory)
                .Where(offer => offer.Company.Id == id)
                .OrderByDescending(it => it.CreateDate)
                .Select(offer => new OfferResponse(offer, false))
                .ToList();

            var groups = OfferUtils.GroupByRelevance(offers);

            return Ok(
                new OfferByUserResponse
                {
                    activeOffer = groups.preOffer.Concat(groups.activeOffer),
                    inactiveOffer = groups.inactiveOffer
                }
            );
        }

        [HttpGet("category/{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> GetOffersByCategory(int id)
        {
            var category = await Context.ProductCategory.SingleOrDefaultAsync(c => c.Id == id);

            return Ok(
               await Context.Company
                    .Include(company => company.ProductCategory)
                    .Where(company => company.ProductCategory.Id == id)
                    .ToListAsync()
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
                return BadRequest(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Комания с ИНН = '{companyRequest.inn}' и/или email = '{companyRequest.email}' уже существует."
                    }
                );
            }

            var productCategory = await Context.ProductCategory
                .SingleOrDefaultAsync(cp => cp.Id == companyRequest.productCategoryId);
            if (productCategory == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Не найдена категория с id = '{companyRequest.productCategoryId}'"
                    }
                );
            }

            string tz = TimeZoneLookup.GetTimeZone(companyRequest.Latitude, companyRequest.Longitude).Result;  // "Europe/London"

            var company = new Company(companyRequest, productCategory, tz);
            company.EmailConfirmed = true;

            if (companyRequest.image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    companyRequest.image.CopyTo(ms);

                    var file = new FileData { bytes = ms.ToArray() };
                    Context.Files.Add(file);
                    await Context.SaveChangesAsync();
                    company.Image = file;
                }
            }
            Context.Company.Add(company);
            await Context.SaveChangesAsync();

            return Ok(
                new CreateCompanyResponse
                {
                    status = AuthStatus.Success,
                    company = company,
                    access_token = Auth.generateToken(_configuration),
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
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Не найдена компания с id = '{id}'"
                    }
                );
            }

            var category = await Context.ProductCategory.SingleOrDefaultAsync(cp => cp.Id == company.productCategoryId);
            if (category == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Не найдена категория с id = '{company.productCategoryId}'"
                    }
                );
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
                if (aliveCompany.Image != null)
                {
                    Context.Files.Remove(aliveCompany.Image);
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    company.image.CopyTo(ms);

                    var file = new FileData { bytes = ms.ToArray() };
                    Context.Files.Add(file);
                    await Context.SaveChangesAsync();

                    aliveCompany.Image = file;
                    await Context.SaveChangesAsync();
                }
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
