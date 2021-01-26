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

    public class OfferRequest
    {
        public string text { get; set; }
        public DateTime timeStart { get; set; }
        public DateTime timeEnd { get; set; }
        public int companyId { get; set; }
        public IFormFile image { get; set; }
    }
    public class AuthRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}