using Monitor.Controls;
using Monitor.Models;
using NanopoolApi;
using NanopoolApi.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Monitor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : AppWindow
	{
		private const int PoolTimerTick = 5;
		private const int ErrorMessageTimerTick = 10;

		private readonly object NodeLock = new object();

		private Context Context { get; set; }
		private DispatcherTimer PricesDispatcherTimer { get; set; }
		private DispatcherTimer PoolDispatcherTimer { get; set; }
		private DispatcherTimer MinerDispatcherTimer { get; set; }
		private DispatcherTimer ErrorMessageTimer { get; set; }

		private List<Pool> Pools { get; set; }
		private List<Miner> Miners { get; set; }
		private Proxy Proxy { get; set; }
		private TreeNodes Nodes { get; set; }
		protected BackgroundWorker MinerBackgroundWorker { get; set; }
		protected BackgroundWorker PoolBackgroundWorker { get; set; }
		private TreeNode LastRightClickTreeNode { get; set; }

		public MainWindow()
		{
			InitializeComponent();
			RestoreWindowState(MainGrid);

			ErrorMessageTimer = new DispatcherTimer();
			ErrorMessageTimer.Tick += ErrorMessageTimer_Tick;
			ErrorMessageTimer.Interval = new System.TimeSpan(0, 0, ErrorMessageTimerTick);

			MinerBackgroundWorker = new BackgroundWorker
			{
				WorkerSupportsCancellation = true
			};

			PoolBackgroundWorker = new BackgroundWorker
			{
				WorkerSupportsCancellation = true
			};

			MinerBackgroundWorker.DoWork += MinerBackgroundWorker_DoWork;
			MinerBackgroundWorker.RunWorkerCompleted += MinerBackgroundWorker_RunWorkerCompleted;

			PoolBackgroundWorker.DoWork += PoolBackgroundWorker_DoWork;
			PoolBackgroundWorker.RunWorkerCompleted += PoolBackgroundWorker_RunWorkerCompleted;

			Context = new Context();

			Pools = new List<Pool>();
			Miners = new List<Miner>();

			Nodes = new TreeNodes
			{
				new TreeNode
				{
					Name = TreeNode.RootNodes.Pools,
					Key = TreeNode.RootNodes.Pools,
					Type = NodeType.Group
				},
				new TreeNode
				{
					Name = TreeNode.RootNodes.Miners,
					Key = TreeNode.RootNodes.Miners,
					Type = NodeType.Group
				}
			};

			PoolList.DataContext = Nodes;
			LoadProxy();
		}

		private void UpdateMenu()
		{
			lock (NodeLock)
			{
				foreach (var miner in Miners)
				{
					var current = Nodes.FirstOrDefault(c => c.Key == TreeNode.RootNodes.Miners).Children.FirstOrDefault(c => c.Type == NodeType.Miner && c.Key == miner.Id.ToString());

					if (current == null)
					{
						var node = new TreeNode
						{
							Name = miner.Name,
							Type = NodeType.Miner,
							Data = miner,
							Key = miner.Id.ToString()
						};

						Nodes.FirstOrDefault(c => c.Key == TreeNode.RootNodes.Miners).Children.Add(node);
					}
				}

				var poolNode = Nodes.FirstOrDefault(c => c.Key == TreeNode.RootNodes.Pools);
				var missingPoolKeys = poolNode.Children.Select(c => c.Key).Where(c => !Pools.Select(a => a.Id.ToString()).Contains(c)).ToList();

				foreach (var key in missingPoolKeys)
				{
					var item = poolNode.Children.FirstOrDefault(c => c.Key == key);

					if (item != null)
					{
						poolNode.Children.Remove(item);
					}
				}

				foreach (var pool in Pools)
				{
					var current = Nodes.FirstOrDefault(c => c.Key == TreeNode.RootNodes.Pools).Children.FirstOrDefault(c => c.Type == NodeType.Pool && c.Key == pool.Wallet);

					if (current == null)
					{
						var node = new TreeNode
						{
							Name = pool.Name,
							Type = NodeType.Pool,
							Data = pool,
							Key = pool.Wallet
						};

						foreach (var worker in pool.Workers)
						{
							node.Children.Add(new TreeNode
							{
								Name = worker.Id,
								Type = NodeType.Worker,
								Data = worker,
								Key = worker.Uid.ToString()
							});
						}

						Nodes.FirstOrDefault(c => c.Key == TreeNode.RootNodes.Pools).Children.Add(node);
					}
					else
					{
						foreach (var worker in pool.Workers)
						{
							TreeNode currentChild = null;

							if (current.Children.Any())
							{
								currentChild = current.Children.FirstOrDefault(c => c.Type == NodeType.Worker && c.Key == worker.Uid.ToString());
							}

							if (currentChild == null)
							{
								current.Children.Add(new TreeNode
								{
									Name = worker.Id,
									Type = NodeType.Worker,
									Data = worker,
									Key = worker.Uid.ToString()
								});
							}
							else
							{
								currentChild.Data = worker;
							}
						}
					}
				}
			}
		}

		private void StartPools()
		{
			PoolDispatcherTimer = new DispatcherTimer();
			PoolDispatcherTimer.Tick += PoolDispatcherTimer_Tick;
			PoolDispatcherTimer.Interval = new System.TimeSpan(0, 0, 0);
			PoolDispatcherTimer.Start();
		}

		private void StartMiners()
		{
			MinerDispatcherTimer = new DispatcherTimer();
			MinerDispatcherTimer.Tick += MinerDispatcherTimer_Tick;
			MinerDispatcherTimer.Interval = new System.TimeSpan(0, 0, 0);
			MinerDispatcherTimer.Start();
		}

		private void SetGroupView(TreeNode node)
		{
			switch (node.Key)
			{
				case TreeNode.RootNodes.Miners:
					UserControlMiners.Visibility = Visibility.Visible;
					break;

				case TreeNode.RootNodes.Pools:
					UserControlPools.Visibility = Visibility.Visible;
					break;
			}
		}

		private void LoadProxy()
		{
			Proxy = Context.Proxies.FirstOrDefault();
		}

		private void SetProxy()
		{
			var window = new ControlWindow("Set proxy", new EditProxyControl(Proxy))
			{
				Owner = this
			};

			Proxy proxy = null;

			if (window.ShowDialog() == true)
			{
				var result = window.Result;

				if (result == ControlWindowResult.OK)
				{
					proxy = window.GetValue<Proxy>();

					if (Proxy == null)
					{
						proxy.Id = Guid.NewGuid();
						Context.Proxies.Add(proxy);
					}
					else
					{
						if (!string.IsNullOrWhiteSpace(proxy.Url) && !string.IsNullOrWhiteSpace(proxy.Username) && !string.IsNullOrWhiteSpace(proxy.Password))
						{
							Proxy.Password = proxy.Password;
							Proxy.Username = proxy.Username;
							Proxy.Url = proxy.Url;

							Context.Entry(Proxy).State = EntityState.Modified;
						}
						else
						{
							Context.Proxies.Remove(Proxy);
							proxy = null;
						}
					}
				}

				Proxy = proxy;
				Context.SaveChanges();
			}
		}

		private void CollapseAllViews()
		{
			MainView.Visibility = Visibility.Collapsed;
			UserControlMiners.Visibility = Visibility.Collapsed;
			UserControlPools.Visibility = Visibility.Collapsed;
			UserControlWorker.Visibility = Visibility.Collapsed;
			UserControlPool.Visibility = Visibility.Collapsed;
			UserControlMiner.Visibility = Visibility.Collapsed;
		}

		private void UpdatePoolListContextMenu(TreeViewItem treeViewItem)
		{
			var item = (TreeNode)treeViewItem.DataContext;
			LastRightClickTreeNode = item;

			if (item != null)
			{
				switch (item.Type)
				{
					case NodeType.Pool:
						PoolList.ContextMenu = PoolList.Resources["PoolContext"] as ContextMenu;
						break;
					//case NodeType.Worker:
					//	PoolList.ContextMenu = PoolList.Resources["WorkerContext"] as ContextMenu;
					//	break;
					case NodeType.Miner:
						PoolList.ContextMenu = PoolList.Resources["MinerContext"] as ContextMenu;
						break;

					case NodeType.Group:
						switch (item.Key)
						{
							case TreeNode.RootNodes.Miners:
								PoolList.ContextMenu = PoolList.Resources["MinersContext"] as ContextMenu;
								break;

							case TreeNode.RootNodes.Pools:
								PoolList.ContextMenu = PoolList.Resources["PoolsContext"] as ContextMenu;
								break;

							default:
								PoolList.ContextMenu = null;
								break;
						}
						break;

					default:
						PoolList.ContextMenu = null;
						break;
				}
			}
		}

		#region Workers

		private void MinerBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			var result = e.Result as bool?;
			if (result.Value)
			{
				UpdateMenu();
			}
		}

		private void MinerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			bool? modified = false;

			using (var context = new Context())
			{
				foreach (var miner in context.Miners)
				{
					if (!Miners.Any(c => c.Id == miner.Id))
					{
						Miners.Add(miner);
						modified = true;
					}
				}
			}

			e.Result = modified;
		}

		private void PoolBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			var result = e.Result as bool?;
			if (result.Value)
			{
				UpdateMenu();
			}
		}

		private void PoolBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			bool? modified = false;

			using (var context = new Context())
			{
				foreach (var pool in context.Pools)
				{
					if (!Pools.Any(c => c.Id == pool.Id))
					{
						Pools.Add(pool);
						modified = true;
					}

					var currentPool = Pools.FirstOrDefault(c => c.Id == pool.Id);

					var nanopool = new Nanopool(pool.Type, GetWebProxy());
					var result = nanopool.GetListOfWorkers(pool.Wallet);

					if (result != null && result.Status)
					{
						var newWorkers = new List<Worker>();
						var removed = 0;

						if (pool.Workers.Any())
						{
							removed = pool.Workers.RemoveAll(x => !result.Data.Exists(y => y.Uid == x.Uid));
						}

						foreach (var worker in result.Data)
						{
							Worker current = null;

							if (currentPool.Workers.Any())
							{
								current = currentPool.Workers.FirstOrDefault(c => c.Uid == worker.Uid);
							}

							if (current == null)
							{
								newWorkers.Add(worker);
							}
							else
							{
								worker.UpdateOther(current);
							}
						}

						if (removed > 0)
						{
							modified = true;
						}

						if (newWorkers.Any())
						{
							currentPool.Workers.AddRange(newWorkers);
							modified = true;
						}
					}
				}
			}

			e.Result = modified;
		}

		private void MinerDispatcherTimer_Tick(object sender, EventArgs e)
		{
			MinerDispatcherTimer.Interval = new TimeSpan(0, 0, PoolTimerTick);

			if (!MinerBackgroundWorker.IsBusy)
			{
				MinerBackgroundWorker.RunWorkerAsync();
			}
		}

		private void PoolDispatcherTimer_Tick(object sender, EventArgs e)
		{
			PoolDispatcherTimer.Interval = new TimeSpan(0, 0, PoolTimerTick);

			if (!PoolBackgroundWorker.IsBusy)
			{
				PoolBackgroundWorker.RunWorkerAsync();
			}
		}

		private void ErrorMessageTimer_Tick(object sender, EventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				StatusMessage.Content = " ";
			});

			ErrorMessageTimer.Stop();
		}

		#endregion Workers

		#region Public methods

		/// <summary>
		/// Enable or disable to loader
		/// </summary>
		/// <param name="active"></param>
		public void SetLoader(bool active)
		{
			BusyIndicator.IsBusy = active;
		}

		/// <summary>
		/// Set the message to show on the status bar
		/// </summary>
		/// <param name="message"></param>
		public void SetStatusMessage(string message)
		{
			StatusMessage.Content = message;

			if (ErrorMessageTimer.IsEnabled)
			{
				ErrorMessageTimer.Stop();
			}

			ErrorMessageTimer.Start();
		}

		/// <summary>
		/// Get proxy to use in remote calls
		/// </summary>
		/// <returns></returns>
		public WebProxy GetWebProxy()
		{
			if (Proxy != null)
			{
				return new WebProxy(Proxy.Url, true)
				{
					Credentials = new NetworkCredential(Proxy.Username, Proxy.Password)
				};
			}

			return null;
		}

		#endregion Public methods

		#region Model methods

		private Miner AddMiner()
		{
			var window = new ControlWindow("Add miner", new EditMinerControl(null))
			{
				Owner = this
			};

			Miner miner = null;

			if (window.ShowDialog() == true)
			{
				var result = window.Result;

				if (result == ControlWindowResult.OK)
				{
					miner = window.GetValue<Miner>();
					miner.Id = Guid.NewGuid();

					try
					{
						Context.Miners.Add(miner);
						Context.SaveChanges();
					}
					catch (Exception)
					{
						return null;
					}
				}
			}

			return miner;
		}

		private bool EditMiner(Miner miner)
		{
			var window = new ControlWindow("Edit miner", new EditMinerControl(miner))
			{
				Owner = this
			};

			if (window.ShowDialog() == true)
			{
				var result = window.Result;

				try
				{
					Context.Entry(miner).State = EntityState.Modified;
					Context.SaveChanges();
				}
				catch (Exception)
				{
					return false;
				}

				return true;
			}

			return false;
		}

		private bool RemoveMiner(Miner miner)
		{
			var removeItem = Context.Miners.FirstOrDefault(c => c.Id == miner.Id);

			if (removeItem != null)
			{
				try
				{
					Context.Miners.Remove(removeItem);
					Context.SaveChanges();

					lock (NodeLock)
					{
						Miners.Remove(miner);

						var node = Nodes.FirstOrDefault(c => c.Key == TreeNode.RootNodes.Miners);

						if (node != null)
						{
							var item = node.Children.FirstOrDefault(c => c.Key == miner.Id.ToString());

							if (item != null)
							{
								node.Children.Remove(item);
							}
						}
					}
				}
				catch (Exception)
				{
					return false;
				}

				return true;
			}

			return false;
		}

		private Pool AddPool()
		{
			var window = new ControlWindow("Add pool", new EditPoolControl(null))
			{
				Owner = this
			};

			Pool pool = null;

			if (window.ShowDialog() == true)
			{
				var result = window.Result;

				if (result == ControlWindowResult.OK)
				{
					pool = window.GetValue<Pool>();
					pool.Id = Guid.NewGuid();

					try
					{
						Context.Pools.Add(pool);
						Context.SaveChanges();
					}
					catch (Exception)
					{
						return null;
					}
				}
			}

			return pool;
		}

		private bool EditPool(Pool pool)
		{
			var window = new ControlWindow("Edit pool", new EditPoolControl(pool))
			{
				Owner = this
			};

			if (window.ShowDialog() == true)
			{
				var result = window.Result;

				try
				{
					Context.Entry(pool).State = EntityState.Modified;
					Context.SaveChanges();
				}
				catch (Exception)
				{
					return false;
				}

				return true;
			}

			return false;
		}

		private bool RemovePool(Pool pool)
		{
			var removeItem = Context.Pools.FirstOrDefault(c => c.Id == pool.Id);

			if (removeItem != null)
			{
				try
				{
					Context.Pools.Remove(removeItem);
					Context.SaveChanges();

					lock (NodeLock)
					{
						Pools.Remove(pool);

						var node = Nodes.FirstOrDefault(c => c.Key == TreeNode.RootNodes.Pools);

						if (node != null)
						{
							var item = node.Children.FirstOrDefault(c => c.Key == pool.Wallet);

							if (item != null)
							{
								node.Children.Remove(item);
							}
						}
					}
				}
				catch (Exception)
				{
					return false;
				}

				return true;
			}

			return false;
		}

		#endregion Model methods

		#region Handlers

		private void PoolList_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var treeViewItem = Helpers.VisualUpwardSearch(e.OriginalSource as DependencyObject);
			LastRightClickTreeNode = null;

			if (treeViewItem != null)
			{
				UpdatePoolListContextMenu(treeViewItem);
				e.Handled = true;
			}
		}

		private void PoolList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			var list = sender as TreeView;
			var item = (TreeNode)list.SelectedItem;

			CollapseAllViews();

			switch (item.Type)
			{
				case NodeType.Pool:
					UserControlPool.SetPool(item.GetData<Pool>());
					UserControlPool.Visibility = Visibility.Visible;
					break;

				case NodeType.Worker:
					var pool = Pools.FirstOrDefault(c => c.Workers.Contains(item.GetData<Worker>()));

					UserControlWorker.SetWorker(pool, item.GetData<Worker>());
					UserControlWorker.Visibility = Visibility.Visible;
					break;

				case NodeType.Miner:
					UserControlMiner.SetMiner(item.GetData<Miner>());
					UserControlMiner.Visibility = Visibility.Visible;
					break;

				case NodeType.Group:
					SetGroupView(item);
					break;

				default:
					break;
			}
		}

		private void MenuItemExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void MenuItemAddPool_Click(object sender, RoutedEventArgs e)
		{
			AddPool();
		}

		private void MenuItemAddMiner_Click(object sender, RoutedEventArgs e)
		{
			AddMiner();
		}

		private void MenuItemSetProxy_Click(object sender, RoutedEventArgs e)
		{
			SetProxy();
		}

		private void MenuMinerEdit_Click(object sender, RoutedEventArgs e)
		{
			if (LastRightClickTreeNode != null)
			{
				EditMiner(LastRightClickTreeNode.GetData<Miner>());
			}
		}

		private void MenuMinerRemove_Click(object sender, RoutedEventArgs e)
		{
			if (LastRightClickTreeNode != null)
			{
				RemoveMiner(LastRightClickTreeNode.GetData<Miner>());
			}
		}

		private void MenuPoolEdit_Click(object sender, RoutedEventArgs e)
		{
			if (LastRightClickTreeNode != null)
			{
				EditPool(LastRightClickTreeNode.GetData<Pool>());
			}
		}

		private void MenuPoolRemove_Click(object sender, RoutedEventArgs e)
		{
			if (LastRightClickTreeNode != null)
			{
				RemovePool(LastRightClickTreeNode.GetData<Pool>());
			}
		}

		private void MenuPoolsAdd_Click(object sender, RoutedEventArgs e)
		{
			AddPool();
		}

		private void MenuMinersAdd_Click(object sender, RoutedEventArgs e)
		{
			AddMiner();
		}

		#endregion Handlers

		#region Overrides

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			StartPools();
			StartMiners();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (PoolDispatcherTimer.IsEnabled)
			{
				PoolDispatcherTimer.Stop();
			}

			if (MinerDispatcherTimer.IsEnabled)
			{
				MinerDispatcherTimer.Stop();
			}

			if (ErrorMessageTimer.IsEnabled)
			{
				ErrorMessageTimer.Stop();
			}

			SaveWindowState(MainGrid);

			base.OnClosing(e);
		}

		#endregion Overrides
	}
}