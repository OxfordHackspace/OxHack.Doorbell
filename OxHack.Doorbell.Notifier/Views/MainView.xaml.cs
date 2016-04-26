﻿using OxHack.Doorbell.Notifier.ViewModels;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using SD = System.Drawing;
using SW = System.Windows;
using SWF = System.Windows.Forms;

namespace OxHack.Doorbell.Notifier.Views
{
	/// <summary>
	/// Interaction logic for MainView.xaml
	/// </summary>
	[Export]
	public partial class MainView : SW.Window
	{
		private MainViewModel viewModel;
		private SWF.NotifyIcon trayIcon;

		[ImportingConstructor]
		public MainView()
		{
			InitializeComponent();
			this.SetupTrayIcon();
        }

		private void SetupTrayIcon()
		{
			this.trayIcon = new SWF.NotifyIcon();
			this.trayIcon.Icon = SD.Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().ManifestModule.Name);
			this.trayIcon.Visible = true;
			this.trayIcon.MouseDown += (sender, args) =>
			{
				if (args.Button == SWF.MouseButtons.Left)
				{
					this.ToggleWindow();
				}
			};
			this.trayIcon.ContextMenu = new SWF.ContextMenu(new SWF.MenuItem[] { new SWF.MenuItem("E&xit", (sender, args) => { this.Close(); }) });

			this.Closing += (sender, args) =>
			{
				this.ViewModel.Teardown();
				this.trayIcon.Visible = false;
			};
		}

		private void ToggleWindow()
		{
			if (this.grid.Opacity == 0)
			{
				this.TransitionToVisible();
			}
			else
			{
				this.TransitionToHidden();
			}
		}

		public void TransitionToVisible()
		{
			this.SkipToHidden();
			this.MoveToBottomRight();
			this.Reveal();
			this.Activate();
		}

		private SW.VisualState GetHiddenState()
		{
			SW.VisualState result = this.hiddenAtBottom;

			var workingArea = SWF.Screen.PrimaryScreen.WorkingArea;
			var mousePosition =
				new SW.Point(
					SWF.Control.MousePosition.X,
					SWF.Control.MousePosition.Y);

			if (mousePosition.Y <= workingArea.Top)
			{
				result = this.hiddenAtTop;
			}

			if (mousePosition.Y >= workingArea.Bottom)
			{
				result = this.hiddenAtBottom;
			}

			if (mousePosition.X <= workingArea.Left)
			{
				result = this.hiddenAtLeft;
			}

			if (mousePosition.X >= workingArea.Right)
			{
				result = this.hiddenAtRight;
			}

			return result;
		}

		private void SkipToHidden()
		{
			this.GoToHidden(false);
		}

		private void GoToHidden(bool useTransition)
		{
			this.IsHitTestVisible = false;
			var appropriateHiddenState = this.GetHiddenState();
			SW.VisualStateManager.GoToElementState(this.grid, appropriateHiddenState.Name, useTransition);
		}

		public void TransitionToHidden()
		{
			this.GoToHidden(true);
		}

		private void Reveal()
		{
			this.IsHitTestVisible = true;
			SW.VisualStateManager.GoToElementState(this.grid, this.visibleState.Name, true);
		}

		private void MoveToBottomRight()
		{
			var workingArea = SWF.Screen.PrimaryScreen.WorkingArea;

			var workingTop = (int)(workingArea.Top * this.VerticalDpiScale);
			var workingBottom = (int)(workingArea.Bottom * this.VerticalDpiScale);
			var workingLeft = (int)(workingArea.Left * this.HorizontalDpiScale);
			var workingRight = (int)(workingArea.Right * this.HorizontalDpiScale);

			var candidatePosition =
				new SD.Rectangle(
					(int)(workingRight - (this.AdjustedWidth / 2)),
					(int)(workingBottom - (this.AdjustedHeight / 2)),
					(int)this.AdjustedWidth,
					(int)this.AdjustedHeight);

			if (candidatePosition.Top < workingTop)
			{
				candidatePosition.Offset(0, workingTop - candidatePosition.Top);
			}
			if (candidatePosition.Bottom > workingBottom)
			{
				candidatePosition.Offset(0, workingBottom - candidatePosition.Bottom);
			}
			if (candidatePosition.Left < workingLeft)
			{
				candidatePosition.Offset(workingLeft - candidatePosition.Left, 0);
			}
			if (candidatePosition.Right > workingRight)
			{
				candidatePosition.Offset(workingRight - candidatePosition.Right, 0);
			}

			candidatePosition.Offset(-10, -10);

			this.AdjustedTop = candidatePosition.Top;
			this.AdjustedLeft = candidatePosition.Left;
		}

		public double HorizontalDpiScale
		{
			get
			{
				return SW.PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice.M11;
			}
		}

		public double VerticalDpiScale
		{
			get
			{
				return SW.PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice.M22;
			}
		}

		public void window_Deactivated(object sender, EventArgs e)
		{
			if (!this.ViewModel.HasPendingAcknowledgments)
			{
				this.TransitionToHidden();
			}
		}

		public double AdjustedWidth
		{
			get
			{
				return this.Width - (this.grid.Margin.Left + this.grid.Margin.Right);
			}
		}

		public double AdjustedHeight
		{
			get
			{
				return this.Height - (this.grid.Margin.Top + this.grid.Margin.Bottom);
			}
		}

		public double AdjustedTop
		{
			set
			{
				this.Top = value - this.grid.Margin.Top;
			}
		}

		public double AdjustedLeft
		{
			set
			{
				this.Left = value - this.grid.Margin.Left;
			}
		}

		public MainViewModel ViewModel
		{
			get
			{
				if (this.viewModel == null)
				{
					this.viewModel = (MainViewModel)this.DataContext;
				}

				return this.viewModel;
			}
		}
	}
}
