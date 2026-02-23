using Microsoft.AspNetCore.SignalR;

public class AuctionHub : Hub
{
    public async Task JoinAuction(string auctionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, auctionId);
    }

    public async Task LeaveAuction(string auctionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId);
    }
}
