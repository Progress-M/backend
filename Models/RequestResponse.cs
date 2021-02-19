using System.Collections.Generic;
using Main.PostgreSQL;

namespace Main.Models
{
    public class AuthCompanyResponse
    {
        public Company company { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }

    public class AuthUserResponse
    {
        public User user { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }

    public class OfferByUserResponse
    {
        public IEnumerable<Offer> preOffer { get; set; }
        public IEnumerable<Offer> activeOffer { get; set; }
        public IEnumerable<Offer> inactiveOffer { get; set; }
    }

    static class AuthStatus
    {
        public const string Success = "Success";
        public const string Fail = "Fail";
    }
}