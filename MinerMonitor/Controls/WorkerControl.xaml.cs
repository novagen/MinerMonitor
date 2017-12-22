using Monitor.Models;
using NanopoolApi;
using NanopoolApi.Data;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Monitor.Controls
{
	/// <summary>
	/// Interaction logic for WorkerControl.xaml
	/// </summary>
	public partial class WorkerControl : LoaderControl
	{
		private Pool Account { get; set; }
		private Worker Worker { get; set; }
		private DispatcherTimer PoolDispatcherTimer { get; set; }
		private static readonly int PoolTimerTick = 5;

		public WorkerControl()
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
			if (e.Result is NanopoolApi.Response.ListOfWorkers result)
			{
				if (result.Status)
				{
					UpdateValues(result.Data.FirstOrDefault(c => c.Uid == Worker.Uid));
				}
				else if (!string.IsNullOrWhiteSpace(result.Error))
				{
					SetStatusMessage(result.Error);
				}
			}
		}

		private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var nanopool = new Nanopool(Account.Type, GetProxy());
			var result = nanopool.GetListOfWorkers(Account.Wallet);

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

		public void SetWorker(Pool account, Worker worker)
		{
			Debug.WriteLine(worker);

			Account = account;
			Worker = worker;

			PoolDispatcherTimer.Interval = new TimeSpan(0, 0, 0);
		}

		private void UpdateValues(Worker data)
		{
			if (data != null)
			{
				WorkerName.Content = data.Id;
				WorkerHashrate.Content = data.Hashrate.ToString();
				WorkerRating.Content = data.Rating.ToString();
				WorkerLastShare.Content = data.LastShare.ToDate().ToString();
			}
		}

		public override void LoaderControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender != null && Account != null)
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