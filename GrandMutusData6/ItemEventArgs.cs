using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Net6.Data
{
	// ヘルパー的．
	public class ItemEventArgs<T> : EventArgs
	{
		public T? Item { get; set; }

	}
}
