using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace GrandMutus.Data
{





	// ↓こんなのいるの？むしろsnippetで対応すべき案件ではないのか？
	public abstract class PropertyChangedCache<P> : IOperationCache
	{
		// Songもジェネリックにして，リフレクションを使ってプロパティをsetすることもできるが，
		// そこまでする必要はありますかねぇ？

		//Song _song;
		//string _propertyName;
		protected P _previousValue;
		protected P _currentValue;

		protected PropertyChangedCache(P from, P to)
		{
			//this._song = song;
			//this._propertyName = propertyName;
			this._previousValue = from;
			this._currentValue = to;
		}

		public abstract void Do();
		public abstract void Reverse();
		public abstract bool CanCancelWith(IOperationCache other);
		public abstract void GetInverse();
	}

	public class SongTitleChangedCache : PropertyChangedCache<string>
	{
		Song _song;

		public SongTitleChangedCache(Song song, string from, string to) : base(from, to)
		{
			this._song = song;
		}

		// DoとかReverseで実行するときにはOperationCacheを新規作成したくないわけだが...

		// →考えられる方法は2つ．
		// 1つは，通常のプロパティのsetterで値をセットするんだけど，キャッシュの作成を抑止する．
		// もう1つは，キャッシュを作成せずに値をセットする別の機構(internalメソッドか？)を用意する．
		// 並列実行の対応も気になりますが…


		public override void Do()
		{
			_song.Title = _currentValue;
		}

		public override void Reverse()
		{
			_song.Title = _previousValue;
		}

		// そもそもoperationCache.Reverse(); だけでアンドゥできる仕組みだったのに，
		// 実装側のコードが複雑になってしまっては意味がないのではないか？

		public bool CanCancelWith(IOperationCache other)
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

		public IOperationCache GetInverse()
		{
			return new SongTitleChangedCache(this._song, this._currentValue, this._previousValue);
		}

	}
}
