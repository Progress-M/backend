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
        public IEnumerable<OfferResponse> preOffer { get; set; }
        public IEnumerable<OfferResponse> activeOffer { get; set; }
        public IEnumerable<OfferResponse> nearbyOffer { get; set; }
        public IEnumerable<OfferResponse> inactiveOffer { get; set; }

    }
    public class CreateCompanyResponse
    {
        public System.String status { get; set; }
        public Company company { get; set; }
        public System.String access_token { get; set; }
        public System.String token_type { get; set; }
    }

    public class CreateUserResponse
    {
        public System.String status { get; set; }
        public User user { get; set; }
        public System.String access_token { get; set; }
        public System.String token_type { get; set; }
    }

    static class AuthStatus
    {
        public const string Success = "Success";
        public const string Fail = "Fail";
    }
}