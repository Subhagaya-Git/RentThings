using RentThings.Api.Models;

namespace RentThings.Api.Services.Azure;

public interface IEntraIdService
{
    Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName, UserRole role, CancellationToken ct = default);
    Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default);
    Task SendPasswordResetAsync(string email, CancellationToken ct = default);
    Task<User?> GetUserFromTokenAsync(string token, CancellationToken ct = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default);
}

public record AuthResult(bool Success, string? Token, User? User, string? Error);
