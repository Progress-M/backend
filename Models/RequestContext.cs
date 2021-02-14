using System;
using Microsoft.AspNetCore.Http;

namespace Main.PostgreSQL
{

    public class UserRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public bool isMan { get; set; }
        public DateTime BirthYear { get; set; }
        public IFormFile image { get; set; }
    }

    public class UserUpdateRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public bool isMan { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime BirthYear { get; set; }
        public string PlayerId { get; set; }
    }

    public class UserImageRequest
    {
        public IFormFile image { get; set; }
    }

    public class UserAcceptance
    {
        public string email { get; set; }
        public string code { get; set; }
    }

    public class EmailСonfirmation
    {
        public string email { get; set; }
    }

    public class CompanyRequest
    {
        public string name { get; set; }
        public string representative { get; set; }
        public string email { get; set; }
        public string inn { get; set; }
        public string password { get; set; }
        public string address { get; set; }
        public string timeOfWork { get; set; }
        public int productCategoryId { get; set; }
        public IFormFile image { get; set; }
    }

    public class LikeRequest
    {
        public int offerId { get; set; }
        public int userId { get; set; }
    }

    public class OfferRequest
    {
        public string text { get; set; }
        public DateTime timeStart { get; set; }
        public DateTime timeEnd { get; set; }
        public int companyId { get; set; }
        public IFormFile image { get; set; }
        public int percentage { get; set; }
    }
    public class AuthRequest
    {
        public string username { get; set; }
        public string password { get; set; }
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