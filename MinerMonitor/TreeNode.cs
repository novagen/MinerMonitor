using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Monitor
{
	public class TreeNode : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private string _name;
		private object _data;
		private string _key;
		private NodeType _type;

		public string Name
		{
			get
			{
				return this._name;
			}

			set
			{
				if (value != this._name)
				{
					this._name = value;
					NotifyPropertyChanged();
				}
			}
		}

		public object Data
		{
			get
			{
				return this._data;
			}

			set
			{
				if (value != this._data)
				{
					this._data = value;
					NotifyPropertyChanged();
				}
			}
		}

		public string Key
		{
			get
			{
				return this._key;
			}

			set
			{
				if (value != this._key)
				{
					this._key = value;
					NotifyPropertyChanged();
				}
			}
		}

		public NodeType Type
		{
			get
			{
				return this._type;
			}

			set
			{
				if (value != this._type)
				{
					this._type = value;
					NotifyPropertyChanged();
				}
			}
		}

		public ObservableCollection<TreeNode> Children { get; set; }

		public TreeNode()
		{
			Children = new ObservableCollection<TreeNode>();
		}

		public TreeNode(string name, object data, NodeType type)
		{
			Name = name;
			Data = data;
			Type = type;
			Children = new ObservableCollection<TreeNode>();
			Children.CollectionChanged += Children_CollectionChanged;
		}

		private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			NotifyPropertyChanged();
		}

		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public T GetData<T>()
		{
			if (Data != null)
			{
				return (T)Data;
			}

			return default(T);
		}

		public bool HasData
		{
			get
			{
				return Data != null;
			}
		}

		public bool HasChildren
		{
			get
			{
				return Children.Any();
			}
		}

		public TreeNode GetChild(string key)
		{
			var child = Children.FirstOrDefault(c => c.Key == key);

			if (child == null && HasChildren)
			{
				foreach (var node in Children)
				{
					child = node.GetChild(key);

					if (child != null)
					{
						break;
					}
				}
			}

			return child;
		}

		public TreeNode GetChild(string key, NodeType type)
		{
			var child = Children.FirstOrDefault(c => c.Key == key && c.Type == type);

			if (child == null && HasChildren)
			{
				foreach (var node in Children)
				{
					child = node.GetChild(key, type);

					if (child != null)
					{
						break;
					}
				}
			}

			return child;
		}

		public static class RootNodes
		{
			public const string Miners = "Miners";
			public const string Pools = "Pools";
		}
	}
}