using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace RentThings.Api.Hubs;

public class NotificationHub : Hub
{
    public const string ListingsGroup = "listings";

    public static string UserGroup(Guid userId) => $"user-{userId}";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ListingsGroup);

        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userId, out var id))
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(id));

        await base.OnConnectedAsync();
    }
}
