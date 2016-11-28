using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aldentea.Wpf.Document;

namespace GrandMutus.Data
{

	// (0.9.0)
	#region [abstract]PlayerCacheクラス
	public abstract class PlayerCache : MutusGameDocumentCache
	{
		public string PlayerName { get; protected set; }

		public PlayerCache(IMutusGameDocument document, string name) : base(document)
		{
			this.PlayerName = name;
		}

	}
	#endregion

	// (0.9.0)
	#region AddOrderCacheクラス
	public class AddPlayerCache : PlayerCache
	{

		public AddPlayerCache(IMutusGameDocument document, string name) : base(document, name)
		{
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			if (other is RemovePlayerCache)
			{
				var other_cache = (RemovePlayerCache)other;
				return other_cache.Document == this.Document && other_cache.PlayerName == this.PlayerName;
			}
			else
			{
				return false;
			}
		}

		public override void Reverse()
		{
			Document.RemovePlayer(PlayerName);
		}
	}
	#endregion

	// (0.9.0)
	#region RemovePlayerCacheクラス
	public class RemovePlayerCache : PlayerCache
	{
		public RemovePlayerCache(IMutusGameDocument document, string name) : base(document, name)
		{
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			if (other is AddPlayerCache)
			{
				var other_cache = (AddPlayerCache)other;
				return other_cache.Document == this.Document && other_cache.PlayerName == this.PlayerName;
			}
			else
			{
				return false;
			}
		}

		public override void Reverse()
		{
			Document.AddPlayer(PlayerName);
		}
	}
	#endregion

}
