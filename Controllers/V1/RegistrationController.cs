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
using System;
using Microsoft.Extensions.Configuration;

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
        readonly IConfiguration _configuration;

        public RegistrationController(KindContext KindContext, ILogger<AuthController> logger, IConfiguration Configuration)
        {
            Context = KindContext;
            _logger = logger;
            _configuration = Configuration;
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
                    new BdobrResponse
                    {
                        status = ResponseStatus.RegistrationError,
                        message = $"Не найден email = {acceptance.email} или некорректный код."
                    }
                );
            }
            double durationSeconds = DateTime.UtcNow.Subtract(ue.createdDateTime).TotalSeconds;
            TimeSpan seconds = TimeSpan.FromSeconds(durationSeconds);
            var EmailCodeTimeLife = Int32.Parse(_configuration["EmailCodeTimeLife"]);
            if (seconds.TotalMinutes > EmailCodeTimeLife)
            {
                return BadRequest(
                    new BdobrResponse
                    {
                        status = ResponseStatus.RegistrationError,
                        message = $"Время жизни кода истекло."
                    }
                );
            }

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
