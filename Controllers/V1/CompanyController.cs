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

        [HttpGet("{playerId}/haspincode")]
        [Produces("application/json")]
        [AllowAnonymous]
        public ActionResult GetCompanyHasPinCode(string playerId)
        {
            var company = Context.Company
               .AsNoTracking()
               .SingleOrDefault(company => company.PlayerId == playerId);

            if (company == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Не найдена компания с playerId = {playerId}"
                    });
            }

            return Ok(new BdobrResponse
            {
                status = ResponseStatus.Success,
                haspincode = company.PinCode != null
            });
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

        [HttpGet("new/user/{userId}")]
        [Produces("application/json")]
        public ActionResult GetNewCompanyList(int userId)
        {
            var RESULT_LIMIT = 3;

            var user = Context.User.SingleOrDefault(user => user.Id == userId);

            if (user == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.UserError,
                        message = $"Не найден пользватель с id = '{userId}'"
                    }
                );
            }

            var items = Context.Company
              .AsNoTracking()
            //   .Where(company => Utils.CalculateDistance(
            //     new Location { Latitude = company.Latitude, Longitude = company.Longitude },
            //     new Location { Latitude = user.Latitude, Longitude = user.Longitude }
            //     ) < 40000)
              .OrderByDescending(fc => fc.RegistrationDate)
              .Take(RESULT_LIMIT)
              .ToList();

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
        public async Task<ActionResult> CheckOfferLimit(int id)
        {
            var company = await Context.Company
                 .SingleOrDefaultAsync(company => company.Id == id);

            if (company == null)
            {
                return NotFound(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Не найдена компания с id = '{id}'"
                    }
                );
            }

            var lastOffer = Context.Offer
                .AsNoTracking()
                .Include(o => o.Company)
                .OrderByDescending(o => o.CreateDate)
                .Where(o => o.Company == company)
                .FirstOrDefault();

            if (lastOffer == null)
            {
                return Ok(new BdobrResponse
                {
                    status = ResponseStatus.Success,
                    message = ""
                });
            }

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(lastOffer.Company.TimeZone);
            var dateTimeTZ = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            var CreateDateTZ = TimeZoneInfo.ConvertTimeFromUtc(lastOffer.CreateDate, timeZone);

            if (lastOffer != null && CreateDateTZ.Date == dateTimeTZ.Date)
            {
                return BadRequest(new BdobrResponse
                {
                    status = ResponseStatus.CompanyError,
                    message = $"Компания {company.NameOfficial} уже публиковала акцию сегодня. В день можно отправлять только одну акцию. Следующую акцию Вы сможете создать и отправить завтра."
                });
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
            if (companyRequest.image == null)
            {
                return BadRequest(
                   new BdobrResponse
                   {
                       status = ResponseStatus.CompanyError,
                       message = $"При регистрации компании необходимо загрузить логотип."
                   }
               );
            }

            var exist = await Context.Company
                .AsNoTracking()
                .SingleOrDefaultAsync(cp => cp.INN == companyRequest.inn || cp.Email == companyRequest.email);

            if (exist != null)
            {
                return BadRequest(
                    new BdobrResponse
                    {
                        status = ResponseStatus.CompanyError,
                        message = $"Компания с ИНН = '{companyRequest.inn}' и/или email = '{companyRequest.email}' уже существует."
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

            using (MemoryStream ms = new MemoryStream())
            {
                companyRequest.image.CopyTo(ms);

                var file = new FileData { bytes = ms.ToArray() };
                Context.Files.Add(file);
                await Context.SaveChangesAsync();
                company.Image = file;
            }

            Context.Company.Add(company);
            await Context.SaveChangesAsync();

            Context.CompanyNotification.Add(
                new CompanyNotification(
                    company,
                    "Добро пожаловать",
                    "Добро пожаловать в сервис «БудьДобр.Бизнес»! Ежедневно создавайте собственные креативные предложения и следите за новостями развития сервиса. Мы предоставляем круглосуточную техническую поддержку, если у вас возникли вопросы пишите нам на support@buddobr.ru и мы обязательно вам ответим"
                )
            );
            Context.SaveChanges();

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
            aliveCompany.SubscriptionActivity = company.SubscriptionActivity;
            aliveCompany.Representative = company.representative;
            aliveCompany.Password = company.password;
            aliveCompany.Address = company.address;
            aliveCompany.TimeOpen = company.timeOpen;
            aliveCompany.TimeClose = company.timeClose;
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

            Context.CompanyNotification
                .RemoveRange(Context.CompanyNotification
                    .Where(cn => cn.company == item)
                    .ToList()
                );

            Context.SaveChanges();
            Context.Company.Remove(item);
            Context.SaveChanges();

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

            Context.CompanyNotification
                .RemoveRange(Context.CompanyNotification
                    .Where(cn => cn.company == item)
                    .ToList()
                );

            Context.SaveChanges();


            Context.Message.RemoveRange(Context.Message
               .Where(message => message.company.Id == id)
               .ToList());
            await Context.SaveChangesAsync();

            Context.Offer.RemoveRange(Context.Offer
                .Where(offer => offer.Company.Id == id)
                .ToList());
            await Context.SaveChangesAsync();

            Context.Stories.RemoveRange(
                Context.Stories
                .Include(s => s.Offer)
                .ThenInclude(o => o.Company)
                .Where(story => story.Offer.Company.Id == id)
                .ToList());
            await Context.SaveChangesAsync();

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
