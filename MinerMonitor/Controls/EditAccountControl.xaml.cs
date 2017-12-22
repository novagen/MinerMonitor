using Monitor.Models;
using System;
using System.Linq;
using static NanopoolApi.Statics;

namespace Monitor.Controls
{
	/// <summary>
	/// Interaction logic for EditAccountControl.xaml
	/// </summary>
	public partial class EditPoolControl : IValueUserControl
	{
		private Pool Pool { get; set; }

		public EditPoolControl()
		{
			Init(null);
		}

		public EditPoolControl(Pool account)
		{
			Init(account);
		}

		private void Init(Pool account)
		{
			InitializeComponent();

			TypeInput.ItemsSource = Enum.GetValues(typeof(PoolType)).Cast<PoolType>();
			SetPool(account);
		}

		private void SetPool(Pool pool)
		{
			if (pool != null)
			{
				Pool = pool;

				NameInput.Text = Pool.Name;
				AccountInput.Text = pool.Wallet;
				TypeInput.SelectedValue = pool.Type;
			}
			else
			{
				Pool = new Pool();
			}
		}

		public object GetValue()
		{
			Pool.Name = NameInput.Text;
			Pool.Wallet = AccountInput.Text;
			Pool.Type = (PoolType)TypeInput.SelectedValue;

			return Pool;
		}
	}
}