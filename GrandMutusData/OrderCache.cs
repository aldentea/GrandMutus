using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aldentea.Wpf.Document;

namespace GrandMutus.Data
{

	// これ，OperationCacheの考え方を組み立て直さないといけないのでは？

	// Song.Titleを変更したときのような可逆なものと，
	// Orderを追加するような，基本的には不可逆なものと．

	#region AddOrderCacheクラス
	public class AddOrderCache : IOperationCache
	{
		public IMutusGameDocument Document { get; protected set; }
		//public ISet<Question> Questions
		//{ get; protected set; }
		public Order Order { get; protected set; }

		protected AddOrderCache(IMutusGameDocument document, Order order)
		{
			this.Document = document;
			this.Order = order;
		}

		public void Reverse()
		{
			throw new NotImplementedException();
		}

		public bool CanCancelWith(IOperationCache other)
		{
			throw new NotImplementedException();
		}

	}
	#endregion
}
