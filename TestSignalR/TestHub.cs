using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Shared;

namespace TestSignalR;

public class TestHub : Hub<IAgentMethods>, IHubMethods
{
    private static readonly List<string> Connections = new();
    private static readonly MessageList MessageList = new();
    private static string _agent = string.Empty;

    public override Task OnConnectedAsync()
    {
        if (!Connections.Any())
        {
            _agent = Context.ConnectionId;
        }

        Connections.Add(Context.ConnectionId);
        Console.WriteLine(Context);
        Console.WriteLine(Context.ConnectionId + " connected");
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? ex)
    {
        Console.WriteLine("Disconnected");
        Console.WriteLine(Context.ConnectionId);
        return Task.CompletedTask;
        // _connections.Remove(Context.ConnectionId);
    }

    private async Task<T> WaitForAnswer<T>(Guid requestId, int waitFor = 10000, int waitForInterval = 100)
        where T : class
    {
        for (var i = 0; i < waitFor / waitForInterval; i++)
        {
            if (MessageList.TryGet(requestId, out T? value))
            {
                if (value is not null)
                {
                    return value;
                }
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


    public Task Receive(Guid requestId, object obj)
    {
        var res = JsonConvert.DeserializeObject<JobbsResponse<object>>(obj.ToString() ?? "");
        if (res?.Success == false)
        {
            //log(response.Message);
        }

        MessageList.TrySet(requestId, obj);
        return Task.CompletedTask;
    }
}