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

        [HttpPost("email-acceptance")]
        [Produces("application/json")]
        public async Task<ActionResult> UserAcceptance(UserAcceptance acceptance)
        {
            var ue = await Context.EmailCode
                .SingleOrDefaultAsync(ue => ue.email == acceptance.email);

            if (ue == null)
            {
                return NotFound($"Not found email-confirmation to user with email = {acceptance.email}");
            }

            if (ue.code != acceptance.code)
            {
                return BadRequest($"Incorrect code");
            }

            Context.EmailCode.Remove(ue);
            await Context.SaveChangesAsync();

            return Ok();
        }


        [HttpPost("email-confirmation")]
        [Produces("application/json")]
        public async Task<ActionResult> EmailСonfirmation(EmailСonfirmation сonfirmation)
        {
            var code = Utils.RandomCode();
            MimeMessage message = Utils.BuildMessageСonfirmation(сonfirmation.email, code);
            Utils.SendEmail(message);

            foreach (var ce in Context.EmailCode.Where(ce => ce.email == сonfirmation.email))
            {
                Context.EmailCode.Remove(ce);
            }

            Context.EmailCode.Add(new EmailCode
            {
                code = code,
                email = сonfirmation.email
            });
            await Context.SaveChangesAsync();

            return Ok();
        }
    }

}
