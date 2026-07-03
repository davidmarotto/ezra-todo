using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TodoApi.Data;
using TodoApi.DTOs.Auth;
using TodoApi.Services;

namespace TodoApi.Tests;

public class AuthServiceTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private IConfiguration CreateConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-key-that-is-long-enough-for-hmac",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:ExpiryHours"] = "24"
            })
            .Build();

    [Fact]
    public async Task Register_ReturnsToken_WhenEmailIsNew()
    {
        var service = new AuthService(CreateDb(), CreateConfig());
        var response = await service.RegisterAsync(new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password"
        });

        Assert.NotNull(response.Token);
        Assert.Equal("test@example.com", response.Email);
    }

    [Fact]
    public async Task Register_Throws_WhenEmailAlreadyExists()
    {
        var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        var request = new RegisterRequest { Email = "test@example.com", Password = "password" };

        await service.RegisterAsync(request);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(request));
    }

    [Fact]
    public async Task Login_ReturnsToken_WhenCredentialsAreValid()
    {
        var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        await service.RegisterAsync(new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password"
        });

        var response = await service.LoginAsync(new LoginRequest
        {
            Email = "test@example.com",
            Password = "password"
        });

        Assert.NotNull(response.Token);
    }

    [Fact]
    public async Task Login_Throws_WhenPasswordIsWrong()
    {
        var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        await service.RegisterAsync(new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            }));
    }

    [Fact]
    public async Task Login_Throws_WhenEmailDoesNotExist()
    {
        var service = new AuthService(CreateDb(), CreateConfig());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest
            {
                Email = "nobody@example.com",
                Password = "password"
            }));
    }
}
