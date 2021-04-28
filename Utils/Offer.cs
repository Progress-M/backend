using System;
using System.Collections.Generic;
using System.Linq;
using Main.Models;

namespace Main.Function
{
    public static class OfferUtils
    {
        private static bool isPreOffer(OfferResponse offer)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(offer.Company.TimeZone);
            var dateTimeTZ = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            var start = new DateTime(offer.DateStart.Year, offer.DateStart.Month, offer.DateStart.Day, offer.TimeStart.Hour, offer.TimeStart.Minute, offer.TimeStart.Second);
            var end = new DateTime(offer.DateEnd.Year, offer.DateEnd.Month, offer.DateEnd.Day, offer.TimeEnd.Hour, offer.TimeEnd.Minute, offer.TimeEnd.Second);

            if (DateTime.Compare(dateTimeTZ, start) >= 0 && DateTime.Compare(end, dateTimeTZ) >= 0)
            {
                if (dateTimeTZ.TimeOfDay > offer.TimeStart.TimeOfDay && dateTimeTZ.TimeOfDay < offer.TimeEnd.TimeOfDay)
                {
                    return false;
                }
            }

            if (DateTime.Compare(dateTimeTZ, end) >= 0)
            {
                return false;
            }
            return true;
        }
        private static bool isActiveOffer(OfferResponse offer)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(offer.Company.TimeZone);
            var dateTimeTZ = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            var start = new DateTime(offer.DateStart.Year, offer.DateStart.Month, offer.DateStart.Day, offer.TimeStart.Hour, offer.TimeStart.Minute, offer.TimeStart.Second);
            var end = new DateTime(offer.DateEnd.Year, offer.DateEnd.Month, offer.DateEnd.Day, offer.TimeEnd.Hour, offer.TimeEnd.Minute, offer.TimeEnd.Second);

            if (DateTime.Compare(dateTimeTZ, start) >= 0 && DateTime.Compare(end, dateTimeTZ) >= 0)
            {
                if (dateTimeTZ.TimeOfDay > start.TimeOfDay && dateTimeTZ.TimeOfDay < end.TimeOfDay)
                {
                    return true;
                }
            }
            return false;
        }

        public static OffersByRelevance GroupByRelevance(List<OfferResponse> offers)
        {
            var preOffer = new List<OfferResponse>();
            var activeOffer = new List<OfferResponse>();
            var inactiveOffer = new List<OfferResponse>();

            offers.ForEach(offer =>
            {
                if (isPreOffer(offer))
                {
                    preOffer.Add(offer);
                    return;
                }
                if (isActiveOffer(offer))
                {
                    activeOffer.Add(offer);
                    return;
                }
                inactiveOffer.Add(offer);
            });

            return new OffersByRelevance
            {
                preOffer = preOffer,
                activeOffer = activeOffer,
                inactiveOffer = inactiveOffer
            };
        }
    }
}