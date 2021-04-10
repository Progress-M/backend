using System;
using System.Collections.Generic;
using System.Linq;
using Main.Models;

namespace Main.Function
{
    public static class OfferUtils
    {

        public static OffersByRelevance GroupByRelevance(List<OfferResponse> offers)
        {
            var preOffer = offers.Where(offer =>
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(offer.Company.TimeZone);
                var dateTimeTZ = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                var start = offer.DateStart.AddHours(offer.TimeStart.Hour).AddMinutes(offer.TimeEnd.Minute);
                var end = offer.DateEnd.AddHours(offer.TimeEnd.Hour).AddMinutes(offer.TimeEnd.Minute);

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
            });
            var activeOffer = offers.Where(offer =>
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(offer.Company.TimeZone);
                var dateTimeTZ = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                var start = offer.DateStart.AddHours(offer.TimeStart.Hour).AddMinutes(offer.TimeEnd.Minute);
                var end = offer.DateEnd.AddHours(offer.TimeEnd.Hour).AddMinutes(offer.TimeEnd.Minute);

                if (DateTime.Compare(dateTimeTZ, start) >= 0 && DateTime.Compare(end, dateTimeTZ) >= 0)
                {
                    if (dateTimeTZ.TimeOfDay > offer.TimeStart.TimeOfDay && dateTimeTZ.TimeOfDay < offer.TimeEnd.TimeOfDay)
                    {
                        return true;
                    }
                }
                return false;
            });
            var inactiveOffer = offers.Where(offer =>
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(offer.Company.TimeZone);
                var dateTimeTZ = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                var end = offer.DateEnd.AddHours(offer.TimeEnd.Hour).AddMinutes(offer.TimeEnd.Minute);
                return DateTime.Compare(dateTimeTZ, end) >= 0;
            });


            return new OffersByRelevance
            {
                preOffer = preOffer.ToList(),
                activeOffer = activeOffer.ToList(),
                inactiveOffer = inactiveOffer.ToList()
            };
        }
    }
}