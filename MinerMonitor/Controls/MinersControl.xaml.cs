using System.Windows;

namespace Monitor.Controls
{
	/// <summary>
	/// Interaction logic for MinersControl.xaml
	/// </summary>
	public partial class MinersControl : LoaderControl
	{
		public MinersControl()
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