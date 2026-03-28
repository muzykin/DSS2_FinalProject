using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Todo.Api.Services
{
	public interface IRabbitMqService
	{
		void PublishEvent(string eventType, object message);
		string CheckHealth();
	}

	public class RabbitMqService : IRabbitMqService
	{
		private readonly ConnectionFactory _factory;

		public RabbitMqService(IConfiguration configuration)
		{
			_factory = new ConnectionFactory
			{
				HostName = "rabbitmq",
				Port = 5672,
				UserName = "guest",
				Password = "guest"
			};
		}

		public void PublishEvent(string eventType, object message)
		{
			try
			{
				using var connection = _factory.CreateConnection();
				using var channel = connection.CreateModel();

				channel.QueueDeclare(queue: "todo_events", durable: false, exclusive: false, autoDelete: false, arguments: null);

				var payload = new { Event = eventType, Data = message };
				var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));

				channel.BasicPublish(exchange: "", routingKey: "todo_events", basicProperties: null, body: body);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"RabbitMQ Publish Error: {ex.Message}");
			}
		}

		public string CheckHealth()
		{
			try
			{
				using var connection = _factory.CreateConnection();
				if (connection.IsOpen)
				{
					return "connected";
				}
				return "Connection is not open";
			}
			catch (Exception ex)
			{
				return $"Exception: {ex.Message}";
			}
		}
	}
}