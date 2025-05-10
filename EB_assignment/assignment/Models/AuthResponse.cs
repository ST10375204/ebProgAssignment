using System.ComponentModel.DataAnnotations;

namespace assignment.Models
{
    public class AuthResponse
    {
        [Required]
        public string Token { get; set; }
        
    }
}