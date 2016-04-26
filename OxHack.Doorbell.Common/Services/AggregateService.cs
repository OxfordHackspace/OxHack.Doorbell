using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxHack.Doorbell.Common.Services
{
	public class AggregateService : IService
	{
		private readonly List<IService> services;

		public AggregateService(params IService[] services)
		{
			if (services == null || !services.Any())
			{
				throw new ArgumentException();
			}

			this.services = services.ToList();
		}

		public async Task Start()
		{
			var startTasks = this.services.Select(item => item.Start()).ToList();
            await Task.WhenAll(startTasks);
		}

		public async Task Stop()
		{
			var stopTasks = this.services.Select(item => item.Stop()).ToList();
			await Task.WhenAll(stopTasks);
		}
	}
}
