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
using System.Collections.Generic;
using Main.PostgreSQL;

namespace Main.Function
{
    public static class Utils
    {
        public static async Task<string> saveFile(IFormFile file, string path, int userId)
        {
            if (file != null && file.Length > 0)
            {
                var filePath = Path
                    .GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                    .Replace(@"/bin/Debug/netcoreapp5.0", "");
                Directory.CreateDirectory($@"{filePath}{path}");

                using (var stream = System.IO.File.Create($@"{filePath}{path}{userId}-{file.FileName}"))
                {
                    await file.CopyToAsync(stream);
                }

                return $"{userId}-{file.FileName}";
            }

            return "";
        }

        public static void deleteFile(string path, string fileName)
        {
            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.CreateDirectory($@"{filePath}{path}");
            if (File.Exists($@"{filePath}{path}{fileName}"))
            {
                File.Delete($@"{filePath}{path}{fileName}");
            }
        }

        private const string emailServerURL = "smtp.mail.ru";
        private const int emailServerPort = 465;
        private const string emailBdobr = "no-reply@bdobr.ru";
        private const string passwordBdobr = "uT1r9y(siARI";

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

            message.Subject = "[Будьдобр] Пожалуйста, подтвердите свой адрес электронной почты";

            var filePath = Path
                .GetDirectoryName(Assembly.GetCallingAssembly().Location)
                .Replace(@"/bin/Debug/netcoreapp5.0", "");
            var body = System.IO.File.ReadAllText($"{filePath}/EmailTemplates/confirm.html").Replace("CONFIRM_CODE", code);

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = body;
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
                headings = new { en = "Bdobr", ru = "Будьдобр" },
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

        public static void CreateNotificationToFavorites(string message, string[] favorites)
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
                headings = new { en = "Bdobr", ru = "Будьдобр" },
                channel_for_external_user_ids = "push",
                include_player_ids = favorites
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

    public class CompanyComparer : IEqualityComparer<Company>
    {
        public bool Equals(Company x, Company y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Company x)
        {
            return x.Id.GetHashCode();
        }
    }

    public class UserComparer : IEqualityComparer<User>
    {
        public bool Equals(User x, User y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(User x)
        {
            return x.Id.GetHashCode();
        }
    }
}