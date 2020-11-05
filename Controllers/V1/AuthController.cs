using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<AuthController> _logger;

        public AuthController(KindContext KindContext, ILogger<AuthController> logger)
        {
            Context = KindContext;
            _logger = logger;
        }

        [HttpPost("company")]
        [Produces("application/json")]
        public async Task<ActionResult> SignInCompany(AuthRequest auth)
        {
            var item = await Context.Company
              .AsNoTracking()
              .SingleOrDefaultAsync(company => company.Email == auth.username);

            if (item == null || item.Password != auth.password)
            {
                return Forbid(auth.username);
            }

            return Ok(item);
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult> SignInUser(AuthRequest auth)
        {
            var item = await Context.User
              .AsNoTracking()
              .SingleOrDefaultAsync(user => user.Email == auth.username);

            if (item == null || item.Password != auth.password)
            {
                return Forbid(auth.username);
            }

            return Ok(item);
        }

    }
}
