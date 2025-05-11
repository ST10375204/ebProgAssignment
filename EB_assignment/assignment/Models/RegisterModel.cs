using System.ComponentModel.DataAnnotations;

namespace assignment.Models
{
    public class RegisterModel
    {
      
        [Required(ErrorMessage = "A valid Email Address is required.")]
        [EmailAddress]
        public required string email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public required string password { get; set; }


        [Required(ErrorMessage = "First name is required.")]
        public string firstName {get;set;}

         [Required(ErrorMessage = "Surname is required.")]
        public string lastName {get;set;}

       [Required(ErrorMessage = "User Role  is required.")]
        public string userRole{get;set;}
    }
}