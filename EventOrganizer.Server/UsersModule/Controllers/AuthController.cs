using BCrypt.Net;
using EventOrganizer.Server.DTOs;
using EventOrganizer.Server.Models;
using EventOrganizer.Server.Repositories;
using EventOrganizer.Server.Tools;
using EventOrganizer.Server.UsersModule.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventOrganizer.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _repo;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;

    public AuthController(IUserRepository repo, IConfiguration config, IEmailService emailService)
    {
        _repo = repo;
        _config = config;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        // Check if the user already exists
        var existing = await _repo.GetByEmailAsync(dto.Email);
        if (existing != null)
            return Conflict("Email is already in use.");

        // Create a new user
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // set inside initializer
            CreatedAt = DateTime.UtcNow
        };

        await _repo.CreateAsync(user);

        return Ok("Registration successful.");
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto dto)
    {
        var user = await _repo.GetByEmailAsync(dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        var token = GenerateJwtToken(user);
        return Ok(new { token });
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpGet("confirm")]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
        var user = await _repo.FindByEmailVerificationTokenAsync(token);
        if (user == null || user.EmailVerificationTokenExpires < DateTime.UtcNow)
            return BadRequest("Invalid or expired token.");

        user.IsVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpires = null;
        await _repo.UpdateAsync(user);

        return Ok("Email confirmed successfully.");
    }

    [HttpPost("request-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] string email)
    {
        var user = await _repo.GetByEmailAsync(email);
        if (user == null) return Ok(); // Don't expose that user doesn't exist

        user.PasswordResetToken = TokenGenerator.GenerateToken();
        user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
        await _repo.UpdateAsync(user);

        await _emailService.SendEmailAsync(user.Email, "Reset your password",
            $"Click here to reset: https://yourdomain.com/reset-password?token={user.PasswordResetToken}");

        return Ok("Reset link sent.");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await _repo.FindByResetTokenAsync(dto.Token);
        if (user == null || user.PasswordResetTokenExpires < DateTime.UtcNow)
            return BadRequest("Invalid or expired token.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpires = null;
        await _repo.UpdateAsync(user);

        return Ok("Password reset successfully.");
    }

}
