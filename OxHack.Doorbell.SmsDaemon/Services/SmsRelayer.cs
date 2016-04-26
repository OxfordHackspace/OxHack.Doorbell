using Caliburn.Micro;
using OxHack.Doorbell.Common.Events;
using OxHack.Doorbell.Common.Services;
using OxHack.Doorbell.Messaging;
using OxHack.Doorbell.SmsDaemon.Events;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace OxHack.Doorbell.SmsDaemon.Services
{
	class SmsRelayer : IService, IHandle<SmsFileReceived>
	{
		private readonly MessagePublisher messagePublisher;

		public SmsRelayer(EventAggregator eventAggregator)
		{
			eventAggregator.Subscribe(this);

			string rmqHostname;
			int rmqHostPort;
			string rmqUsername;
			string rmqPassword;

			this.ExtractConfiguration(out rmqHostname, out rmqHostPort, out rmqUsername, out rmqPassword);

			this.messagePublisher = new MessagePublisher(rmqHostname, rmqHostPort, rmqUsername, rmqPassword);
		}

		private void ExtractConfiguration(out string rmqHostname, out int rmqHostPort, out string rmqUsername, out string rmqPassword)
		{
			
			rmqHostname = ConfigurationManager.AppSettings["rmqHostname"];
			if (String.IsNullOrWhiteSpace(rmqHostname))
			{
				throw new ConfigurationErrorsException("Error: Could not load rmqHostname from configuration file.");
			}

			if (!Int32.TryParse(ConfigurationManager.AppSettings["rmqHostPort"], out rmqHostPort))
			{
				throw new ConfigurationErrorsException("Error: Could not load rmqHostPort from configuration file.");
			}

			rmqUsername = ConfigurationManager.AppSettings["rmqUsername"];
			if (String.IsNullOrWhiteSpace(rmqUsername))
			{
				throw new ConfigurationErrorsException("Error: Could not load rmqUsername from configuration file.");
			}

			rmqPassword = ConfigurationManager.AppSettings["rmqPassword"];
			if (String.IsNullOrWhiteSpace(rmqPassword))
			{
				throw new ConfigurationErrorsException("Error: Could not load rmqPassword from configuration file.");
			}
		}

		public async Task Start()
		{
			await this.messagePublisher.Connect();
		}

		public async Task Stop()
		{
			await this.messagePublisher.Disconnect();
		}

		public async void Handle(SmsFileReceived message)
		{
			Console.WriteLine("Handing over to RabbitMQ...");
			Console.WriteLine("  Received on: " + message.Message.ReceivedOn);
			Console.WriteLine("  Sender     : " + message.Message.Sender);
			Console.WriteLine("  Body       : " + message.Message.Body);

			try
			{
				await this.messagePublisher.Publish(message.Message);
			}
			catch (Exception e)
			{
				Console.WriteLine("Error: ", e.ToString());
			}

			Console.WriteLine("... done!");
		}
	}
}
