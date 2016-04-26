using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Newtonsoft.Json;
using OxHack.Doorbell.Common.Models;
using OxHack.Doorbell.Common.Events;

namespace OxHack.Doorbell.Messaging.Consumers
{
	public class DoorbellSmsConsumer : IConsumer<Sms>
	{
		private IEventAggregator eventAggregator;

		public DoorbellSmsConsumer(IEventAggregator eventAggregator)
		{
			this.eventAggregator = eventAggregator;
		}

		public async Task Consume(ConsumeContext<Sms> context)
		{
			//var json = context.GetPayload<string>();
			//var message = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Sms>(json));
			//this.eventAggregator.PublishOnUIThread(new SmsReceived(message));

			this.eventAggregator.PublishOnUIThread(new SmsReceived(context.Message));
		}
	}
}
