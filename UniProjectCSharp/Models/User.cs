namespace UniProjectCSharp.Models
{
    public class User
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; } 
        public List<int> SavedGames { get; set; }
        public string? RepeatPassword { get; set; }
    }
}
