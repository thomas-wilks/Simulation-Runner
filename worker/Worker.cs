using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Simulate.FostersAndPartners.Shared.Constants;
using Simulate.FostersAndPartners.Shared.Data;
using Simulate.FostersAndPartners.Shared.Models;
using Simulate.FostersAndPartners.Shared.Processing;
using Simulate.FostersAndPartners.Shared.Repositories;

namespace Simulate.FostersAndPartners.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ISimulationRepository _simRepo;
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;

        public Worker(ISimulationRepository simulationRepository, ILoggerFactory loggerFactory, IOptions<Storage> connections)
        {
            _simRepo = simulationRepository;
            _logger = loggerFactory.CreateLogger<Worker>();
            InitRabbitMQ(connections.Value.RabbitMQHost);
        }

        private void InitRabbitMQ(string rabbitMqHost = "rabbitmq")
        {
            var factory = new ConnectionFactory { HostName = rabbitMqHost };

            // create connection
            _connection = factory.CreateConnection();

            // create channel
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(RabbitMQConstants.SimulationExchange, ExchangeType.Direct);
            _channel.QueueDeclare(RabbitMQConstants.SimulationQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            _channel.QueueBind(RabbitMQConstants.SimulationQueue,
                RabbitMQConstants.SimulationExchange,
                RabbitMQConstants.SimulationQueuedRoutingKey);
            _channel.BasicQos(0, 5, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                // received message
                var simId = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

                // handle the received message
                await HandleMessage(simId);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(RabbitMQConstants.SimulationQueue, false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessage(string simId)
        {
            _logger.LogInformation($"consumer received simulation request: {simId}");
            if (string.IsNullOrEmpty(simId))
            {
                _logger.LogError("simulation id is null or empty");
                return;
            }

            if (!ObjectId.TryParse(simId, out var _))
            {
                _logger.LogError("simulation id is not a valid ObjectId");
                return;
            }

            var simulation = await _simRepo.GetSimulation(simId, default);
            if (simulation == null)
            {
                _logger.LogError($"simulation not found: {simId}");
                return;
            }

            await _simRepo.UpdateSimulationStatus(simId, SimulationStatusConstants.Started, default);

            // Simulate async work
            await SimulationProcessor.Simulate(default);

            _logger.LogInformation($"simulation completed: {simId}");

            await _simRepo.UpdateSimulationStatus(simId, SimulationStatusConstants.Completed, default);

            var result = new Result
            {
                Id = simId,
                Data = $"dummy result - {simId}"
            };
            await _simRepo.InsertResult(result, default);

            _logger.LogInformation($"result saved: {simId}");
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}