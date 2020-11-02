using System;

namespace Main.PostgreSQL
{
    public class CompanyRequest
    {
        public string name { get; set; }
    }

    public class OfferRequest
    {
        public string text { get; set; }
        public DateTime timeStart { get; set; }
        public DateTime timeEnd { get; set; }
        public int companyId { get; set; }
        public int[] usersId { get; set; }
    }
    public class AuthRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class AuthResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}