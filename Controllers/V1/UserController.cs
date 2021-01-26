using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;

using System.Linq;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<UserController> _logger;

        public UserController(KindContext KindContext, ILogger<UserController> logger)
        {
            Context = KindContext;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> GetUser(int id)
        {
            var item = await Context.User
               .AsNoTracking()
               .SingleOrDefaultAsync(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            return Ok(item);
        }

        [HttpGet("{id}/offers")]
        [Produces("application/json")]
        public async Task<ActionResult> GetUserOffers(int id)
        {
            var item = await Context.User
               .SingleOrDefaultAsync(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            var offers = Context.OfferUser
                .Where(uo => uo.User == item)
                .Include(uo => uo.User)
                .Include(uo => uo.Offer)
                    .ThenInclude(offer => offer.Company)
                        .ThenInclude(company => company.ProductСategory);

            return Ok(offers);
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult> GetUser()
        {
            return Ok(await Context.User.ToListAsync());
        }

        [HttpGet("image/{id}")]
        public async Task<ActionResult> GetUserAvatar(int id)
        {
            var item = await Context.User
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!System.IO.File.Exists(@$"{filePath}\image\user\{item.AvatarName}"))
            {
                return NotFound($"Not found file with name = '{item.AvatarName}'");
            }

            var stream = System.IO.File.OpenRead(@$"{filePath}\image\user\{item.AvatarName}");
            return new FileStreamResult(stream, "image/jpeg");
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> CreateUser([FromForm] UserRequest user)
        {
            var item = new User(user);
            Context.User.Add(item);
            await Context.SaveChangesAsync();
            item.AvatarName = await saveUserAvatar(user.image, item.Id);
            await Context.SaveChangesAsync();

            return Ok(item);
        }

        public async Task<string> saveUserAvatar(IFormFile file, int userId)
        {
            if (file != null && file.Length > 0)
            {
                var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Directory.CreateDirectory($@"{filePath}\image\user\");
                using (var stream = System.IO.File.Create($@"{filePath}\image\user\{userId}-{file.FileName}"))
                {
                    await file.CopyToAsync(stream);
                }

                return $"{userId}-{file.FileName}";
            }

            return "";
        }

    }
}
