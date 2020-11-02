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

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult> SignIn(AuthRequest auth)
        {
            _logger.LogInformation($"Hello world {auth.username} {auth.password}");
            
            var item = await Context.User
              .AsNoTracking()
              .SingleOrDefaultAsync(user => user.Username == auth.username);


            if (item == null || item.Password != auth.password)
            {
                return Forbid(auth.username);
            }

            return Ok(new AuthResponse {
                Id = item.Id,
                Username = item.Username,
                Name = item.Name,
                Surname = item.Surname,
                Latitude = item.Latitude,
                Longitude = item.Longitude
            });
        }

    }
}
