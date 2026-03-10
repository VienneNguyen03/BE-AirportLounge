using AirportLounge.Application.Features.Auth.Commands;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Infrastructure.Repositories;
using AirportLounge.Persistence;
using AirportLounge.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AirportLounge.Tests.Application;

public class LoginCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly BaseUnitOfWork _uow;

    public LoginCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _uow = new BaseUnitOfWork(_context);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ReturnsFailure()
    {
        var jwtService = new Mock<IJwtService>();
        var passwordHasher = new Mock<IPasswordHasher>();

        var handler = new LoginCommandHandler(_uow, jwtService.Object, passwordHasher.Object);
        var result = await handler.Handle(new LoginCommand("nonexistent@test.com", "any"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid", result.Message ?? "");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ReturnsFailure()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correct", BCrypt.Net.BCrypt.GenerateSalt(12));
        await SeedUserAsync("admin@test.com", hash, UserRole.Admin);

        var jwtService = new Mock<IJwtService>();
        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var handler = new LoginCommandHandler(_uow, jwtService.Object, passwordHasher.Object);
        var result = await handler.Handle(new LoginCommand("admin@test.com", "wrong"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccess()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correct", BCrypt.Net.BCrypt.GenerateSalt(12));
        await SeedUserAsync("admin@test.com", hash, UserRole.Admin);

        var jwtService = new Mock<IJwtService>();
        jwtService.Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns("token");
        jwtService.Setup(j => j.GenerateRefreshToken()).Returns("refresh");
        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(h => h.Verify("correct", hash)).Returns(true);

        var handler = new LoginCommandHandler(_uow, jwtService.Object, passwordHasher.Object);
        var result = await handler.Handle(new LoginCommand("admin@test.com", "correct"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("token", result.Data.AccessToken);
        Assert.Equal("Admin", result.Data.User.Role);
    }

    private async Task SeedUserAsync(string email, string passwordHash, UserRole role)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true,
            FullName = "Admin"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public void Dispose() => _context.Dispose();
}
