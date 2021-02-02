using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Net;
using System.Text;

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

        public static void CreateNotification(string message)
        {
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.Headers.Add("authorization", "Basic MWY2MWY4ODctMjU5NC00ZTZjLTgyMWYtZGEzM2M5NmNhYTJh");

            var obj = new
            {
                app_id = "ae165b6f-ed06-4a28-aab6-37e7a96f9e68",
                contents = new { en = "English Message", ru = message },
                channel_for_external_user_ids = "push",
                included_segments = new string[] { "Subscribed Users" }
            };

            var param = JsonConvert.SerializeObject(obj, Formatting.Indented);
            byte[] byteArray = Encoding.UTF8.GetBytes(param);

            string responseContent = null;

            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
            }

            System.Diagnostics.Debug.WriteLine(responseContent);
        }
    }
}