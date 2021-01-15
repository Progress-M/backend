using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;

using System.Linq;

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

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult> CreateUser(UserRequest user)
        {
            Context.User.Add(new User(user));
            await Context.SaveChangesAsync();
            return Ok(user);
        }

    }
}
