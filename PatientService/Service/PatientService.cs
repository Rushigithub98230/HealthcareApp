using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PatientService.Context;
using PatientService.DTOs;
using PatientService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MimeKit;

namespace PatientService.Service
{
    public class PatientService : IPatientService
    {
        private readonly PatientDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailConfiguration _emailConfig;

        public PatientService(PatientDbContext context, IConfiguration configuration, EmailConfiguration emailConfig)
        {
            _context = context;
            _configuration = configuration;
            _emailConfig = emailConfig;
        }


        public async Task<AuthResponse> RegisterAsync(PatientRegistrationDto dto)
        {
            if (await _context.Patients.AnyAsync(p => p.Username == dto.Username))
                return new AuthResponse { StatusCode = StatusCodes.Status400BadRequest, Error = "Username already exists" };

            var patient = new Patient
            {
                
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                DateOfBirth = dto.DateOfBirth,
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();

            var message = new Message(new String[] { patient.Email! }, "Registration Done successfully", $"Your Registration has been completed , here is your username and password, UserName: {patient.Username}, Password:{dto.Password}");
            sendEmail(message);

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

        public void sendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            Send(emailMessage);
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };

            return emailMessage;

        }


        public void Send(MimeMessage mailMessage)
        {
            using var client = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_emailConfig.UserName, _emailConfig.Password);

                client.Send(mailMessage);
            }
            catch
            {
                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();

            }
        }




    }
}
