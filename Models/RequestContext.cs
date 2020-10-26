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
        public int companyId { get; set; }
    }
}