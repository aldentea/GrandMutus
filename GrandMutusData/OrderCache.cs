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

	// (0.9.0)名前をMutusGameDocumentCacheに変更。 
	// (0.8.1)
	public abstract class MutusGameDocumentCache : IOperationCache
	{
		public IMutusGameDocument Document { get; protected set; }

		protected MutusGameDocumentCache(IMutusGameDocument document)
		{
			this.Document = document;
		}

		public abstract bool CanCancelWith(IOperationCache other);
		public abstract void Reverse();

	}

	// (0.3.1.1)
	#region AddOrderCacheクラス
	public class AddOrderCache : MutusGameDocumentCache
	{
		public int? QuestionID { get; protected set; }

		// (0.8.1.2)questionID引数を追加。
		public AddOrderCache(IMutusGameDocument document, int? questionID) : base(document)
		{
			this.QuestionID = questionID;
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			if (other is RemoveOrderCache)
			{
				var other_cache = (RemoveOrderCache)other;
				return other_cache.Document == this.Document && other_cache.QuestionID == this.QuestionID;
			}
			else
			{
				return false;
			}
		}

		public override void Reverse()
		{
			Document.RemoveOrder();
		}
	}
	#endregion

	// (0.3.1.1)
	#region RemoveOrderCacheクラス
	public class RemoveOrderCache : MutusGameDocumentCache
	{
		public int? QuestionID { get; protected set; }

		// (0.8.1.2)questionID引数を追加。
		public RemoveOrderCache(IMutusGameDocument document, int? questionID) : base(document)
		{
			this.QuestionID = questionID;
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			if (other is AddOrderCache)
			{
				var other_cache = (AddOrderCache)other;
				return other_cache.Document == this.Document && other_cache.QuestionID == this.QuestionID;
			}
			else
			{
				return false;
			}
		}

		public override void Reverse()
		{
			Document.AddOrder(QuestionID);
		}
	}
	#endregion
}
