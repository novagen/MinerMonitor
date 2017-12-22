using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace Monitor.Controls
{
	public class LoaderControl : UserControl
	{
		protected BackgroundWorker BackgroundWorker { get; set; }

		public LoaderControl()
		{
			IsVisibleChanged += LoaderControl_IsVisibleChanged;
			Unloaded += LoaderControl_Unloaded;
		}

		public virtual void LoaderControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
		}

		public void ShowLoader()
		{
			var parentWindow = GetWindow();

			if (parentWindow != null)
			{
				parentWindow.SetLoader(true);
			}
		}

		public void HideLoader()
		{
			var parentWindow = GetWindow();

			if (parentWindow != null)
			{
				parentWindow.SetLoader(false);
			}
		}

		private MainWindow GetWindow()
		{
			var parentWindow = Dispatcher.Invoke(() =>
			{
				return Window.GetWindow(this);
			});

			if (parentWindow is MainWindow)
			{
				return (MainWindow)parentWindow;
			}

			return null;
		}

		private void LoaderControl_Unloaded(object sender, RoutedEventArgs e)
		{
			if (BackgroundWorker != null)
			{
				BackgroundWorker.Dispose();
			}
		}

		public void SetStatusMessage(string message)
		{
			var parentWindow = GetWindow();

			if (parentWindow != null)
			{
				parentWindow.SetStatusMessage(message);
			}
		}

		public WebProxy GetProxy()
		{
			var parentWindow = GetWindow();

			if (parentWindow != null)
			{
				return parentWindow.GetWebProxy();
			}

			return null;
		}
	}
}