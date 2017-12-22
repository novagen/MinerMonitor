using Monitor.Models;

namespace Monitor.Controls
{
	/// <summary>
	/// Interaction logic for EditProxyControl.xaml
	/// </summary>
	public partial class EditProxyControl : IValueUserControl
	{
		private Proxy Proxy { get; set; }

		public EditProxyControl()
		{
			Init(null);
		}

		public EditProxyControl(Proxy proxy)
		{
			Init(proxy);
		}

		private void Init(Proxy proxy)
		{
			InitializeComponent();
			SetProxy(proxy);
		}

		private void SetProxy(Proxy proxy)
		{
			if (proxy != null)
			{
				Proxy = proxy;

				UsernameInput.Text = proxy.Username;
				PasswordInput.Text = proxy.Password;
				UrlInput.Text = proxy.Url;
			}
			else
			{
				Proxy = new Proxy();
			}
		}

		public object GetValue()
		{
			Proxy.Username = UsernameInput.Text;
			Proxy.Password = PasswordInput.Text;
			Proxy.Url = UrlInput.Text;

			return Proxy;
		}
	}
}