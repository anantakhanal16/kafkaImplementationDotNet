using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProducerApi.OrderEvent;
using ProducerApi.Service;

namespace ProducerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly KafkaProducerService kafkaProducer;

        public OrdersController(KafkaProducerService kafkaProducer)
        {
            this.kafkaProducer = kafkaProducer;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreatedEvent order)
        {
            order.OrderId = Guid.NewGuid();
            order.CreatedAt = DateTime.UtcNow;

            await kafkaProducer.ProduceOrderAsync(order);
            return Ok(new { message = "Order created and event published." });
        }
    }

}
