namespace WordWebApplication.Models
{
    public class CustomRestrictParameter
    {
        public string PasswordBase64 { get; set; }
        public string SaltBase64 { get; set; }
        public int SpinCount { get; set; }
    }
}