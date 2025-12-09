var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5002);
});
var app = builder.Build();
app.MapReverseProxy();
//app.MapForwarder("{**rest}", "http://localhost:5000");
app.MapGet("/", () => "Hello World!");


app.Run();
