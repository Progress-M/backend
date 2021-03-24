namespace Main.Models
{
    public class ErrorStatus
    {
        public static string OfferTimeError = "OFFER_TIME_ERROR";
    }

    public class ErrorResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }
}