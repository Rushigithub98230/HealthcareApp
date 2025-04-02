namespace PatientService.DTOs
{
    public class AuthResponse
    {
       public int StatusCode { get; set; }
       public string? Token { get; set; }

       public string Error { get; set; }

    }
}
