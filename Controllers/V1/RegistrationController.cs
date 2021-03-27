using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;

using MimeKit;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Main.Function;
using Main.Models;

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
        public async Task<ActionResult> UserAcceptance(EmailAcceptance acceptance)
        {
            var ue = await Context.EmailCode
                .SingleOrDefaultAsync(ue => ue.email == acceptance.email);

            if (ue == null || ue.code != acceptance.code)
            {
                return NotFound(
                    new ErrorResponse
                    {
                        status = ErrorStatus.RegistrationError,
                        message = $"Не найден email = {acceptance.email} или некорректный код."
                    }
                );
            }

            var company = await Context.Company
               .SingleOrDefaultAsync(c => c.Email == acceptance.email);

            if (company == null)
            {
                return BadRequest(new ErrorResponse
                {
                    status = ErrorStatus.SignUpError,
                    message = $"Компания с id = '{acceptance.email}' не найдена"
                });
            }

            company.EmailConfirmed = true;
            Context.EmailCode.Remove(ue);
            await Context.SaveChangesAsync();

            return Ok();
        }


        [HttpPost("email-confirmation")]
        [Produces("application/json")]
        public async Task<ActionResult> EmailСonfirmation(EmailRequest сonfirmation)
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
