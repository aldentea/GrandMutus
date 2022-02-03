using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aldentea.Wpf.Document;

namespace GrandMutus.Net6.Data
{

	public abstract class PlayerPropertyChangedCache<T> : PropertyChangedCache<T>
	{
		protected Player _player;

		public PlayerPropertyChangedCache(Player player, T from, T to)
			: base(from, to)
		{
			this._player = player;
		}

	}


	public class PlayerNameChangedCache : PlayerPropertyChangedCache<string>
	{
		public PlayerNameChangedCache(Player player, string from, string to)
			: base(player, from, to)
		{ }

		public override void Reverse()
		{
			_player.Name = _previousValue;
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			var other_cache = other as PlayerNameChangedCache;
			if (other_cache == null)
			{ return false; }
			else
			{
				return other_cache._player == this._player &&
					other_cache._previousValue == this._currentValue &&
					other_cache._currentValue == this._previousValue;
			}
		}

	}

	public abstract class PlayerDecimalPropertyChangedCache : PlayerPropertyChangedCache<decimal>
	{
		public PlayerDecimalPropertyChangedCache(Player player, decimal from, decimal to)
			: base(player, from, to)
		{
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			if (this.GetType() != other.GetType())
			{ return false; }

			var other_cache = (PlayerDecimalPropertyChangedCache)other;
				return other_cache._player == this._player &&
					other_cache._previousValue == this._currentValue &&
					other_cache._currentValue == this._previousValue;
		}

	}

	public class PlayerMaruChangedCache : PlayerDecimalPropertyChangedCache
	{
		public PlayerMaruChangedCache(Player player, decimal from, decimal to)
			: base(player, from, to)
		{
		}

		public override void Reverse()
		{
			_player.Maru = _previousValue;
		}

	}

	public class PlayerBatsuChangedCache : PlayerDecimalPropertyChangedCache
	{
		public PlayerBatsuChangedCache(Player player, decimal from, decimal to)
			: base(player, from, to)
		{
		}

		public override void Reverse()
		{
			_player.Batsu = _previousValue;
		}

	}

	public class PlayerScoreChangedCache : PlayerDecimalPropertyChangedCache
	{
		public PlayerScoreChangedCache(Player player, decimal from, decimal to)
			: base(player, from, to)
		{
		}

		public override void Reverse()
		{
			_player.Score = _previousValue;
		}

	}

}
