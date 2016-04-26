using Caliburn.Micro;
using Newtonsoft.Json;
using OxHack.Doorbell.Common.Events;
using OxHack.Doorbell.Common.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxHack.Doorbell.Messaging
{
	[Obsolete]
	public class RabbitMQListener : IMessageListener
	{
		private readonly IEventAggregator eventAggregator;

		private readonly string hostname;
		private readonly int port;
		private readonly string username;
		private readonly string password;
		private readonly string exchange;
		private readonly string routingKey;
		private readonly string queueName;
		private readonly bool isDurable;
		private IModel channel;

		public RabbitMQListener(IEventAggregator eventAggregator, string hostname, int port, string username, string password, string exchange, string routingKey, string queueName, bool isDurable)
		{
			this.eventAggregator = eventAggregator;

			this.hostname = hostname;
			this.port = port;
			this.username = username;
			this.password = password;
			this.exchange = exchange;
			this.routingKey = routingKey;
			this.isDurable = isDurable;

			if (this.isDurable)
			{
				this.queueName = queueName;
			}
			else
			{
				this.queueName = queueName + "_" + Guid.NewGuid().ToString();
			}
		}

		public async Task Connect()
		{
			try
			{
				var connectionFactory = new ConnectionFactory();
				connectionFactory.HostName = this.hostname;
				connectionFactory.Port = this.port;
				connectionFactory.UserName = this.username;
				connectionFactory.Password = this.password;

				var connection = connectionFactory.CreateConnection();

				this.channel = connection.CreateModel();
				connection.AutoClose = true;

				var queueDeclarationOk = this.channel.QueueDeclare(this.queueName, this.isDurable, false, !this.isDurable, null);
				this.channel.QueueBind(this.queueName, this.exchange, this.routingKey);

				var consumer = new EventingBasicConsumer(this.channel);
				consumer.Received += (s, e) =>
				{
					this.PublishSmsReceivedEvent(e.Body);
				};

				this.channel.BasicConsume(queueName, true, consumer);

				if (queueDeclarationOk.MessageCount > 0)
				{
					var queueConsumer = new QueueingBasicConsumer(channel);
					channel.BasicConsume(queueName, true, queueConsumer);

					while (true)
					{
						var item = queueConsumer.Queue.DequeueNoWait(null);

						if (item != null)
						{
							this.PublishSmsReceivedEvent(item.Body);
						}
						else
						{
							break;
						}
					}
				}
			}
			catch (Exception e)
			{
				// todo: log
				//throw;
			}
		}

		public async Task Disconnect()
		{
			if (this.channel.IsOpen)
			{
				this.channel.Close();
			}
		}

		private void PublishSmsReceivedEvent(byte[] body)
		{
			var json = Encoding.UTF8.GetString(body);
			var message = JsonConvert.DeserializeObject<Sms>(json);
			this.eventAggregator.PublishOnUIThread(new SmsReceived(message));
		}
	}
}
