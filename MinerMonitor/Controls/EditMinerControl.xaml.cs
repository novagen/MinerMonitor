using Monitor.Models;

namespace Monitor.Controls
{
	/// <summary>
	/// Interaction logic for EditAccountControl.xaml
	/// </summary>
	public partial class EditMinerControl : IValueUserControl
	{
		private Miner Miner { get; set; }

		public EditMinerControl()
		{
			Init(null);
		}

		public EditMinerControl(Miner miner)
		{
			Init(miner);
		}

		private void Init(Miner miner)
		{
			InitializeComponent();
			SetMiner(miner);
		}

		private void SetMiner(Miner miner)
		{
			if (miner != null)
			{
				Miner = miner;

				NameInput.Text = miner.Name;
				UrlInput.Text = Miner.Url;
				PortInput.Text = Miner.Port.ToString();
				UsernameInput.Text = Miner.Username;
				PasswordInput.Password = Miner.Password;
			}
			else
			{
				Miner = new Miner();
			}
		}

		public object GetValue()
		{
			Miner.Name = NameInput.Text;
			Miner.Url = UrlInput.Text;
			Miner.Port = int.Parse(PortInput.Text);
			Miner.Username = UsernameInput.Text;
			Miner.Password = PasswordInput.Password;

			return Miner;
		}
	}
}