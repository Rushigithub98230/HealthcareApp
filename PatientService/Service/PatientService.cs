using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PatientService.Context;
using PatientService.DTOs;
using PatientService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PatientService.Service
{
    public class PatientService : IPatientService
    {
        private readonly PatientDbContext _context;
        private readonly IConfiguration _configuration;


        public PatientService(PatientDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        public async Task<AuthResponse> RegisterAsync(PatientRegistrationDto dto)
        {
            if (await _context.Patients.AnyAsync(p => p.Username == dto.Username))
                return new AuthResponse { StatusCode = StatusCodes.Status400BadRequest, Error = "Username already exists" };

            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                DateOfBirth = dto.DateOfBirth,
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();

            

            return new AuthResponse
            {
                StatusCode = StatusCodes.Status200OK,
                Token = GenerateToken(patient)
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginDto dto)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Username == dto.Username);

            if (patient == null || !BCrypt.Net.BCrypt.Verify(dto.Password, patient.PasswordHash))
                return new AuthResponse { StatusCode = StatusCodes.Status200OK, Error = "Invalid credentials" };

            return new AuthResponse
            {
                StatusCode = StatusCodes.Status200OK,
                Token = GenerateToken(patient)
            };
        }

        public string GenerateToken(Patient patient)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, patient.Username),
            new Claim(JwtRegisteredClaimNames.Email, patient.Email),
            new Claim("PatientId", patient.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



    }
}
