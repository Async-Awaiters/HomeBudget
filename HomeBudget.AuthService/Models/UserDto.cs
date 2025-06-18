using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeBudget.AuthService.Models
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public required string Login { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTime RegDate { get; set; }
        public DateOnly? BirthDate { get; set; }
    }
}
