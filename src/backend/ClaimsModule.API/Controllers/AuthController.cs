using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Simple hardcoded auth based on request role for assessment/testing purposes
        var userId = request.Role switch
        {
            "handler" => "a1111111-1111-1111-1111-111111111111",
            "supervisor" => "b2222222-2222-2222-2222-222222222222",
            "manager" => "c3333333-3333-3333-3333-333333333333",
            _ => null
        };

        if (userId == null)
        {
            return BadRequest("Invalid role. Supported roles: handler, supervisor, manager");
        }

        var userName = request.Role switch
        {
            "handler" => "John Handler",
            "supervisor" => "Sarah Supervisor",
            "manager" => "Michael Manager",
            _ => "Unknown"
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "DiceusClaimsManagementSystemSecretKey2026");
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, request.Role),
                new Claim("OrganizationId", "11111111-1111-1111-1111-111111111111") // Seed tenant
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"] ?? "DiceusClaimsAPI",
            Audience = _configuration["Jwt:Audience"] ?? "DiceusClaimsApp"
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            user = new { id = userId, name = userName, role = request.Role }
        });
    }
}

public record LoginRequest(string Role);
