// See https://aka.ms/new-console-template for more information

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Shared;

namespace AppClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = new AppClient();
            await app.Init();
        }
    }

    public class AppClient
    {
        private HubConnection _hubConnection;

        public AppClient()
        {
            Console.WriteLine("App Client");
        }

        public async Task Init()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7169/test")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.KeepAliveInterval = TimeSpan.FromSeconds(10);
            _hubConnection.HandshakeTimeout = TimeSpan.FromSeconds(15);
            _hubConnection.ServerTimeout = TimeSpan.FromSeconds(300);

            await _hubConnection.StartAsync();

            await Task.Delay(1000);
            Console.WriteLine(_hubConnection.State);
            Console.WriteLine(_hubConnection.ConnectionId);

            var fakePart = new Part() {Name = "test", Id = "123"};
            var part = await _hubConnection.InvokeOnHub(x => x.AppRequestPart("123"));
            Console.WriteLine(part.Data?.Name);

            var part2 = await _hubConnection.InvokeOnHub(x => x.AppRequestPart(fakePart.Id + (1+2).ToString()));
            Console.WriteLine(part2.Data?.Name);

            var part3 = await _hubConnection.InvokeOnHub(x => x.AppRequestPart(TestMethodAsArgument("123").Substring(0, 2)));
            Console.WriteLine(part3.Data?.Name);

            var part4 = await _hubConnection.InvokeOnHub(x => x.AppRequestPart(TestMethodAsArgument("123")));
            Console.WriteLine(part4.Success? part4.Data?.Name : part4.Message);

            var part5 = await _hubConnection.InvokeOnHub(x => x.AppRequestInvalidPart(TestMethodAsArgument("123")));
            Console.WriteLine(part5.Success ? part5.Data?.Name : part5.Message);

            var part6 = await _hubConnection.InvokeOnHub(x => x.AppRequestPartTwoArguments(fakePart.Id, "filter"));
            Console.WriteLine(part6.Data?.Name);
            
            var partList = await _hubConnection.InvokeOnHub(x => x.AppRequestListOfParts());
            Console.WriteLine(partList.Success ? partList.Data?.Count ?? 0 : "error");
        }

        private static string TestMethodAsArgument(string a) => a + "test";
    }
}