using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RentThings.Api.Configuration;
using RentThings.Api.Data;
using RentThings.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RentThings.Api.Services.Azure;

/// <summary>
/// Mock Entra ID service for local development. Replace with Microsoft.Identity.Web in production.
/// </summary>
public class MockEntraIdService(
    RentThingsDbContext db,
    IOptions<AzureSettings> settings,
    ILogger<MockEntraIdService> logger) : IEntraIdService
{
    public async Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName, UserRole role, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(u => u.Email == email, ct))
            return new AuthResult(false, null, null, "Email already registered.");

        var user = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            EntraObjectId = Guid.NewGuid().ToString()
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("[Mock Entra] Registered user {Email}", email);

        return new AuthResult(true, GenerateToken(user), user, null);
    }

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive, ct);
        if (user is null)
            return new AuthResult(false, null, null, "Invalid credentials.");

        logger.LogInformation("[Mock Entra] Login for {Email}", email);
        return new AuthResult(true, GenerateToken(user), user, null);
    }

    public Task SendPasswordResetAsync(string email, CancellationToken ct = default)
    {
        logger.LogInformation("[Mock Entra] Password reset email sent to {Email}", email);
        return Task.CompletedTask;
    }

    public async Task<User?> GetUserFromTokenAsync(string token, CancellationToken ct = default)
    {
        if (!ValidateToken(token, out var userId)) return null;
        return await db.Users.FindAsync([userId], ct);
    }

    public Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
        => Task.FromResult(ValidateToken(token, out _));

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            settings.Value.EntraId.ClientSecret.Length >= 32
                ? settings.Value.EntraId.ClientSecret
                : "RentThings-Dev-Secret-Key-32chars!!"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("trust_score", user.TrustScore.ToString())
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: settings.Value.EntraId.ClientId,
            audience: settings.Value.EntraId.ClientId,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private bool ValidateToken(string token, out Guid userId)
    {
        userId = Guid.Empty;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                settings.Value.EntraId.ClientSecret.Length >= 32
                    ? settings.Value.EntraId.ClientSecret
                    : "RentThings-Dev-Secret-Key-32chars!!"));

            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = settings.Value.EntraId.ClientId,
                ValidateAudience = true,
                ValidAudience = settings.Value.EntraId.ClientId,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true
            }, out var validated);

            var jwt = (JwtSecurityToken)validated;
            var idClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out userId);
        }
        catch
        {
            return false;
        }
    }
}
