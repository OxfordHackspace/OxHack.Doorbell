using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxHack.Doorbell.Common.Models
{
	public class Sms
	{
		public Sms(DateTime receivedOn, string sender, string body)
		{
			this.ReceivedOn = receivedOn;
			this.Sender = sender;
			this.Body = body;
		}

		public DateTime ReceivedOn
		{
			get;
			private set;
		}

		public string Sender
		{
			get;
			private set;
		}

		public string Body
		{
			get;
			private set;
		}
	}
}
