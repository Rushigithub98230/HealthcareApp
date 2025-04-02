using System.ComponentModel.DataAnnotations;

namespace PatientService.DTOs
{
    public class PatientRegistrationDto
    {
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required][EmailAddress] public string Email { get; set; }
        [Required] public DateTime DateOfBirth { get; set; }
        [Required] public string Username { get; set; }
        [Required][MinLength(6)] public string Password { get; set; }
    }
}
