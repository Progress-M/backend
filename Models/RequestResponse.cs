namespace Main.Models
{
    public class AuthResponse
    {
        public int? Id { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }

    static class AuthStatus
    {
        public const string Success = "Success";
        public const string Fail = "Fail";
    }
}