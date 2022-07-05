using TestSignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = true;
    o.MaximumReceiveMessageSize = null;

}); //.AddNewtonsoftJsonProtocol();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapRazorPages();
app.MapHub<TestHub>("/test");

app.Run();