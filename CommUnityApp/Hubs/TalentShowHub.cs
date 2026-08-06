using Microsoft.AspNetCore.SignalR;

namespace CommUnityApp.Hubs
{
    public class TalentShowHub : Hub
    {
        public async Task JoinTalentShow()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "TalentShow");
        }

        public async Task LeaveTalentShow()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "TalentShow");
        }
    }
}
