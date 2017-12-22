using Monitor.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Monitor
{
	public partial class ControlWindow : Window
	{
		private readonly IValueUserControl Control;
		public ControlWindowResult? Result;

		public ControlWindow(String title, IValueUserControl control)
		{
			InitializeComponent();

			Control = control;
			Title = title;

			ControlHost.Child = (UserControl)Control;
		}

		private void DialogOk_Click(object sender, RoutedEventArgs e)
		{
			Result = ControlWindowResult.OK;
			DialogResult = true;
		}

		private void DialogCancel_Click(object sender, RoutedEventArgs e)
		{
			Result = ControlWindowResult.Cancel;
			DialogResult = true;
		}

		public T GetValue<T>()
		{
			return (T)Control.GetValue();
		}
	}
}