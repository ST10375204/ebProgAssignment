using System.ComponentModel.DataAnnotations;

namespace assignmentAPI.Models
{
  public class RegisterModel
  {


  
    [Required]
    [EmailAddress]
    public required string email { get; set; }
    [Required]
    public required string password { get; set; }

    [Required]
    public string firstName { get; set; }
    public string lastName { get; set; }
    [Required]
    public string userRole { get; set; }
  }
}