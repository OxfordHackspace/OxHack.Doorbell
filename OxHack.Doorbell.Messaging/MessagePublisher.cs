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
	public class MessagePublisher
	{
		private readonly IBusControl bus;
		private BusHandle busHandle;

		public MessagePublisher(string hostname, int port, string username, string password)
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
				});
		}

		public async Task Connect()
		{
			this.busHandle = await this.bus.StartAsync();
		}

		public async Task Disconnect()
		{
			await this.busHandle.StopAsync();
		}

		public async Task Publish(Sms message)
		{
			await this.bus.Publish<Sms>(message);
		}
	}
}
