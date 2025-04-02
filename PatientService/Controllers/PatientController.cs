using Microsoft.AspNetCore.Mvc;
using PatientService.DTOs;
using PatientService.Service;

namespace PatientService.Controllers;

[ApiController]
[Route("[controller]")]
public class PatientController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientController(IPatientService patientService)
    {
        _patientService = patientService;
    }



    [HttpPost("register")]
    public async Task<IActionResult> Register(PatientRegistrationDto dto)
    {
        var response = await _patientService.RegisterAsync(dto);
        return response.StatusCode == StatusCodes.Status200OK ? Ok(response) : BadRequest(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var response = await _patientService.LoginAsync(dto);
        return response.StatusCode == StatusCodes.Status200OK ? Ok(response) : Unauthorized(response);
    }
}
