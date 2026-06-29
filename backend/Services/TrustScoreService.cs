using Microsoft.EntityFrameworkCore;
using RentThings.Api.Data;
using RentThings.Api.DTOs;
using RentThings.Api.Models;

namespace RentThings.Api.Services;

public class TrustScoreService(RentThingsDbContext db) : ITrustScoreService
{
    public async Task RecalculateScoreAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Users
            .Include(u => u.RentalsAsRenter)
            .Include(u => u.RentalsAsOwner)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return;

        var completed = user.RentalsAsRenter.Count(r => r.Status == RentalStatus.Completed)
                        + user.RentalsAsOwner.Count(r => r.Status == RentalStatus.Completed);
        var reviews = await db.Reviews.Where(r => r.RevieweeId == userId).ToListAsync(ct);
        var avgRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 3.0;
        var lateReturns = user.RentalsAsRenter.Count(r => r.ReturnedAt.HasValue && r.EndDate < DateOnly.FromDateTime(r.ReturnedAt.Value));

        var score = 50;
        score += Math.Min(completed * 3, 30);
        score += (int)((avgRating - 3) * 10);
        score += user.IsVerified ? 10 : 0;
        score -= lateReturns * 5;
        score = Math.Clamp(score, 0, 100);

        var prev = user.TrustScore;
        user.TrustScore = score;
        user.TrustLevel = CalculateLevel(score);
        user.UpdatedAt = DateTime.UtcNow;

        db.TrustScoreHistories.Add(new TrustScoreHistory
        {
            UserId = userId,
            PreviousScore = prev,
            NewScore = score,
            Reason = "Automatic recalculation"
        });
        await db.SaveChangesAsync(ct);
    }

    public TrustScoreDto GetTrustScore(User user)
    {
        var factors = new List<string>();
        if (user.IsVerified) factors.Add("Verified account (+10)");
        factors.Add($"Trust level: {user.TrustLevel}");
        return new TrustScoreDto(user.TrustScore, user.TrustLevel.ToString(), factors);
    }

    public TrustLevel CalculateLevel(int score) => score switch
    {
        >= 85 => TrustLevel.Platinum,
        >= 70 => TrustLevel.Gold,
        >= 55 => TrustLevel.Silver,
        _ => TrustLevel.Bronze
    };
}

public class NotificationService(RentThingsDbContext db) : INotificationService
{
    public async Task CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, Guid? relatedId = null, CancellationToken ct = default)
    {
        db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedEntityId = relatedId
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.Type.ToString(), n.IsRead, n.RelatedEntityId, n.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
    {
        var n = await db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, ct);
        if (n is null) return;
        n.IsRead = true;
        await db.SaveChangesAsync(ct);
    }
}
