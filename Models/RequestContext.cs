using System;
using Microsoft.AspNetCore.Http;

namespace Main.PostgreSQL
{

    public class UserRequest
    {
        public string Name { get; set; }
        public bool isMan { get; set; }
        public string playerId { get; set; }
        public DateTime BirthYear { get; set; }
        public IFormFile image { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class UserImageRequest
    {
        public IFormFile image { get; set; }
    }

    public class EmailAcceptance
    {
        public string email { get; set; }
        public string code { get; set; }
    }

    public class EmailRequest
    {
        public string email { get; set; }
    }
    public class PasswordRestoreRequest
    {
        public string email { get; set; }
        public string newPassword { get; set; }
        public string code { get; set; }
    }

    public class ProductCategoryRequest
    {
        public string name { get; set; }
        public int ageLimit { get; set; } = 0;
        public int priority { get; set; } = 1;
        public IFormFile image { get; set; }
    }

    public class CompanyRequest
    {
        public int id { get; set; }
        public string name { get; set; }
        public string nameOfficial { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool SubscriptionActivity { get; set; } = true;
        public string representative { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string inn { get; set; }
        public string password { get; set; }
        public string address { get; set; }
        public DateTime timeOpen { get; set; }
        public DateTime timeClose { get; set; }
        public int productCategoryId { get; set; }
        public string playerId { get; set; }
        public IFormFile image { get; set; }
    }

    public class ImageRequest
    {
        public IFormFile image { get; set; }
    }

    public class LikeRequest
    {
        public int offerId { get; set; }
        public int userId { get; set; }
    }

    public class ChatMessage
    {
        public string text { get; set; }
        public int companyId { get; set; }
        public int userId { get; set; }
        public bool isUserMessage { get; set; }

    }

    public class OfferRequest
    {
        public string text { get; set; }
        public string timeRange { get; set; }
        public DateTime sendingTime { get; set; }
        public DateTime dateStart { get; set; }
        public DateTime dateEnd { get; set; }
        public DateTime? timeStart { get; set; }
        public DateTime? timeEnd { get; set; }
        public int companyId { get; set; }
        public IFormFile image { get; set; }
        public int percentage { get; set; }
        public bool forMan { get; set; }
        public bool forWoman { get; set; }
        public int UpperAgeLimit { get; set; }
        public int LowerAgeLimit { get; set; }
    }
    public class AuthRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class AuthPinCodeRequest
    {
        public string playerId { get; set; }
        public string pincode { get; set; }
    }

    public class PlayerIdRequest
    {
        public string playerId { get; set; }
    }

    public class FavoritesRequest
    {
        public string playerId { get; set; }
    }
}