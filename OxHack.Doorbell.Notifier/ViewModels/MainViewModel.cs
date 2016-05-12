using Caliburn.Micro;
using OxHack.Doorbell.Common.Events;
using OxHack.Doorbell.Common.Models;
using OxHack.Doorbell.Messaging;
using OxHack.Doorbell.Services;
using OxHack.Doorbell.Notifier.Views;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Media;
using System.Reflection;
using System.IO;
using OxHack.Doorbell.Notifier.Screensaver;
using System.Windows.Interop;
using System.Windows;
using System.Diagnostics;

namespace OxHack.Doorbell.Notifier.ViewModels
{
	[Export]
	public class MainViewModel : Conductor<DoorbellNotificationViewModel>.Collection.AllActive, IHandle<SmsReceived>
	{
		private readonly MessageReceiver messageReceiver;

		private MainView hack;
		private readonly SoundPlayer player;

		[ImportingConstructor]
		public MainViewModel(IEventAggregator eventAggregator, MessageReceiver messageReceiver)
		{
			eventAggregator.Subscribe(this);

			this.messageReceiver = messageReceiver;

			if (Execute.InDesignMode)
			{
				this.Items.Add(new DoorbellNotificationViewModel(DateTime.Now, "+447554443212", "Yo mama"));
				this.Items.Add(new DoorbellNotificationViewModel(DateTime.Now, "+447554443216", "YoYo Ma", true));
				this.Items.Add(new DoorbellNotificationViewModel(DateTime.Now, "+447554443219", "Let me in!"));
			}

			var assemblyFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
			var doorbellFileInfo = new FileInfo(Path.Combine(assemblyFileInfo.DirectoryName, "Assets", "doorbell.wav"));

			this.player = new SoundPlayer(doorbellFileInfo.FullName);
		}

		protected override async void OnViewLoaded(object view)
		{
			base.OnViewLoaded(view);

			this.hack = (MainView)view;
			this.hack.TransitionToHidden();

			await this.messageReceiver.Start();

			HwndSource source = PresentationSource.FromVisual(this.hack) as HwndSource;
			source.AddHook(WndProc);
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			// Abort screensaver and monitor power-down
			const int WM_SYSCOMMAND = 0x0112;
			const int SC_SCREENSAVE = 0xF140;
			int WParam = wParam.ToInt32();

			if (msg == WM_SYSCOMMAND && WParam == SC_SCREENSAVE)
			{
				handled = (this.HasPendingAcknowledgments);
			}

			return IntPtr.Zero;
		}

		public bool HasPendingAcknowledgments
		{
			get
			{
				return this.Items.Any(item => item.IsNotAcknowledged);
			}
		}

		internal async Task Teardown()
		{
			await this.messageReceiver.Stop();
		}

		public async void Handle(SmsReceived message)
		{
			// stop screensaver
			if (Win32Interop.GetIsScreenSaverRunning())
			{
				Win32Interop.KillScreenSaver();
			}

			this.hack.TransitionToHidden();
			await Task.Delay(TimeSpan.FromSeconds(0.3));

			while (this.Items.Count >= 4)
			{
				var item = this.Items.First();
				item.Acknowledge();
				this.Items.Remove(item);
			}

			this.Items.Add(new DoorbellNotificationViewModel(message.Message.ReceivedOn, message.Message.Sender, message.Message.Body));

			await Task.Delay(TimeSpan.FromSeconds(0.4));
			this.hack.TransitionToVisible();

			if (message.Message.Body.ToLowerInvariant().Contains("secretariat"))
			{
				var proc = new ProcessStartInfo("https://www.youtube.com/watch?v=_B-CCX4ff64&feature=youtu.be&t=10s")
				{
					UseShellExecute = true
				};
				Process.Start(proc);
			}
			else
			{
				this.player.Play();
			}
		}
	}
}
