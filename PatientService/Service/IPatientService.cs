using PatientService.DTOs;

namespace PatientService.Service
{
    public interface IPatientService
    {
        Task<AuthResponse> RegisterAsync(PatientRegistrationDto dto);
        Task<AuthResponse> LoginAsync(LoginDto dto);
    }
}
