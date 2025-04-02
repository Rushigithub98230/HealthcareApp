using System.ComponentModel.DataAnnotations;

namespace PatientService.DTOs
{
    public class LoginDto
    {
        [Required] public string Username { get; set; }
        [Required] public string Password { get; set; }
    }

}
