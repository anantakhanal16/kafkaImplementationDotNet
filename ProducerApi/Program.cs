using Microsoft.AspNetCore.Builder;
using ProducerApi.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSingleton<KafkaProducerService>();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // Listen on all IPs
});

var app = builder.Build();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Producer API V1");
    c.RoutePrefix = string.Empty; // Swagger available at http://localhost:5000/
});

// For local testing, you can comment this out if you don’t have HTTPS set up
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
