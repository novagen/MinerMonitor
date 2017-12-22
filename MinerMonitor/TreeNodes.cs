using System.Collections.Generic;
using System.Linq;

namespace Monitor
{
	public class TreeNodes : List<TreeNode>
	{
		public TreeNode GetChild(string key)
		{
			var child = this.FirstOrDefault(c => c.Key == key);

			if (child == null)
			{
				foreach(var node in this)
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
			var child = this.FirstOrDefault(c => c.Key == key && c.Type == type);

			if (child == null && this.Count() > 0)
			{
				foreach (var node in this)
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
	}
}