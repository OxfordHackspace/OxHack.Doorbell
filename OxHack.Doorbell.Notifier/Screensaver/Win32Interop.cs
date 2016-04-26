using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OxHack.Doorbell.Notifier.Screensaver
{
	class Win32Interop
	{
		// Signatures for unmanaged calls
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool SystemParametersInfo(
		   int uAction, int uParam, ref int lpvParam,
		   int flags);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool SystemParametersInfo(
		   int uAction, int uParam, ref bool lpvParam,
		   int flags);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int PostMessage(IntPtr hWnd,
		   int wMsg, int wParam, int lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr OpenDesktop(
		   string hDesktop, int Flags, bool Inherit,
		   uint DesiredAccess);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool CloseDesktop(
		   IntPtr hDesktop);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool EnumDesktopWindows(
		   IntPtr hDesktop, EnumDesktopWindowsProc callback,
		   IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool IsWindowVisible(
		   IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr GetForegroundWindow();

		// Callbacks
		private delegate bool EnumDesktopWindowsProc(
		   IntPtr hDesktop, IntPtr lParam);

		// Constants
		private const int SPI_GETSCREENSAVERACTIVE = 16;
		private const int SPI_SETSCREENSAVERACTIVE = 17;
		private const int SPI_GETSCREENSAVERTIMEOUT = 14;
		private const int SPI_SETSCREENSAVERTIMEOUT = 15;
		private const int SPI_GETSCREENSAVERRUNNING = 114;
		private const int SPIF_SENDWININICHANGE = 2;

		private const uint DESKTOP_WRITEOBJECTS = 0x0080;
		private const uint DESKTOP_READOBJECTS = 0x0001;
		private const int WM_CLOSE = 16;


		///// <summary>
		///// Returns True if the screen saver is active (enabled, but not necessarily running).
		///// </summary>
		///// <returns></returns>
		//public static bool GetIsScreenSaverActive()
		//{
		//	bool isActive = false;

		//	SystemParametersInfo(SPI_GETSCREENSAVERACTIVE, 0,
		//	   ref isActive, 0);
		//	return isActive;
		//}

		///// <summary>
		///// Pass in True to activate or False to deactivate the screen saver.
		///// </summary>
		///// <param name="isActive"></param>
		//public static void SetIsScreenSaverActive(bool isActive)
		//{
		//	int nullVar = 0;

		//	SystemParametersInfo(SPI_SETSCREENSAVERACTIVE,
		//	   isActive ? 1 : 0, ref nullVar, SPIF_SENDWININICHANGE);
		//}

		//public static TimeSpan GetScreenSaverTimeout()
		//{
		//	Int32 seconds = 0;

		//	SystemParametersInfo(SPI_GETSCREENSAVERTIMEOUT, 0,
		//	   ref seconds, 0);

		//	return TimeSpan.FromSeconds(seconds);
		//}

		//public static void SetScreenSaverTimeout(TimeSpan timeout)
		//{
		//	int nullVar = 0;

		//	SystemParametersInfo(SPI_SETSCREENSAVERTIMEOUT,
		//	   (int)timeout.TotalSeconds, ref nullVar, SPIF_SENDWININICHANGE);
		//}

		/// <summary>
		/// Returns True if the screen saver is actually running.
		/// </summary>
		/// <returns></returns>
		public static bool GetIsScreenSaverRunning()
		{
			bool isRunning = false;

			SystemParametersInfo(SPI_GETSCREENSAVERRUNNING, 0,
			   ref isRunning, 0);
			return isRunning;
		}

		public static void KillScreenSaver()
		{
			IntPtr hDesktop = OpenDesktop("Screen-saver", 0, false, DESKTOP_READOBJECTS | DESKTOP_WRITEOBJECTS);
			if (hDesktop != IntPtr.Zero)
			{
				EnumDesktopWindows(hDesktop, new
				   EnumDesktopWindowsProc(KillScreenSaverFunc),
				   IntPtr.Zero);
				CloseDesktop(hDesktop);
			}
			else
			{
				PostMessage(GetForegroundWindow(), WM_CLOSE,
				   0, 0);
			}
		}

		private static bool KillScreenSaverFunc(IntPtr hWnd,
		   IntPtr lParam)
		{
			if (IsWindowVisible(hWnd))
				PostMessage(hWnd, WM_CLOSE, 0, 0);
			return true;
		}
	}
}