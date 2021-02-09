using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;

using MimeKit;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Main.Function;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("OpenPolicy")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RegistrationController : Controller
    {
        readonly KindContext Context;
        readonly ILogger<AuthController> _logger;

        public RegistrationController(KindContext KindContext, ILogger<AuthController> logger)
        {
            Context = KindContext;
            _logger = logger;
        }

        [HttpGet("email-acceptance/{email}/{code}")]
        [Produces("application/json")]
        public async Task<ActionResult> UserAcceptance(string email, string code)
        {
            var ue = await Context.EmailCode
                .SingleOrDefaultAsync(ue => ue.email == email);

            if (ue == null)
            {
                return NotFound($"Not found email-confirmation to user with email = {email}");
            }

            if (ue.code != code)
            {
                return BadRequest($"Incorrect code");
            }

            Context.EmailCode.Remove(ue);
            await Context.SaveChangesAsync();

            return Ok();
        }


        [HttpGet("email-confirmation/{email}")]
        [Produces("application/json")]
        public async Task<ActionResult> EmailСonfirmation(string email)
        {
            var code = Utils.RandomCode();
            MimeMessage message = Utils.BuildMessageСonfirmation(email, code);
            Utils.SendEmail(message);

            foreach (var ce in Context.EmailCode.Where(ce => ce.email == email))
            {
                Context.EmailCode.Remove(ce);
            }

            Context.EmailCode.Add(new EmailCode
            {
                code = code,
                email = email
            });
            await Context.SaveChangesAsync();

            return Ok();
        }
    }

}
