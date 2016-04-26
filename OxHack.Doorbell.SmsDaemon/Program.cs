using Caliburn.Micro;
using Newtonsoft.Json;
using OxHack.Doorbell.Common.Models;
using OxHack.Doorbell.Common.Services;
using OxHack.Doorbell.Messaging;
using OxHack.Doorbell.SmsDaemon.Services;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using Topshelf;

namespace OxHack.Doorbell.Sms2Json
{
	class Program
	{
		static void Main(string[] args)
		{
			HostFactory.Run(host =>
			{
				var eventAggregator = new EventAggregator();

				host.Service<AggregateService>(service =>
				{
					service.ConstructUsing(name => new AggregateService(new SmsFolderMonitor(eventAggregator), new SmsRelayer(eventAggregator)));
					service.WhenStarted(async item => await item.Start());
					service.WhenStopped(async item => await item.Stop());
				});

                host.UseLinuxIfAvailable();

				host.RunAsLocalSystem();

				host.SetDescription("Receives notifications gammu-smsd and relays them to RabbitMQ.");
				host.SetDisplayName("OxHack Doorbell Daemon");
				host.SetServiceName("OxHackDoorbellDaemon");
			});
		}
	}
}