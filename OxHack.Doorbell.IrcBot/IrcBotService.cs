using IrcDotNet;
using OxHack.Doorbell.Common.Events;
using Prism.Events;
using System;
using System.Linq;
using System.Configuration;
using System.Threading.Tasks;
using System.Text;
using System.Timers;
using System.Net;
using OxHack.Doorbell.Common.Models;
using System.Collections.Concurrent;
using Caliburn.Micro;
using OxHack.Doorbell.Common.Services;

namespace OxHack.Doorbell.IrcBot
{
	public class IrcBotService : IService
	{
		private readonly IEventAggregator eventAggregator;
		private BlockingCollection<Sms> messageQueue;
		private Task messageQueueWorker;

		private readonly string ircHost;
		private readonly int ircPort;
		private readonly string ircNick;
		private readonly string ircPassword;
		private readonly string ircChannel;

		private readonly StandardIrcClient client;
		private IrcChannel channel;
		private Timer reconnectTimer;
		private readonly string phoneNumber;
		private int greetingThrottler;

		public IrcBotService(IEventAggregator eventAggregator)
		{
			this.eventAggregator = eventAggregator;
			this.eventAggregator.GetEvent<SmsReceived>().Subscribe(message => this.messageQueue.Add(message));

			this.messageQueue = new BlockingCollection<Sms>();

			// todo: add sanity checks
			this.ircHost = ConfigurationManager.AppSettings["ircHost"];
			this.ircPort = Int32.Parse(ConfigurationManager.AppSettings["ircPort"]);
			this.ircNick = ConfigurationManager.AppSettings["ircNick"];
			this.ircPassword = ConfigurationManager.AppSettings["ircPassword"];
			this.ircChannel = ConfigurationManager.AppSettings["ircChannel"];
			this.phoneNumber = ConfigurationManager.AppSettings["phoneNumber"];

			this.client = new StandardIrcClient();
			this.client.FloodPreventer = new IrcStandardFloodPreventer(7, 3000);
			this.client.ServerBounce += (s, e) =>
			{
				this.Connect(host: e.Address);
			};

			this.client.Connected += (s, e) =>
			{
				this.reconnectTimer = new Timer(30000);
				this.reconnectTimer.AutoReset = true;
				this.reconnectTimer.Elapsed += (sender, args) =>
				{
					if (!this.client.IsConnected)
					{
						this.Connect();
					}
				};
				this.reconnectTimer.Start();

				this.client.LocalUser.JoinedChannel += (sender, args) =>
				{
					this.channel = args.Channel;
					this.SendGreeting();
					this.messageQueueWorker = Task.Run(() => this.ProcessMessageQueue(this.messageQueue));
				};

				this.client.Channels.Join(this.ircChannel);
			};

			this.client.Disconnected += (s, e) =>
			{
				var oldQueue = this.messageQueue;
				this.messageQueue = new BlockingCollection<Sms>();
				oldQueue.CompleteAdding();
			};

			this.client.ConnectFailed += this.OnConnectFailed;
			this.client.Error += this.OnError;
			this.client.ProtocolError += this.OnProtocolError;
		}

		private void OnConnectFailed(object sender, IrcErrorEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void OnError(object sender, IrcErrorEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void OnProtocolError(object sender, IrcProtocolErrorEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void SendGreeting()
		{
			if (this.greetingThrottler % 5 == 0)
			{
				this.greetingThrottler = 0;
				this.SendMessage("Bzzt.  I am a robot.  I listen to robot jams.  Send me your jams at " + this.phoneNumber + ".");
			}
			this.greetingThrottler++;
		}

		public void Start()
		{
			Task.Run(() => this.Connect());
		}

		public void Stop()
		{
			this.reconnectTimer.Stop();
			// todo: sign off with this:
			//this.SendMessage("Marry me with my money.");
			this.client.Disconnect();
		}

		private void SendMessage(string message)
		{
			if (this.client != null && this.client.IsConnected && this.client.LocalUser != null && this.channel != null)
			{
				var sanitizedMessage = message.Replace("\n", String.Empty);
				this.client.LocalUser.SendMessage(this.channel, sanitizedMessage);
			}
			else
			{
				// todo: log error
			}
		}

		public void Connect(string nick = null, string host = null, int? port = null)
		{
			nick = nick ?? this.ircNick;
			host = host ?? this.ircHost;
			port = port ?? this.ircPort;

			this.client.Connect(
				new DnsEndPoint(host, port.Value),
				false,
				new IrcUserRegistrationInfo
				{
					UserName = this.ircNick,
					NickName = nick,
					Password = this.ircPassword,
					RealName = "OxHack Doorbell Bot"
				});
		}

		private void ProcessMessageQueue(BlockingCollection<Sms> queue)
		{
			while (!queue.IsCompleted)
			{
				try
				{
					var message = queue.Take();
					var sender = message.Sender.ToString();

					var unmaskedStartLength = 5;
					var unmaskedEndLength = 4;
					var maskLength = sender.Length - unmaskedStartLength - unmaskedEndLength;

					var sanitizer = new StringBuilder(sender);
					sanitizer.Remove(unmaskedStartLength, maskLength);
					sanitizer.Insert(unmaskedStartLength, "*", maskLength);
					var sanitizedSender = sanitizer.ToString();

					var formattedMessage = "At " + message.ReceivedOn + ", " + sanitizedSender + " sent the following SMS: " + message.Body.ToString().Trim();
					this.SendMessage(formattedMessage);
					this.SendGreeting();
				}
				catch(Exception e)
				{
					// todo: log
				}
			}
		}
	}
}
