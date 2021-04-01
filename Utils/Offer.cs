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
            var ONE_DAY_IN_MILLISECONDS = 86400000;
            var preOffer = offers.Where(offer => offer.DateStart > DateTime.UtcNow);
            var activeOffer = offers.Where(offer =>
            {
                var utcNow = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var offerStartDateTime = offer.DateStart.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                if (offer.DateStart.CompareTo(offer.DateEnd) == 0 && utcNow - offerStartDateTime < ONE_DAY_IN_MILLISECONDS)
                {
                    return true;
                }

                return offer.DateEnd >= DateTime.UtcNow && DateTime.UtcNow >= offer.DateStart;
            });
            var inactiveOffer = offers.Where(offer =>
            {
                var utcNow = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var offerStartDateTime = offer.DateStart.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                if (offer.DateStart.CompareTo(offer.DateEnd) == 0 && utcNow - offerStartDateTime < ONE_DAY_IN_MILLISECONDS)
                {
                    return false;
                }
                return offer.DateEnd < DateTime.UtcNow;
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