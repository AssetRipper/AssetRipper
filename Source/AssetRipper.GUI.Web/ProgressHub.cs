using Microsoft.AspNetCore.SignalR;

namespace AssetRipper.GUI.Web;

public class ProgressHub : Hub
{
    public async Task UpdateProgress(int progress)
    {
        await Clients.All.SendAsync("UpdateProgress", progress);
    }
}
