namespace HomeBudget.AuthService.EF.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Login { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Password { get; set; } // Хэш пароля
        public DateTime RegDate { get; set; }
        public DateOnly? BirthDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
