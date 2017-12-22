using Monitor.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Monitor.Controls
{
	/// <summary>
	/// Interaction logic for MinerControl.xaml
	/// </summary>
	public partial class MinerControl : LoaderControl
	{
		private Miner Miner { get; set; }
		private DispatcherTimer PoolDispatcherTimer { get; set; }
		private static readonly int PoolTimerTick = 5;

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
			if (e.Result is XmrStakApi.Miner result)
			{
				if (result.Error == null)
				{
					UpdateValues(result);
				}
				else if (result.Error != null)
				{
					SetStatusMessage(result.Error.Message);
				}
			}
		}

		private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var xmrStak = new XmrStakApi.XmrStak(GetProxy());
			var result = xmrStak.GetData(Miner.GetStakMiner());

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

		public void SetMiner(Miner miner)
		{
			Miner = miner;
			PoolDispatcherTimer.Interval = new TimeSpan(0, 0, 0);
		}

		private void UpdateValues(XmrStakApi.Miner data)
		{
			if (data != null && data.Data != null)
			{
				MinerName.Content = Miner.Name;
				MinerHashrate.Content = data.Data.Hashrate.Total;
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