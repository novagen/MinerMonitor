using System;
using System.Windows;
using System.Windows.Input;

namespace Monitor.StyleableWindow
{
	public class WindowCloseCommand : ICommand
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
				window.Close();
			}
		}

		public void OnCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}