[kafka_microservices_readme (2).md](https://github.com/user-attachments/files/24059936/kafka_microservices_readme.2.md)
# Kafka Microservices with YARP API Gateway

A distributed microservices architecture using Apache Kafka for event-driven communication, built with .NET and Docker.

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        Docker Network                        │
│                                                              │
│  ┌──────────┐         ┌──────────┐         ┌──────────┐    │
│  │  YARP    │────────→│ Producer │────────→│  Kafka   │    │
│  │ Gateway  │         │   API    │         │  Broker  │    │
│  │  :9001   │         │  :5000   │         │  :9093   │    │
│  └──────────┘         └──────────┘         └────┬─────┘    │
│                                                  │          │
│                                                  ↓          │
│                       ┌──────────┐         ┌──────────┐    │
│                       │ Consumer │←────────│  message │    │
│                       │   API    │         │          │
│                       │  :8086   │         └──────────┘    │
│                       └──────────┘                          │
│                                                              │
│                       ┌──────────┐                          │
│                       │ Kafdrop  │                          │
│                       │   UI     │                          │
│                       │  :9000   │                          │
│                       └──────────┘                          │
└─────────────────────────────────────────────────────────────┘
```

## 🚀 Features

- **API Gateway**: YARP-based reverse proxy for routing requests
- **Event-Driven Architecture**: Asynchronous message processing with Kafka
- **Microservices**: Independent Producer and Consumer APIs
- **Docker Compose**: Full containerized deployment
- **Monitoring**: Kafdrop UI for Kafka topic visualization
- **Security**: Internal network communication, no external Kafka exposure

## 📋 Prerequisites

- [Docker](https://www.docker.com/get-started) (20.10+)
- [Docker Compose](https://docs.docker.com/compose/install/) (2.0+)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) (for local development)

## 🛠️ Tech Stack

- **.NET 8.0** - Microservices framework
- **Apache Kafka 7.5.0** - Message broker
- **YARP** - Reverse proxy and API Gateway
- **Confluent.Kafka** - .NET Kafka client
- **Zookeeper 7.5.0** - Kafka coordination service
- **Kafdrop 3.28.0** - Kafka web UI
- **Docker & Docker Compose** - Containerization

## 📁 Project Structure

```
.
├── ProducerApi/
│   ├── Controllers/         # API endpoints
│   ├── Services/           # Kafka producer service
│   ├── OrderEvent/         # Event models
│   ├── Configurations/     # Kafka settings
│   ├── Dockerfile
│   └── appsettings.json
│
├── ConsumerApi/
│   ├── Services/           # Kafka consumer service
│   ├── Dockerfile
│   └── appsettings.json
│
├── YarpApiGateway/
│   ├── Dockerfile
│   └── appsettings.json    # Route configuration
│
└── docker-compose.yml      # Container orchestration
```

## 🚦 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/anantakhanal16/kafkaImplementationDotNet.git
cd kafkaImplementationDotNet
```

### 2. Build and Run with Docker Compose

```bash
docker-compose up --build
```

This will start all services:
- API Gateway on `http://localhost:9001`
- Consumer API on `http://localhost:8086`
- Kafdrop UI on `http://localhost:9000`
- Producer API (internal only)
- Kafka, Zookeeper (internal only)

### 3. Verify Services are Running

```bash
docker-compose ps
```

You should see all containers in "Up" state.

## 📡 API Endpoints

### Producer API (via Gateway)

**Create Order**
```bash
curl --location 'http://localhost:9001/api/orders/create' \
--header 'Content-Type: application/json' \
--data '{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "Ananta",
  "productName": "from consumer",
  "quantity": 3,
  "createdAt": "2025-12-09T12:00:00Z",
  "status": "Pending",
  "totalPrice": 59.97
}'
```

**Response:**
```json
{
  "message": "Order published successfully",
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "partition": 0,
  "offset": 42
}
```

### Consumer API

**Health Check**
```bash
GET http://localhost:8086/health
```

## 🔍 Monitoring with Kafdrop

Access Kafdrop UI at `http://localhost:9000` to:
- View topics and partitions
- Inspect messages
- Monitor consumer groups
- Check broker health

## 🐳 Docker Services

| Service | Internal Port | External Port | Purpose | Network Access |
|---------|--------------|---------------|---------|----------------|
| API Gateway | 5002 | 9001 | Routes requests to microservices | Public |
| Producer API | 5000 | None (Internal only) | Publishes messages to Kafka | Internal only |
| Consumer API | 5001 | 8086 | Consumes messages from Kafka | Public |
| Kafka | 9093 (internal)<br>9092 (external) | 9092, 9093 | Message broker | Public + Internal |
| Zookeeper | 2181 | 2181 | Kafka coordination | Public |
| Kafdrop | 9000 | 9000 | Kafka monitoring UI | Public |

### Access Points from Host Machine:
- ✅ **API Gateway**: `http://localhost:9001` - Send requests here
- ✅ **Consumer API**: `http://localhost:8086` - Direct access for health checks
- ✅ **Kafdrop UI**: `http://localhost:9000` - Monitor Kafka topics
- ✅ **Kafka (External)**: `localhost:9092` - For local Kafka clients/tools
- ✅ **Zookeeper**: `localhost:2181` - For Zookeeper clients (if needed)
- ❌ **Producer API**: Not exposed - only accessible via API Gateway

### Internal Container Communication:
- API Gateway → Producer API: `http://producerapi:5000`
- Producer API → Kafka: `kafka:9093`
- Consumer API → Kafka: `kafka:9093`
- Kafdrop → Kafka: `kafka:9093`
- Kafka → Zookeeper: `zookeeper:2181`

## ⚙️ Configuration

### Docker Network
All services communicate through a single bridge network named `kafka`:
```yaml
networks:
  kafka:
    driver: bridge
```

### Kafka Configuration
Your Kafka setup supports both internal and external connections:

**Listeners:**
- **Internal**: `PLAINTEXT://kafka:9093` - Used by Docker containers
- **External**: `PLAINTEXT_HOST://localhost:9092` - Used by host machine

**Key Settings:**
```yaml
KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:9093,PLAINTEXT_HOST://0.0.0.0:9092
KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:9093,PLAINTEXT_HOST://localhost:9092
KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
KAFKA_AUTO_CREATE_TOPICS_ENABLE: "true"
```

**Port Mappings:**
```yaml
ports:
  - "9092:9092"  # External access from host
  - "9093:9093"  # Internal access (also exposed to host)
```

### Application Configuration

**Producer & Consumer APIs** use internal Kafka address:
```json
{
  "Kafka": {
    "BootstrapServers": "kafka:9093",
    "Topics": {
      "OrderCreated": "order-created-topic",
      "PaymentProcessed": "payment-processed-topic"
    }
  }
}
```

**External clients** on your host machine would use:
```
BootstrapServers: "localhost:9092"
```

### Gateway Routes

Configured in `YarpApiGateway/appsettings.json`:

```json
{
  "ReverseProxy": {
    "Routes": {
      "route1": {
        "ClusterId": "cluster1",
        "Match": {
          "Path": "{**catch-all}"
        }
      }
    },
    "Clusters": {
      "cluster1": {
        "Destinations": {
          "producer-api": {
            "Address": "http://producerapi:5000/"
          }
        }
      }
    }
  }
}
```

### Service Dependencies
```yaml
apigateway:
  depends_on:
    - producerapi
    - consumerapi

producerapi:
  depends_on:
    - kafka

consumerapi:
  depends_on:
    - kafka

kafka:
  depends_on:
    - zookeeper

kafdrop:
  depends_on:
    - kafka
```

## 🔒 Security Features

- **Network Isolation**: All services run in isolated `kafka` Docker bridge network
- **Producer API Protection**: No exposed ports - only accessible via API Gateway at `http://producerapi:5000`
- **Dual Kafka Listeners**: 
  - Internal (`kafka:9093`) - for container-to-container communication
  - External (`localhost:9092`) - for development/testing from host machine
- **Internal Communication**: All service-to-service communication uses Docker container names:
  - API Gateway → Producer API: `http://producerapi:5000`
  - Producer API → Kafka: `kafka:9093`
  - Consumer API → Kafka: `kafka:9093`
  - Kafdrop → Kafka: `kafka:9093`
  - Kafka → Zookeeper: `zookeeper:2181`
- **Gateway Pattern**: Single entry point (`localhost:9001`) for application requests
- **Development Friendly**: Kafka and Zookeeper exposed for local development tools and debugging

## 🧪 Testing

### Test Order Creation

```bash
curl --location 'http://localhost:9001/api/orders/create' \
--header 'Content-Type: application/json' \
--data '{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "Ananta",
  "productName": "from consumer",
  "quantity": 3,
  "createdAt": "2025-12-09T12:00:00Z",
  "status": "Pending",
  "totalPrice": 59.97
}'
```

### View Messages in Kafdrop

1. Open `http://localhost:9000`
2. Click on `order-created-topic`
3. View messages in the topic

## 🛑 Stopping the Application

```bash
# Stop all containers
docker-compose down

# Stop and remove volumes (cleans all data)
docker-compose down -v
```

## 🔧 Troubleshooting

### Container Connection Issues

```bash
# Check container logs
docker-compose logs [service-name]

# Example: Check producer logs
docker-compose logs producerapi

# Check Kafka logs
docker-compose logs kafka
```

### Kafka Connection Refused

Ensure services use the correct internal ports:
- Producer API → `kafka:9093`
- Consumer API → `kafka:9093`
- Kafdrop → `kafka:9093`

### Rebuild After Code Changes

```bash
docker-compose up --build --force-recreate
```

## 📊 Message Flow

1. **Client** sends HTTP request to API Gateway (`localhost:9001`)
2. **Gateway** routes request to Producer API (`producerapi:5000`)
3. **Producer API** serializes order and publishes to Kafka (`kafka:9093`)
4. **Kafka** stores message in `order-created-topic`
5. **Consumer API** subscribes to topic and processes message
6. **Kafdrop** provides visualization of all topics and messages

## 🎯 Use Cases

- Order processing systems
- Event-driven microservices
- Asynchronous task processing
- Real-time data streaming
- Decoupled service communication

## 📚 Additional Resources

- [Apache Kafka Documentation](https://kafka.apache.org/documentation/)
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [Confluent .NET Client](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html)
- [Docker Compose Documentation](https://docs.docker.com/compose/)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👤 Author

**Ananta Khanal**

- GitHub: [@anantakhanal16](https://github.com/anantakhanal16)
- Project Link: [https://github.com/anantakhanal16/kafkaImplementationDotNet](https://github.com/anantakhanal16/kafkaImplementationDotNet)

## 🙏 Acknowledgments

- Confluent for Kafka Docker images
- Microsoft for YARP reverse proxy
- Obsidian Dynamics for Kafdrop

---

⭐ Star this repo if you find it helpful!
