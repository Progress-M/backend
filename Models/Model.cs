
using System;
using System.Collections.Generic;
using Main.PostgreSQL;

namespace Main.Models
{
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class OfferResponse
    {
        public int Id { get; set; }
        public int LikeCounter { get; set; }
        public string Text { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime SendingTime { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public Company Company { get; set; }
        public string ImageName { get; set; }
        public int Percentage { get; set; }
        public bool ForMan { get; set; }
        public bool ForWoman { get; set; }
        public int UpperAgeLimit { get; set; }
        public int LowerAgeLimit { get; set; }
        public bool UserLike { get; set; }
    }
    public class OffersByRelevance
    {
        public List<OfferResponse> preOffer;
        public List<OfferResponse> activeOffer;
        public List<OfferResponse> inactiveOffer;
    }
}