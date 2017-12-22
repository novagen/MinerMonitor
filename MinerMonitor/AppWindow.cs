using System.Windows;
using System.Windows.Controls;

namespace Monitor
{
	public class AppWindow : Window
	{
		public Window AsWindow()
		{
			return this as Window;
		}

		protected static void SaveWindowState(Grid mainGrid)
		{
			if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
			{
				Properties.Settings.Default.WindowTop = Application.Current.MainWindow.RestoreBounds.Top;
				Properties.Settings.Default.WindowLeft = Application.Current.MainWindow.RestoreBounds.Left;
				Properties.Settings.Default.WindowHeight = Application.Current.MainWindow.RestoreBounds.Height;
				Properties.Settings.Default.WindowWidth = Application.Current.MainWindow.RestoreBounds.Width;
				Properties.Settings.Default.WindowMaximized = true;
			}
			else
			{
				Properties.Settings.Default.WindowTop = Application.Current.MainWindow.Top;
				Properties.Settings.Default.WindowLeft = Application.Current.MainWindow.Left;
				Properties.Settings.Default.WindowHeight = Application.Current.MainWindow.Height;
				Properties.Settings.Default.WindowWidth = Application.Current.MainWindow.Width;
				Properties.Settings.Default.WindowMaximized = false;
			}

			Properties.Settings.Default.TreeWidth = mainGrid.ColumnDefinitions[0].Width.Value;
			Properties.Settings.Default.EditorWidth = mainGrid.ColumnDefinitions[2].Width.Value;

			Properties.Settings.Default.Save();
		}

		protected static void RestoreWindowState(Grid mainGrid)
		{
			if (Properties.Settings.Default.WindowTop > 0 || Properties.Settings.Default.WindowLeft > 0)
			{
				Application.Current.MainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
				Application.Current.MainWindow.Top = Properties.Settings.Default.WindowTop;
				Application.Current.MainWindow.Left = Properties.Settings.Default.WindowLeft;
			}

			Application.Current.MainWindow.Height = Properties.Settings.Default.WindowHeight;
			Application.Current.MainWindow.Width = Properties.Settings.Default.WindowWidth;
			Application.Current.MainWindow.WindowState = Properties.Settings.Default.WindowMaximized ? WindowState.Maximized : WindowState.Normal;

			mainGrid.ColumnDefinitions[0].Width = new GridLength(Properties.Settings.Default.TreeWidth, GridUnitType.Star);
			mainGrid.ColumnDefinitions[2].Width = new GridLength(Properties.Settings.Default.EditorWidth, GridUnitType.Star);

			SizeToFit();
			MoveIntoView();
		}

		private static void SizeToFit()
		{
			if (Application.Current.MainWindow.Height > SystemParameters.VirtualScreenHeight)
			{
				Application.Current.MainWindow.Height = SystemParameters.VirtualScreenHeight;
			}

			if (Application.Current.MainWindow.Width > SystemParameters.VirtualScreenWidth)
			{
				Application.Current.MainWindow.Width = SystemParameters.VirtualScreenWidth;
			}
		}

		private static void MoveIntoView()
		{
			if (Application.Current.MainWindow.Top + Application.Current.MainWindow.Height / 2 > SystemParameters.VirtualScreenHeight)
			{
				Application.Current.MainWindow.Top = SystemParameters.VirtualScreenHeight - Application.Current.MainWindow.Height;
			}

			if (Application.Current.MainWindow.Left + Application.Current.MainWindow.Width / 2 > SystemParameters.VirtualScreenWidth)
			{
				Application.Current.MainWindow.Left = SystemParameters.VirtualScreenWidth - Application.Current.MainWindow.Width;
			}

			if (Application.Current.MainWindow.Top < 0)
			{
				Application.Current.MainWindow.Top = 0;
			}

			if (Application.Current.MainWindow.Left < 0)
			{
				Application.Current.MainWindow.Left = 0;
			}
		}
	}
}