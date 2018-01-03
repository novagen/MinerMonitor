using System.Windows;

namespace Monitor.Controls
{
	/// <summary>
	/// Interaction logic for PoolsControl.xaml
	/// </summary>
	public partial class PoolsControl : LoaderControl
	{
		public PoolsControl()
		{
			InitializeComponent();
		}

		public override void LoaderControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender != null)
			{
				if (!((bool)e.NewValue))
				{
				}
				else
				{
				}
			}

			HideLoader();
		}
	}
}