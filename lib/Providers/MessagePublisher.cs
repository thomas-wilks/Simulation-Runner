using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Simulate.FostersAndPartners.Shared.Constants;
using Simulate.FostersAndPartners.Shared.Data;

namespace Simulate.FostersAndPartners.Shared.Providers
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly ConnectionFactory _connectionFactory;

        public MessagePublisher(IOptions<Storage> connections)
        {
            _connectionFactory = new ConnectionFactory() { HostName = connections.Value.RabbitMQHost };
        }

        public void PublishMessage(string simulationId)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();
            {
                channel.ExchangeDeclare(RabbitMQConstants.SimulationExchange, ExchangeType.Direct);
                var body = Encoding.UTF8.GetBytes(simulationId);

                channel.BasicPublish(exchange: RabbitMQConstants.SimulationExchange,
                    routingKey: RabbitMQConstants.SimulationQueuedRoutingKey,
                    body: body);
            }
        }
    }
}