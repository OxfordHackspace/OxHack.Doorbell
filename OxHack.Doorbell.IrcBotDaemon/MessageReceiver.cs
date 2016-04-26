using Caliburn.Micro;
using OxHack.Doorbell.Common.Services;
using OxHack.Doorbell.Messaging;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace OxHack.Doorbell.IrcBotDaemon
{
	public class MessageReceiver : IService
	{
		private readonly IMessageListener listener;

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
					true);
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
