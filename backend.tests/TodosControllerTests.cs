using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests;

public class TodosControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _connection;

    public TodosControllerTests(WebApplicationFactory<Program> factory)
    {
        // An open SqliteConnection with DataSource=:memory: creates an in-memory database
        // that lives as long as the connection is open. Sharing this connection across
        // all scopes (seeding + HTTP requests) ensures they see the same data.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove ReminderBackgroundService — it creates its own scope and would
                // interfere with the database setup during testing.
                var hostedServices = services
                    .Where(d => d.ServiceType == typeof(IHostedService)
                             && d.ImplementationType == typeof(ReminderBackgroundService))
                    .ToList();
                foreach (var d in hostedServices) services.Remove(d);

                // Replace the production SQLite registration with our shared in-memory
                // connection. Staying on the SQLite provider avoids the "two database
                // providers registered" error that occurs when mixing Sqlite + InMemory.
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                             || d.ServiceType == typeof(AppDbContext))
                    .ToList();
                foreach (var d in descriptors) services.Remove(d);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(_connection));

                // Override JWT signing key so test-generated tokens are accepted
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters.IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes("test-secret-key-that-is-long-enough-for-hmac"));
                });
            });
        });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private string GenerateToken(Guid userId)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("test-secret-key-that-is-long-enough-for-hmac"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()) };
        var token = new JwtSecurityToken(
            issuer: "TodoApp",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<TodoList> SeedListAsync(Guid ownerId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var owner = new User { Id = ownerId, Email = $"{ownerId}@example.com", PasswordHash = "x" };
        var list = new TodoList { Name = "Test List", OwnerId = ownerId };
        db.Users.Add(owner);
        db.TodoLists.Add(list);
        await db.SaveChangesAsync();
        return list;
    }

    [Fact]
    public async Task GetTodos_Returns401_WhenUnauthenticated()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/lists/{Guid.NewGuid()}/todos");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTodos_Returns200_WhenAuthenticated()
    {
        var userId = Guid.NewGuid();
        var list = await SeedListAsync(userId);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateToken(userId));

        var response = await client.GetAsync($"/lists/{list.Id}/todos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTodos_Returns404_WhenAccessingAnotherUsersList()
    {
        var ownerId = Guid.NewGuid();
        var list = await SeedListAsync(ownerId);

        var otherUserId = Guid.NewGuid();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateToken(otherUserId));

        var response = await client.GetAsync($"/lists/{list.Id}/todos");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
