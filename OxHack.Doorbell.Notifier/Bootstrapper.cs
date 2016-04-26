using Caliburn.Micro;
using OxHack.Doorbell.Messaging;
using OxHack.Doorbell.Notifier.Extensions;
using OxHack.Doorbell.Notifier.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace OxHack.Doorbell.Notifier
{
	public class Bootstrapper : BootstrapperBase
	{
		private CompositionContainer container;

		public Bootstrapper()
		{
			base.Initialize();
		}

		protected override void Configure()
		{
			container = new CompositionContainer(
				new AggregateCatalog(
					AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()
					)
				);

			var batch = new CompositionBatch();

			batch.AddExportedValue<IWindowManager>(new WindowManager());
			batch.AddExportedValue<IEventAggregator>(new EventAggregator());
			batch.AddExportedValue(container);

			container.Compose(batch);
		}

		protected override object GetInstance(Type serviceType, string key)
		{
			string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
			var exports = container.GetExportedValues<object>(contract);

			if (exports.Any())
			{
				return exports.First();
			}

			throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
		}

		protected override IEnumerable<object> GetAllInstances(Type serviceType)
		{
			return container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
		}

		protected override void BuildUp(object instance)
		{
			container.SatisfyImportsOnce(instance);
		}

		protected override IEnumerable<Assembly> SelectAssemblies()
		{
			return base.SelectAssemblies().Union(typeof(IMessageListener).Assembly.Yield());
		}

		protected override void OnStartup(object sender, StartupEventArgs e)
		{
			base.DisplayRootViewFor<MainViewModel>();
		}
	}
}
