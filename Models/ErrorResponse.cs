namespace Main.Models
{
    public class ErrorStatus
    {
        public static string ChatError = "CHAT_ERROR";
        public static string UserError = "USER_ERROR";
        public static string RegistrationError = "REGISTRATION_ERROR";
        public static string ProductCategoryError = "PRODUCT_CATEGORY_ERROR";
        public static string OfferError = "OFFER_ERROR";
        public static string CompanyError = "COMPANY_ERROR";
        public static string OfferTimeError = "OFFER_TIME_ERROR";
        public static string SignUpError = "SIGN_UP_ERROR";
        public static string SignInError = "SIGN_IN_ERROR";
    }

    public class ErrorResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }
}