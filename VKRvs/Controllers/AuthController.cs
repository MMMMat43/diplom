using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VKRvs.DTO;
using VKRvs.Models;

namespace VKRvs.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly VkrContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthController(VkrContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        // Регистрация нового клиента
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationDto registrationDto)
        {
            // Проверка на существование пользователя с таким же email
            if (await _dbContext.Customers.AnyAsync(c => c.Email == registrationDto.Email))
            {
                return BadRequest("Пользователь с таким email уже зарегистрирован.");
            }

            // Создание нового пользователя
            var newCustomer = new Customer
            {
                FirstName = registrationDto.FirstName,
                LastName = registrationDto.LastName,
                Email = registrationDto.Email,
                PhoneNumber = registrationDto.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registrationDto.Password), // Хэшируем пароль
                LoyaltyPoints = 0 // Начальное значение
            };

            // Сохранение в базу данных
            await _dbContext.Customers.AddAsync(newCustomer);
            await _dbContext.SaveChangesAsync();

            return Ok("Регистрация прошла успешно.");
        }

        // Авторизация клиента
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Email == loginDto.Email);
            if (customer == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, customer.PasswordHash))
            {
                return Unauthorized("Неверный email или пароль.");
            }

            var token = GenerateJwtToken(customer);

            // Сохранение токена в сессию
            HttpContext.Session.SetString("JwtToken", token);

            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(Customer customer)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, customer.Email),
                new Claim("CustomerId", customer.Id.ToString()),
                new Claim(ClaimTypes.Role, "Customer")
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(180),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
