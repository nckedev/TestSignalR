using Microsoft.AspNetCore.SignalR;
using Shared;

namespace TestSignalR;

public class TestHub : Hub<IAgentMethods> , IAppClientMethods
{
    private static List<string> _connections = new();
    private static readonly MessageList MessageList = new();
    private static string _agent = string.Empty;

    public override async Task OnConnectedAsync()
    {
        if (!_connections.Any())
        {
            _agent = Context.ConnectionId;
        }
        _connections.Add(Context.ConnectionId);
        Console.WriteLine(Context);
        Console.WriteLine(Context.ConnectionId + " connected");
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        Console.WriteLine("Disconnected");
        Console.WriteLine(Context.ConnectionId);
        // _connections.Remove(Context.ConnectionId);
    }

    private async Task<T> WaitForAnswer<T>(Guid requestId, int waitFor = 10000, int waitForInterval = 100)
        where T : class
    {
        for (var i = 0; i < waitFor / waitForInterval; i++)
        {
            if (MessageList.TryGet(requestId, out T value))
            {
                return value;
            }

            await Task.Delay(waitForInterval);
        }
        MessageList.Remove(requestId);
        throw new Exception("Fick inget svar från agenten");
    }


    public async Task<JobbsResponse<Part>> AppRequestPart(string id)
    {
        var requestId = MessageList.NewEntry();
        await Clients.Client(_agent).RequestPart(requestId, id);
        
        return await WaitForAnswer<JobbsResponse<Part>>(requestId);
    }

    public async Task<JobbsResponse<List<Part>>> AppRequestListOfParts()
    {
        var requestId = MessageList.NewEntry();
        await Clients.Client(_agent).RequestListOfParts(requestId);

        return await WaitForAnswer<JobbsResponse<List<Part>>>(requestId);
    }

    public async Task<JobbsResponse<Part>> AppRequestInvalidPart(string id)
    {
        var requestId = MessageList.NewEntry();
        await Clients.Client(_agent).RequestInvalidPart(requestId, id);

        return await WaitForAnswer<JobbsResponse<Part>>(requestId);
    }


    public async Task Receive(Guid requestId, object obj)
    {
        MessageList.TrySet(requestId, obj);
    }

}