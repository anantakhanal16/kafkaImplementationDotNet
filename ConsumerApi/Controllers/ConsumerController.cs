using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ProducerApi.OrderEvent;

namespace ConsumerApi.Controllers
{
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
            var bootstrapServers =
                Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS")
                ?? configuration["Kafka:BootstrapServers"]
                ?? "localhost:9092"; // fallback for local host

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
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
