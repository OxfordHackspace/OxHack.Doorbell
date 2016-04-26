using Caliburn.Micro;
using System;

namespace OxHack.Doorbell.Notifier.ViewModels
{
	public class DoorbellNotificationViewModel : Screen
	{
		public DoorbellNotificationViewModel(DateTime receivedOn, string from, string message, bool isAcknowledged = false)
		{
			this.ReceivedOn = receivedOn;
			this.From = from;
			this.Message = message;
			this.IsAcknowledged = isAcknowledged;
		}

		public DateTime ReceivedOn
		{
			get;
			private set;
		}

		public string From
		{
			get;
			private set;
		}

		public string Message
		{
			get;
			private set;
		}

		public bool IsAcknowledged
		{
			get;
			private set;
		}

		public bool IsNotAcknowledged
		{
			get
			{
				return !this.IsAcknowledged;
			}
		}

		public void Acknowledge()
		{
			this.IsAcknowledged = true;
			base.NotifyOfPropertyChange(() => this.IsAcknowledged);
			base.NotifyOfPropertyChange(() => this.IsNotAcknowledged);
		}
    }
}
