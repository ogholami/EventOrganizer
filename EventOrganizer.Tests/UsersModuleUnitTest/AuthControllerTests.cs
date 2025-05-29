using EventOrganizer.Server.Controllers;
using EventOrganizer.Server.DTOs;
using EventOrganizer.Server.Models;
using EventOrganizer.Server.Repositories;
using EventOrganizer.Server.Tools;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace EventOrganizer.Tests;

public class AuthControllerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _configMock = new Mock<IConfiguration>();
        _emailServiceMock = new Mock<IEmailService>();

        _controller = new AuthController(
            _userRepoMock.Object,
            _configMock.Object,
            _emailServiceMock.Object
        );
    }


    [Fact]
    public async Task Register_ShouldReturnOk_WhenUserIsNew()
    {
        var dto = new RegisterUserDto { Name = "Test", Email = "test@example.com", Password = "Password123" };

        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var result = await _controller.Register(dto);

        result.Should().BeOfType<OkObjectResult>();

        _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendEmailAsync(dto.Email,
            It.Is<string>(s => s.Contains("Confirm your email")),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenUserExists()
    {
        var dto = new RegisterUserDto { Name = "Test", Email = "exists@example.com", Password = "Password123" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(new User());

        var result = await _controller.Register(dto);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsValid()
    {
        // Arrange
        var dto = new LoginUserDto { Email = "aa@aa", Password = "Me123!" };
        var user = new User
        {
            Id = "1",
            Name = "Test User",
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);

        // Mock configuration values for JWT
        _configMock.Setup(c => c["Jwt:Key"]).Returns("supersecretkeywithatleast32chars!");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        // Act
        var result = await _controller.Login(dto) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var tokenProperty = result.Value!.GetType().GetProperty("token");
        tokenProperty.Should().NotBeNull("Expected response to contain a 'token' property");

        var token = tokenProperty!.GetValue(result.Value) as string;
        token.Should().NotBeNullOrWhiteSpace("Expected 'token' to be a non-empty string");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenInvalidCredentials()
    {
        var dto = new LoginUserDto { Email = "test@example.com", Password = "wrongpassword" };
        var user = new User { Email = dto.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword") };

        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);

        var result = await _controller.Login(dto);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task ConfirmEmail_ShouldReturnOk_WhenTokenValid()
    {
        var token = "validtoken";
        var user = new User
        {
            EmailVerificationToken = token,
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(10)
        };

        _userRepoMock.Setup(r => r.FindByEmailVerificationTokenAsync(token)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var result = await _controller.ConfirmEmail(token);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ConfirmEmail_ShouldReturnBadRequest_WhenTokenExpired()
    {
        var token = "expiredtoken";
        var user = new User
        {
            EmailVerificationToken = token,
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(-1)
        };

        _userRepoMock.Setup(r => r.FindByEmailVerificationTokenAsync(token)).ReturnsAsync(user);

        var result = await _controller.ConfirmEmail(token);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RequestReset_ShouldReturnOk_AndSendEmail_WhenUserExists()
    {
        var email = "reset@example.com";
        var user = new User { Email = email };

        _userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _emailServiceMock.Setup(e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var result = await _controller.RequestPasswordReset(email);

        result.Should().BeOfType<OkObjectResult>();
        _emailServiceMock.Verify(e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RequestReset_ShouldReturnOk_WhenUserDoesNotExist()
    {
        var email = "unknown@example.com";
        _userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((User?)null);

        var result = await _controller.RequestPasswordReset(email);

        result.Should().BeOfType<OkObjectResult>();
        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
