using ConsumerApi.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHostedService<KafkaConsumerService>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001); // Listen on all IPs
});
var app = builder.Build();


// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
