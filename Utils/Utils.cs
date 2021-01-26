using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Http;

using MailKit.Net.Smtp;
using MimeKit;
using System;

namespace Main.Function
{
    public static class Utils
    {
        public static async Task<string> saveFile(IFormFile file, string path, int userId)
        {
            if (file != null && file.Length > 0)
            {
                var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Directory.CreateDirectory($@"{filePath}{path}");
                using (var stream = System.IO.File.Create($@"{filePath}{path}{userId}-{file.FileName}"))
                {
                    await file.CopyToAsync(stream);
                }

                return $"{userId}-{file.FileName}";
            }

            return "";
        }

        private const string emailServerURL = "smtp.mail.ru";
        private const int emailServerPort = 465;
        private const string emailBdobr = "bedobr@mail.ru";
        private const string passwordBdobr = "AkBOE-rcio14";

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