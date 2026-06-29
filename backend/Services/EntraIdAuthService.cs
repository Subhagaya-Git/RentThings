using Microsoft.EntityFrameworkCore;
using RentThings.Api.Data;
using RentThings.Api.DTOs;
using RentThings.Api.Models;

namespace RentThings.Api.Services;

public class EntraIdAuthService(RentThingsDbContext db, ILogger<EntraIdAuthService> logger) : IEntraIdAuthService
{
    public async Task<AuthResponse?> AuthenticateAsync(LoginRequest request, CancellationToken ct = default)
    {
        // Dev mock auth — replace with Microsoft.Identity.Web + Entra ID in production
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (user is null) return null;

        var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user.Id}:{user.Email}:{DateTime.UtcNow:O}"));
        logger.LogInformation("Mock auth for {Email}", request.Email);
        return new AuthResponse(token, MapUser(user));
    }

    public async Task<UserDto?> GetUserFromTokenAsync(string token, CancellationToken ct = default)
    {
        try
        {
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var userId = Guid.Parse(decoded.Split(':')[0]);
            var user = await db.Users.FindAsync([userId], ct);
            return user is null ? null : MapUser(user);
        }
        catch { return null; }
    }

    public Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
        => GetUserFromTokenAsync(token, ct).ContinueWith(t => t.Result is not null, ct);

    private static UserDto MapUser(User u) => new(u.Id, u.Email, u.DisplayName, u.ProfileImageUrl, u.Role.ToString(), u.TrustScore, u.TrustLevel.ToString(), u.IsVerified, u.Location);
}
