using Caliburn.Micro;
using OxHack.Doorbell.Messaging;
using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Threading.Tasks;

namespace OxHack.Doorbell.Services
{
	[Export]
	public class MessageReceiver
	{
		private readonly IMessageListener listener;

		[ImportingConstructor]
		public MessageReceiver(IEventAggregator eventAggregator)
		{
			// Todo: add sanity checks
			this.listener = 
				new MessageListener(
					eventAggregator,
					ConfigurationManager.AppSettings["rmqHostname"],
					Int32.Parse(ConfigurationManager.AppSettings["rmqHostPort"]),
					ConfigurationManager.AppSettings["rmqUsername"],
					ConfigurationManager.AppSettings["rmqPassword"],
                    ConfigurationManager.AppSettings["queueName"],
					false);
		}

		public async Task Start()
		{
			await this.listener.Connect();
		}

		public async Task Stop()
		{
			await this.listener.Disconnect();
		}
	}
}
