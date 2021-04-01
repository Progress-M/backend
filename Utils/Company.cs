using System;
using Main.Models;
using Main.PostgreSQL;

namespace Main.Function
{
    public static class CompanyUtils
    {

        public static bool CanCreateOfferNow(Offer lastOffer)
        {
            if (lastOffer == null)
            {
                return true;
            }

            double durationSeconds = DateTime.UtcNow.Subtract(lastOffer.CreateDate).TotalSeconds;
            TimeSpan seconds = TimeSpan.FromSeconds(durationSeconds);
            var offerTimeout = Int32.Parse(_configuration["OfferTimeout"]);

            if (seconds.TotalHours < offerTimeout)
            {
                TimeSpan diffTimeSpan = TimeSpan.FromHours(offerTimeout).Subtract(seconds);
                string duration = String.Format(@"{0}:{1:mm\:ss\:fff}", diffTimeSpan.Days * offerTimeout + diffTimeSpan.Hours, diffTimeSpan);
                return NotFound(new BdobrResponse
                {
                    status = ResponseStatus.OfferTimeError,
                    message = $"Компания \"{company.NameOfficial}\" уже публиковала акцию за последние {offerTimeout} часа. " +
                    $"Осталось {duration} до следующей возможности создать акцию."
                });
            }

            return false;
        }
    }
}