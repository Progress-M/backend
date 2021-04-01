
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
        public OfferResponse() { }
        public OfferResponse(Offer offer, bool like)
        {
            Id = offer.Id;
            Text = offer.Text;
            DateStart = offer.DateStart;
            DateEnd = offer.DateEnd;
            TimeStart = offer.TimeStart;
            TimeEnd = offer.TimeEnd;
            Percentage = offer.Percentage;
            Company = offer.Company;
            CreateDate = offer.CreateDate;
            ForMan = offer.ForMan;
            LikeCounter = offer.LikeCounter;
            ForWoman = offer.ForWoman;
            SendingTime = offer.SendingTime;
            UpperAgeLimit = offer.UpperAgeLimit;
            LowerAgeLimit = offer.LowerAgeLimit;
            UserLike = like;
        }
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