using OxHack.Doorbell.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxHack.Doorbell.SmsDaemon.Events
{
	public class SmsFileReceived
	{
		public SmsFileReceived(Sms message)
		{
			this.Message = message;
		}

		public Sms Message
		{
			get;
			private set;
		}
	}
}
