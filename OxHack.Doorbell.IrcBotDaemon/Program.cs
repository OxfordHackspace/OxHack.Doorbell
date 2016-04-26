using Caliburn.Micro;
using OxHack.Doorbell.Common.Services;
using OxHack.Doorbell.IrcBot;
using Topshelf;

namespace OxHack.Doorbell.IrcBotDaemon
{
	class Program
	{
		static void Main(string[] args)
		{
			var eventAggregator = new EventAggregator();

			HostFactory.Run(host =>
			{
				host.Service<AggregateService>(service =>
				{
					service.ConstructUsing(name => new AggregateService(new MessageReceiver(eventAggregator), new IrcBotService(eventAggregator)));
					service.WhenStarted(item => item.Start());
					service.WhenStopped(item => item.Stop());
				});

				host.RunAsLocalSystem();

				host.SetDescription("Receives doorbell notifications from RabbitMQ and pushes them to the oxhack irc channel.");
				host.SetDisplayName("OxHack Doorbell Daemon");
				host.SetServiceName("OxHackDoorbellDaemon");
			});
		}
	}
}
