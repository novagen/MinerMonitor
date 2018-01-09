using Monitor.Models;
using NanopoolApi;
using NanopoolApi.Data;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Monitor.Controls
{
	/// <summary>
	/// Interaction logic for AccountControl.xaml
	/// </summary>
	public partial class PoolControl : LoaderControl
	{
		private Pool Pool { get; set; }
		private DispatcherTimer PoolDispatcherTimer { get; set; }
		private static readonly int PoolTimerTick = 5;
		private GeneralInfo GeneralInfo { get; set; }

		public PoolControl() : base()
		{
			InitializeComponent();

			BackgroundWorker = new BackgroundWorker
			{
				WorkerSupportsCancellation = true
			};

			GeneralInfo = new GeneralInfo();

			AccountHashrate.DataContext = GeneralInfo;
			AccountBalance.DataContext = GeneralInfo;
			AccountUnconfirmedBalance.DataContext = GeneralInfo;
			AccountWorkers.DataContext = GeneralInfo;

			BackgroundWorker.DoWork += BackgroundWorker_DoWork;
			BackgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

			PoolDispatcherTimer = new DispatcherTimer();
			PoolDispatcherTimer.Tick += PoolDispatcherTimer_Tick;
			PoolDispatcherTimer.Interval = new TimeSpan(0, 0, 0);
		}

		private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Result is NanopoolApi.Response.GeneralInfo result)
			{
				if (result.Status)
				{
					UpdateValues(result.Data);
				}
				else if (!string.IsNullOrWhiteSpace(result.Error))
				{
					SetStatusMessage(result.Error);
				}
				else
				{
					SetStatusMessage("Unkown error");
				}
			}
		}

		private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var nanopool = new Nanopool(Pool.Type, GetProxy());
			var result = nanopool.GetGeneralInfo(Pool.Wallet);
			e.Result = result;
		}

		private void PoolDispatcherTimer_Tick(object sender, EventArgs e)
		{
			PoolDispatcherTimer.Interval = new TimeSpan(0, 0, PoolTimerTick);

			if (!BackgroundWorker.IsBusy)
			{
				BackgroundWorker.RunWorkerAsync();
			}
		}

		public void SetPool(Pool pool)
		{
			if (Pool != null && Pool.Id == pool.Id)
			{
				return;
			}

			if (PoolDispatcherTimer.IsEnabled)
			{
				PoolDispatcherTimer.Stop();
			}

			if (BackgroundWorker.IsBusy)
			{
				BackgroundWorker.CancelAsync();
			}

			Pool = pool;

			AccountName.Content = Pool.Name;

			PoolDispatcherTimer.Interval = new TimeSpan(0, 0, 0);
			PoolDispatcherTimer.Start();
		}

		private void UpdateValues(GeneralInfo data)
		{
			if (data != null)
			{
				GeneralInfo.Hashrate = data.Hashrate;
				GeneralInfo.Balance = data.Balance;
				GeneralInfo.UnconfirmedBalance = data.UnconfirmedBalance;
				GeneralInfo.Workers = data.Workers;

				HideLoader();
			}
			else
			{
				SetStatusMessage("Data failure");
			}
		}

		public override void LoaderControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender != null && Pool != null)
			{
				if (!((bool)e.NewValue))
				{
					PoolDispatcherTimer.Stop();
				}
				else
				{
					PoolDispatcherTimer.Start();
				}
			}
		}
	}
}