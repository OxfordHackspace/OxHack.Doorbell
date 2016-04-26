using Caliburn.Micro;
using MassTransit;
using Newtonsoft.Json;
using OxHack.Doorbell.Common.Events;
using OxHack.Doorbell.Common.Models;
using OxHack.Doorbell.Messaging.Consumers;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxHack.Doorbell.Messaging
{
	public class MessageListener : IMessageListener
	{
		private readonly IBusControl bus;
		private BusHandle busHandle;

		public MessageListener(IEventAggregator eventAggregator, string hostname, int port, string username, string password, string queueName, bool isDurable)
		{
			this.bus = Bus.Factory.CreateUsingRabbitMq(
				busConfig =>
				{
					busConfig.AutoDelete = true;
					busConfig.Durable = false;
					busConfig.Exclusive = true;
					var host =
						busConfig.Host(
						  new Uri("rabbitmq://" + hostname + ":" + port + "/"),
						  hostConfig =>
						  {
							  hostConfig.Username(username);
							  hostConfig.Password(password);
						  });
					busConfig.ReceiveEndpoint(
						host,
						queueName,
						receiveConfig =>
						{
							receiveConfig.AutoDelete = !isDurable;
							receiveConfig.Durable = isDurable;
							receiveConfig.Exclusive = true;
							receiveConfig.Consumer<DoorbellSmsConsumer>(() => new DoorbellSmsConsumer(eventAggregator));
						});
				});
		}

		public async Task Connect()
		{
			this.busHandle = await this.bus.StartAsync();
		}

		public async Task Disconnect()
		{
			if (this.busHandle != null)
			{
				await this.busHandle.StopAsync();
			}
		}
	}
}
