using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Data
{
	// (0.2.0)
	#region SongTitleChangedCacheクラス
	public class SongTitleChangedCache : PropertyChangedCache<string>
	{
		Song _song;

		public SongTitleChangedCache(Song song, string from, string to)
			: base(from, to)
		{
			this._song = song;
		}

		// DoとかReverseで実行するときにはOperationCacheを新規作成したくないわけだが...

		// →考えられる方法は2つ．
		// 1つは，通常のプロパティのsetterで値をセットするんだけど，キャッシュの作成を抑止する．
		// もう1つは，キャッシュを作成せずに値をセットする別の機構(internalメソッドか？)を用意する．
		// 並列実行の対応も気になりますが…


		//public override void Do()
		//{
		//	_song.Title = _currentValue;
		//}

		public override void Reverse()
		{
			_song.Title = _previousValue;
		}

		// そもそもoperationCache.Reverse(); だけでアンドゥできる仕組みだったのに，
		// 実装側のコードが複雑になってしまっては意味がないのではないか？

		public override bool CanCancelWith(IOperationCache other)
		{
			var other_cache = other as SongTitleChangedCache;
			if (other_cache == null)
			{ return false; }
			else
			{
				return other_cache._song == this._song &&
					other_cache._previousValue == this._currentValue &&
					other_cache._currentValue == this._previousValue;
			}
		}

		//public override IOperationCache GetInverse()
		//{
		//	return new SongTitleChangedCache(this._song, this._currentValue, this._previousValue);
		//}

	}
	#endregion

	// (0.2.0)
	#region SongArtistChangedCacheクラス
	public class SongArtistChangedCache : PropertyChangedCache<string>
	{
		Song _song;

		public SongArtistChangedCache(Song song, string from, string to)
			: base(from, to)
		{
			this._song = song;
		}

		public override void Reverse()
		{
			_song.Artist = _previousValue;
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			var other_cache = other as SongArtistChangedCache;
			if (other_cache == null)
			{ return false; }
			else
			{
				return other_cache._song == this._song &&
					other_cache._previousValue == this._currentValue &&
					other_cache._currentValue == this._previousValue;
			}
		}

	}
	#endregion

	// (0.4.2)
	#region SongArtistChangedCacheクラス
	public class SongSabiPosChangedCache : PropertyChangedCache<TimeSpan>
	{
		Song _song;

		public SongSabiPosChangedCache(Song song, TimeSpan from, TimeSpan to)
			: base(from, to)
		{
			this._song = song;
		}

		public override void Reverse()
		{
			_song.SabiPos = _previousValue;
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			var other_cache = other as SongSabiPosChangedCache;
			if (other_cache == null)
			{ return false; }
			else
			{
				return other_cache._song == this._song &&
					other_cache._previousValue == this._currentValue &&
					other_cache._currentValue == this._previousValue;
			}
		}

	}
	#endregion

}
