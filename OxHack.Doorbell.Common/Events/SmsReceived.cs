using OxHack.Doorbell.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxHack.Doorbell.Common.Events
{
	public class SmsReceived
	{
		public SmsReceived(Sms message)
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
