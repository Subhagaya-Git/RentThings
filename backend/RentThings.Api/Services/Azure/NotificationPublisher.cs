using Microsoft.AspNetCore.SignalR;
using RentThings.Api.DTOs;
using RentThings.Api.Hubs;

namespace RentThings.Api.Services.Azure;

public interface INotificationPublisher
{
    Task PublishAsync(Guid userId, NotificationDto notification, CancellationToken ct = default);
    Task PublishListingCreatedAsync(ListingDto listing, CancellationToken ct = default);
    Task PublishListingUpdatedAsync(ListingDto listing, CancellationToken ct = default);
}

public class NotificationPublisher(IHubContext<NotificationHub> hub, ILogger<NotificationPublisher> logger) : INotificationPublisher
{
    public async Task PublishAsync(Guid userId, NotificationDto notification, CancellationToken ct = default)
    {
        var group = NotificationHub.UserGroup(userId);
        await hub.Clients.Group(group).SendAsync("ReceiveNotification", notification, ct);
        logger.LogInformation("SignalR notification sent to user {UserId}: {Title}", userId, notification.Title);
    }

    public async Task PublishListingCreatedAsync(ListingDto listing, CancellationToken ct = default)
    {
        await hub.Clients.Group(NotificationHub.ListingsGroup).SendAsync("ListingCreated", listing, ct);
        logger.LogInformation("SignalR ListingCreated broadcast for listing {ListingId}", listing.Id);
    }

    public async Task PublishListingUpdatedAsync(ListingDto listing, CancellationToken ct = default)
    {
        await hub.Clients.Group(NotificationHub.ListingsGroup).SendAsync("ListingUpdated", listing, ct);
        logger.LogInformation("SignalR ListingUpdated broadcast for listing {ListingId}", listing.Id);
    }
}
