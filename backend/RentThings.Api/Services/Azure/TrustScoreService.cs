using Microsoft.EntityFrameworkCore;
using RentThings.Api.Data;
using RentThings.Api.Models;

namespace RentThings.Api.Services;

public interface ITrustScoreService
{
    Task RecalculateAsync(Guid userId, CancellationToken ct = default);
    TrustLevel GetTrustLevel(int score);
}

public class TrustScoreService(RentThingsDbContext db) : ITrustScoreService
{
    public async Task RecalculateAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Users
            .Include(u => u.RentalsAsRenter)
            .Include(u => u.ReviewsReceived)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null) return;

        var completedRentals = user.RentalsAsRenter.Count(r => r.Status == RentalStatus.Completed);
        var lateReturns = user.RentalsAsRenter.Count(r => r.IsLateReturn);
        var avgRating = user.ReviewsReceived.Any()
            ? user.ReviewsReceived.Average(r => r.Rating)
            : 3.0;

        var score = 50;
        score += Math.Min(completedRentals * 3, 30);
        score += (int)((avgRating - 3) * 10);
        score -= lateReturns * 5;
        if (user.IsVerified) score += 10;
        score = Math.Clamp(score, 0, 100);

        user.TrustScore = score;
        user.TrustLevel = GetTrustLevel(score);
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public TrustLevel GetTrustLevel(int score) => score switch
    {
        >= 76 => TrustLevel.Platinum,
        >= 51 => TrustLevel.Gold,
        >= 26 => TrustLevel.Silver,
        _ => TrustLevel.Bronze
    };
}
