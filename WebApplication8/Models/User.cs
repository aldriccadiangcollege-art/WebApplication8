namespace WebApplication8.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";   // never store the plain password
        public string Role { get; set; } = "User";
    }

    public record AuthRequest(string Username, string Password);
}