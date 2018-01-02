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

		public PoolControl() : base()
		{
			InitializeComponent();

			BackgroundWorker = new BackgroundWorker
			{
				WorkerSupportsCancellation = true
			};

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
					HideLoader();
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
			if (Pool != null && pool.Id == Pool.Id)
			{
				return;
			}

			ShowLoader();

			Pool = pool;
			PoolDispatcherTimer.Interval = new TimeSpan(0, 0, 0);

			AccountName.Content = pool.Name;
		}

		private void UpdateValues(GeneralInfo data)
		{
			if (data != null)
			{
				AccountHashrate.Content = data.Hashrate.ToString();
				AccountBalance.Content = data.Balance.ToString();
				AccountUnconfirmedBalance.Content = data.UnconfirmedBalance.ToString();
				AccountWorkers.Content = data.Workers.Count().ToString();

				HideLoader();
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