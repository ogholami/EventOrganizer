using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using EventOrganizer.Server.Controllers;
using EventOrganizer.Server.Repositories;
using EventOrganizer.Server.DTOs;
using EventOrganizer.Server.Models;
using EventOrganizer.Server.Tools;
using System.Threading.Tasks;

namespace EventOrganizer.Tests;

public class AuthControllerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly IConfiguration _config;

    public AuthControllerTests()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "supersecretkeysupersecretkey123"},
            {"Jwt:Issuer", "https://localhost"},
            {"Jwt:Audience", "https://localhost"}
        };
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegistrationSuccessful()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email))
                     .ReturnsAsync((User?)null);

        var controller = new AuthController(_userRepoMock.Object, _config, _emailServiceMock.Object);

        // Act
        var result = await controller.Register(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendEmailAsync(
            dto.Email,
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains("Confirm your email"))
        ), Times.Once);
    }
}
