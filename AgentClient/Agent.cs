// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Shared;
using Topshelf;

namespace AgentClient;

public class Program
{
    public static void Main(string[] args)
    {
        //     var agent = new AgentClient();
        //
        //     await agent.Init();
        // }

        var rc = HostFactory.Run(x =>
        {
            x.Service<AgentClient>(s =>
            {
                s.ConstructUsing(name => new AgentClient());
                s.WhenStarted(tc => tc.Init());
                s.WhenStopped(tc => tc.Stop());
            });

            x.EnableServiceRecovery(rc => { rc.RestartService(1); });

            x.RunAsLocalSystem();
            x.SetDescription("signalR test");
            x.SetDisplayName("signalR test");
            x.SetServiceName("signalR test");
        });

        var exitCode = (int) Convert.ChangeType(rc, rc.GetTypeCode());
        Environment.ExitCode = exitCode;
    }
}

public class AgentClient : IAgentMethods
{
    private HubConnection _hubConnection;

    public AgentClient()
    {
        Console.WriteLine("Agent Client");
    }

    public async void Init()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7169/test")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.KeepAliveInterval = TimeSpan.FromSeconds(100);
        _hubConnection.HandshakeTimeout = TimeSpan.FromSeconds(150);
        _hubConnection.ServerTimeout = TimeSpan.FromSeconds(300);
        await _hubConnection.StartAsync();

        await Task.Delay(1000);
        Console.WriteLine(_hubConnection.State);
        Console.WriteLine(_hubConnection.ConnectionId);

        _hubConnection.OnAgent<Guid, string>(x => x.RequestPart,
            async (requestId, id) => await RequestPart(requestId, id));

        _hubConnection.OnAgent<Guid>(x => x.RequestListOfParts,
            async (requestId) => await RequestListOfParts(requestId));

        _hubConnection.OnAgent<Guid, string>(x => x.RequestInvalidPart,
            async (requestId, partId) => await RequestInvalidPart(requestId, partId));
    }

    public async void Stop()
    {
        await _hubConnection.StopAsync();
    }

    private bool IsConnected() => _hubConnection.State == HubConnectionState.Connected;

    public async Task RequestPart(Guid requestId, string partId)
    {
        if (IsConnected())
        {
            await Task.Delay(1000);
            await _hubConnection.SendToHub(requestId, JobbsResponse<Part>.Ok(FakePart.Get()));
        }
    }

    public async Task RequestListOfParts(Guid requestId)
    {
        if (IsConnected())
        {
            var many = Enumerable.Range(1, 1000).Select(_ => FakePart.Get()).ToList();
            await _hubConnection.SendToHub(requestId, JobbsResponse<List<Part>>.Ok(many));
        }
    }

    public async Task RequestInvalidPart(Guid requestId, string partId)
    {
        if (IsConnected())
        {
            try
            {
                throw new Exception("part not found");
            }
            catch (Exception e)
            {
                await _hubConnection.SendToHub(requestId, JobbsResponse<Part>.Error("error", e.Message));
            }
        }
    }
}