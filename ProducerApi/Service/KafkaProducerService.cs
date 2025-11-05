using Confluent.Kafka;
using System.Text.Json;
using ProducerApi.OrderEvent;

namespace ProducerApi.Service
{
    public class KafkaProducerService
    {
        private readonly string topic = "order-created-topic";
        private readonly ProducerConfig config;

        public KafkaProducerService(IConfiguration configuration)
        {
            var bootstrapServers =
                Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS")
                ?? configuration["Kafka:BootstrapServers"]
                ?? "localhost:9092"; // fallback

            config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };
        }

        public async Task ProduceOrderAsync(OrderCreatedEvent order)
        {
            using var producer = new ProducerBuilder<Null, string>(config).Build();

            var message = new Message<Null, string>
            {
                Value = JsonSerializer.Serialize(order)
            };

            var deliveryResult = await producer.ProduceAsync(topic, message);
            Console.WriteLine($"📦 Order sent: {order.OrderId} to partition {deliveryResult.Partition}");
        }
    }
}
