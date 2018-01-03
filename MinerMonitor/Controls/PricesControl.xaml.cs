using NanopoolApi;
using NanopoolApi.Data;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using static NanopoolApi.Statics;

namespace Monitor.Controls
{
	/// <summary>
	/// Interaction logic for PricesControl.xaml
	/// </summary>
	public partial class PricesControl : LoaderControl
	{
		private DispatcherTimer PoolDispatcherTimer { get; set; }
		private static readonly int PoolTimerTick = 5;
		private PoolType? PoolType { get; set; }

		public PricesControl()
		{
			InitializeComponent();

			CurrencySelect.ItemsSource = Enum.GetValues(typeof(PoolType)).Cast<PoolType>();

			BackgroundWorker = new BackgroundWorker
			{
				WorkerSupportsCancellation = true
			};

			BackgroundWorker.DoWork += BackgroundWorker_DoWork;
			BackgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

			PoolDispatcherTimer = new DispatcherTimer();
			PoolDispatcherTimer.Tick += PoolDispatcherTimer_Tick; ;
			PoolDispatcherTimer.Interval = new TimeSpan(0, 0, 0);

			if (Properties.Settings.Default.Currency > 0)
			{
				PoolType = (PoolType)Properties.Settings.Default.Currency;
				CurrencySelect.SelectedItem = PoolType;

				PoolDispatcherTimer.Start();
			}
		}

		public void SetCurrency(PoolType poolType)
		{
			PoolType = poolType;

			Properties.Settings.Default.Currency = (int)poolType;
			Properties.Settings.Default.Save();

			if (!PoolDispatcherTimer.IsEnabled)
			{
				PoolDispatcherTimer.Start();
			}
		}

		private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Result is NanopoolApi.Response.Prices result)
			{
				if (result.Status)
				{
					SetPrices(result.Data);
				}
				else if (!string.IsNullOrWhiteSpace(result.Error))
				{
					SetStatusMessage(result.Error);
				}
			}
		}

		private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var nanopool = new Nanopool(PoolType.Value, GetProxy());
			var result = nanopool.GetPrices();

			e.Result = result;
		}

		private void PoolDispatcherTimer_Tick(object sender, EventArgs e)
		{
			PoolDispatcherTimer.Interval = new TimeSpan(0, 0, PoolTimerTick);

			if (!BackgroundWorker.IsBusy && PoolType != null)
			{
				BackgroundWorker.RunWorkerAsync();
			}
		}

		private void SetPrices(Prices prices)
		{
			if (prices != null)
			{
				PriceBtc.Content = prices.Btc.ToString("0.00");
				PriceUsd.Content = prices.Usd.ToString("0.00");
				PriceEur.Content = prices.Eur.ToString("0.00");
				PriceRur.Content = prices.Rur.ToString("0.00");
				PriceCny.Content = prices.Cny.ToString("0.00");
			}
		}

		private void CurrencySelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var dropdown = (ComboBox)sender;
			var currency = (PoolType)dropdown.SelectedItem;

			SetCurrency(currency);
		}
	}
}