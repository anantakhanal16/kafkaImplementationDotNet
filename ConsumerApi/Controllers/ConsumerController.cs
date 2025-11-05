using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProducerApi.OrderEvent;
using System.Text.Json;

namespace ConsumerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly string topic = "order-created-topic";

        public KafkaConsumerService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = "payment-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);
                    var order = JsonSerializer.Deserialize<OrderCreatedEvent>(cr.Message.Value);

                    Console.WriteLine($"✅ PaymentService received OrderId: {order.OrderId}, Amount: {order.Amount}");

                    await Task.Delay(100, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
        }
    }
}
