﻿using System;
using System.Windows;
using System.Windows.Input;

namespace Monitor.StyleableWindow
{
	public class WindowMinimizeCommand : ICommand
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
				window.WindowState = WindowState.Minimized;
			}
		}

		public void OnCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}