using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using ProducerApi.OrderEvent;

using static ProducerApi.Configurations.ProducerApiConfig;

namespace ProducerApi.Service
{
    public class KafkaProducerService
    {
        private readonly KafkaSettings _kafkaSettings;
        private readonly ProducerConfig _producerConfig;

        public KafkaProducerService(IOptions<KafkaSettings> kafkaSettings)
        {
            _kafkaSettings = kafkaSettings.Value;

            _producerConfig = new ProducerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers
            };
        }

        public async Task ProduceOrderAsync(OrderCreatedEvent order)
        {
            var topic = _kafkaSettings.Topics.OrderCreated;

            using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();

            var message = new Message<Null, string>
            {
                Value = JsonSerializer.Serialize(order)
            };

            var result = await producer.ProduceAsync(topic, message);

            Console.WriteLine($"📦 Order sent: {order.OrderId} -> Partition {result.Partition}, Offset {result.Offset}");
        }
    }
}
