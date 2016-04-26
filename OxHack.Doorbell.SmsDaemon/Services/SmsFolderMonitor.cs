using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using OxHack.Doorbell.Common.Services;
using System.IO;
using System.Configuration;
using OxHack.Doorbell.SmsDaemon.Events;
using OxHack.Doorbell.Common.Models;

namespace OxHack.Doorbell.SmsDaemon.Services
{
	class SmsFolderMonitor : IService
	{
		private EventAggregator eventAggregator;
		private readonly FileSystemWatcher folderWatcher;

		public SmsFolderMonitor(EventAggregator eventAggregator)
		{
			string spoolPath;
			this.ExtractConfiguration(out spoolPath);

			this.eventAggregator = eventAggregator;
			this.folderWatcher = new FileSystemWatcher();
			this.folderWatcher.Path = spoolPath;
			//folderWatcher.NotifyFilter = NotifyFilters.LastWrite;
			this.folderWatcher.Filter = "*.*";
			
		}

		private void OnFileCreation(object sender, FileSystemEventArgs e)
		{
			var filename = e.FullPath;

			Console.WriteLine(filename + " Created...");

			try
			{
				var sms = this.ExtractSmsFromFile(filename);

				var @event = new SmsFileReceived(sms);
				Console.WriteLine("Publishing event...");
				this.eventAggregator.PublishOnCurrentThread(@event);
				Console.WriteLine("... Published event.");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				//throw;
			}
		}

		private Sms ExtractSmsFromFile(string fullPath)
		{
			FileInfo smsFile = new FileInfo(fullPath);
			if (!smsFile.Exists)
			{
				throw new InvalidOperationException("Could not find " + smsFile.FullName);
			}

			var parts = smsFile.Name.Split('_');

			if (parts.Length != 5 ||
				!parts[0].StartsWith("IN") ||
				parts[0].Length != 10 ||
				parts[1].Length != 6 ||
				parts[2].Length != 2 ||
				parts[3].Length != 13 ||
				parts[4].Length != 6 ||
				!parts[4].EndsWith(".txt"))
			{
				throw new InvalidOperationException("Error: Argument not in expected format.  Should be: INaaaaaaaa_bbbbbb_cc_ddddddddddddd_ee.txt");
			}

			int year = 0, month = 0, day = 0;
			try
			{
				year = Int32.Parse(parts[0].Substring(2, 4));
				month = Int32.Parse(parts[0].Substring(6, 2));
				day = Int32.Parse(parts[0].Substring(8, 2));
			}
			catch (Exception e)
			{
				throw new InvalidOperationException("Error: Could not parse date from the first filename segment.", e);
			}

			int hours = 0, minutes = 0, seconds = 0;
			try
			{
				hours = Int32.Parse(parts[1].Substring(0, 2));
				minutes = Int32.Parse(parts[1].Substring(2, 2));
				seconds = Int32.Parse(parts[1].Substring(4, 2));
			}
			catch (Exception e)
			{
				throw new InvalidOperationException("Error: Could not parse time from the second filename segment.", e);
			}

			var fileReader = smsFile.OpenRead();
			byte[] content = new byte[fileReader.Length];
			fileReader.Read(content, 0, (int)fileReader.Length);

			var body = Encoding.UTF8.GetString(content);
			var sender = parts[3];

			var message = new Sms(new DateTime(year, month, day, hours, minutes, seconds), sender, body);

			return message;
		}

		public async Task Start()
		{
			this.folderWatcher.Created += this.OnFileCreation;
			this.folderWatcher.EnableRaisingEvents = true;
			Console.WriteLine("Start...");
		}

		public async Task Stop()
		{
			this.folderWatcher.EnableRaisingEvents = false;
			this.folderWatcher.Created -= this.OnFileCreation;
		}

		private void ExtractConfiguration(out string spoolPath)
		{
			spoolPath = ConfigurationManager.AppSettings["spoolPath"];
			if (String.IsNullOrWhiteSpace(spoolPath))
			{
				throw new ConfigurationErrorsException("Error: Could not load spoolPath from configuration file.");
			}
		}
	}
}
