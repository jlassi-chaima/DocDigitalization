using Microsoft.AspNetCore.SignalR;

public sealed class Progress : Hub
{
    public async Task SendMessage(string content)
    {
        await Clients.All.SendAsync("ReceiveMessage", content);
    }
    //public override Task OnDisconnectedAsync(Exception exception)
    //{
    //    Groups.RemoveFromGroupAsync(Context.ConnectionId, "status_updates");
    //    return base.OnDisconnectedAsync(exception);
    //}

}