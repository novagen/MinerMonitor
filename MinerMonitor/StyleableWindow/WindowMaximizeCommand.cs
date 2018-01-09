using System;
using System.Windows;
using System.Windows.Input;

namespace Monitor.StyleableWindow
{
	public class WindowMaximizeCommand : ICommand
	{
		public bool CanExecute(object parameter)
		{
			return true;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			if (parameter is Window window)
			{
				if (window.WindowState == WindowState.Maximized)
				{
					window.WindowState = WindowState.Normal;
					window.ResizeMode = ResizeMode.CanResize;
				}
				else
				{
					window.ResizeMode = ResizeMode.NoResize;
					window.WindowState = WindowState.Maximized;
				}
			}
		}

		public void OnCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}