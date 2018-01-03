using Monitor.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using XmrStakApi;

namespace Monitor.Controls
{
	/// <summary>
	/// Interaction logic for MinerControl.xaml
	/// </summary>
	public partial class MinerControl : LoaderControl
	{
		private Models.Miner Miner { get; set; }
		private DispatcherTimer PoolDispatcherTimer { get; set; }
		private static readonly int PoolTimerTick = 5;
		private bool Update { get; set; }

		public MinerControl()
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
			if (e.Result != null  && e.Result is MinerResponse result)
			{
				if (result.Status)
				{
					UpdateValues(result);
				}
				else if (result.Error != null)
				{
					SetStatusMessage(result.Error.Message ?? "Unkown error");
				}
				else
				{
					SetStatusMessage("Unkown error");
				}
			}
			else
			{
				SetStatusMessage("Result failure");
			}
		}

		private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			Update = true;
			var xmrStak = new XmrStak(GetProxy(), 10);
			e.Result = xmrStak.GetData(Miner.GetStakMiner());
		}

		private void PoolDispatcherTimer_Tick(object sender, EventArgs e)
		{
			PoolDispatcherTimer.Interval = new TimeSpan(0, 0, PoolTimerTick);

			if (!BackgroundWorker.IsBusy)
			{
				BackgroundWorker.RunWorkerAsync();
			}
		}

		public void SetMiner(Models.Miner miner)
		{
			if (Miner != null && Miner.Id == miner.Id)
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

			Miner = miner;

			PoolDispatcherTimer.Interval = new TimeSpan(0, 0, 0);
			PoolDispatcherTimer.Start();
		}

		private void UpdateValues(MinerResponse response)
		{
			if (response.Data != null)
			{
				MinerName.Content = Miner.Name;
				MinerHashrate.Content = response.Data.Hashrate.Total.First();

				HideLoader();
			}
			else
			{
				SetStatusMessage("Data failure");
			}
		}

		public override void LoaderControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender != null && Miner != null)
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