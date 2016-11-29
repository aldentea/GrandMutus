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


	// このあたりにIDを入れるかどうかは悩ましい？

	// (0.9.4)
	#region AddLogCacheクラス
	public class AddLogCache : MutusGameDocumentCache
	{
		public int OrderID { get; protected set; }
		public int? PlayerID { get; protected set; }
		public string Code { get; protected set; }
		public decimal Value { get; protected set; }

		public AddLogCache(IMutusGameDocument document, int order_id, string code, decimal value, int? player_id = null) : base(document)
		{
			this.OrderID = order_id;
			this.Code = code;
			this.Value = value;
			this.PlayerID = player_id;
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			if (other is RemoveLogCache)
			{
				var other_cache = (RemoveLogCache)other;
				return other_cache.Document == this.Document && other_cache.OrderID == this.OrderID
					&& other_cache.Log.Code == this.Code
					&& other_cache.Log.Value == this.Value
					&& other_cache.Log.PlayerID == this.PlayerID;
			}
			else
			{
				return false;
			}
		}

		public override void Reverse()
		{
			Document.RemoveLog(OrderID, Code, Value, PlayerID);
		}
	}
	#endregion

	// (0.9.4)
	#region RemoveLogCacheクラス
	public class RemoveLogCache : MutusGameDocumentCache
	{
		public Log Log { get; protected set; }
		public int OrderID { get; protected set; }

		public RemoveLogCache(IMutusGameDocument document, int order_id, Log log) : base(document)
		{
			this.OrderID = order_id;
			this.Log = log;
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			if (other is AddLogCache)
			{
				var other_cache = (AddLogCache)other;
				return other_cache.Document == this.Document && other_cache.OrderID == this.OrderID
					&& (this.Log.PlayerID == other_cache.PlayerID)
					&& (this.Log.Code == other_cache.Code)
					&& (this.Log.Value == other_cache.Value);
			}
			else
			{
				return false;
			}
		}

		public override void Reverse()
		{
			Document.AddLog(this.Log, OrderID);
		}
	}
	#endregion
}
