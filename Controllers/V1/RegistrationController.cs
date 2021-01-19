using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Main.PostgreSQL;
using Microsoft.AspNetCore.Cors;

using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

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

    public static class Utils
    {

        private const string emailServerURL = "smtp.mail.ru";
        private const int emailServerPort = 465;
        private const string emailBdobr = "bdobr-test@mail.ru";
        private const string passwordBdobr = "POty2TUuu4a*";

        public static string RandomCode()
        {
            const int min = 1000;
            const int max = 9999;
            Random _random = new Random();
            return _random.Next(min, max).ToString();
        }

        public static MimeMessage BuildMessageСonfirmation(string email, string code)
        {
            MimeMessage message = new MimeMessage();

            MailboxAddress from = new MailboxAddress("Будьдобр", emailBdobr);
            message.From.Add(from);

            MailboxAddress to = new MailboxAddress("Пользователь", email);
            message.To.Add(to);

            message.Subject = "Добро пожаловать в Будьдобр";

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $"<div>Приветсвуем в сообществе Будьдобр. Код подтверждения: {code}</div>";
            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        public static void SendEmail(MimeMessage message)
        {
            using (SmtpClient client = new SmtpClient())
            {
                client.Connect(emailServerURL, emailServerPort, true);
                client.Authenticate(emailBdobr, passwordBdobr);

                client.Send(message);
                client.Disconnect(true);
                client.Dispose();
            }
        }
    }
}
