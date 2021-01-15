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
        private const string emailServerURL = "smtp.mail.ru";
        private const int emailServerPort = 465;
        private const string emailBdobr = "bedobr@mail.ru";
        private const string passwordBdobr = "AkBOE-rcio14";

        public RegistrationController(KindContext KindContext, ILogger<AuthController> logger)
        {
            Context = KindContext;
            _logger = logger;
        }


        [HttpGet("email-acceptance/company/{id}/{code}")]
        [Produces("application/json")]
        public async Task<ActionResult> CompanyAcceptance(int id, string code)
        {
            var item = await Context.Company
               .SingleOrDefaultAsync(company => company.Id == id);

            if (item == null)
            {
                return NotFound($"Not found comapny with id = {id}");
            }

            var ce = await Context.CompanyEmailCode
                .SingleOrDefaultAsync(ce => ce.company.Id == id);

            if (ce == null)
            {
                return NotFound($"Not found email-confirmation to company with id = {id}");
            }

            if (ce.code != code)
            {
                return BadRequest($"Incorrect code");
            }

            Context.CompanyEmailCode.Remove(ce);
            item.EmailConfirmed = true;
            await Context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("email-acceptance/user/{id}/{code}")]
        [Produces("application/json")]
        public async Task<ActionResult> UserAcceptance(int id, string code)
        {
            var item = await Context.User
               .SingleOrDefaultAsync(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }

            var ue = await Context.UserEmailCode
                .SingleOrDefaultAsync(ue => ue.user.Id == id);

            if (ue == null)
            {
                return NotFound($"Not found email-confirmation to user with id = {id}");
            }

            if (ue.code != code)
            {
                return BadRequest($"Incorrect code");
            }

            Context.UserEmailCode.Remove(ue);
            item.EmailConfirmed = true;
            await Context.SaveChangesAsync();

            return Ok();
        }


        [HttpGet("email-confirmation/company/{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> CompanyEmailСonfirmation(int id)
        {
            var item = await Context.Company
               .SingleOrDefaultAsync(company => company.Id == id);

            if (item == null)
            {
                return NotFound($"Not found comapny with id = {id}");
            }
            var code = RandomCode();
            MimeMessage message = BuildMessageСonfirmation(item.Name, item.Email, code);
            SendEmail(message);

            foreach (var ce in Context.CompanyEmailCode.Where(ce => ce.company == item))
            {
                Context.CompanyEmailCode.Remove(ce);
            }

            Context.CompanyEmailCode.Add(new CompanyEmailCode
            {
                code = code,
                company = item
            });
            await Context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("email-confirmation/user/{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> UserEmailСonfirmation(int id)
        {
            var item = await Context.User
               .SingleOrDefaultAsync(user => user.Id == id);

            if (item == null)
            {
                return NotFound($"Not found user with id = {id}");
            }
            var code = RandomCode();
            MimeMessage message = BuildMessageСonfirmation(item.Name, item.Email, code);
            SendEmail(message);

            foreach (var ue in Context.UserEmailCode.Where(ue => ue.user == item))
            {
                Context.UserEmailCode.Remove(ue);
            }

            Context.UserEmailCode.Add(new UserEmailCode
            {
                code = code,
                user = item
            });
            await Context.SaveChangesAsync();

            return Ok();
        }

        public string RandomCode()
        {
            const int min = 1000;
            const int max = 9999;
            Random _random = new Random();
            return _random.Next(min, max).ToString();
        }

        public MimeMessage BuildMessageСonfirmation(string name, string email, string code)
        {
            MimeMessage message = new MimeMessage();

            MailboxAddress from = new MailboxAddress("Будьдобр", emailBdobr);
            message.From.Add(from);

            MailboxAddress to = new MailboxAddress(name, email);
            message.To.Add(to);

            message.Subject = "Добро пожаловать в Будьдобр";

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $"<div>Приветсвуем в сообществе Будьдобр, {name}. Код подтверждения: {code}</div>";
            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        public void SendEmail(MimeMessage message)
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
