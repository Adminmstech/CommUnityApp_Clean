using Microsoft.AspNetCore.SignalR;

public class AuctionHub : Hub
{
    public async Task JoinAuction(int auctionId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"Auction_{auctionId}");
    }

    public async Task LeaveAuction(int auctionId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"Auction_{auctionId}");
    }
}