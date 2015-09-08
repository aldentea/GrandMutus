using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Data
{
	// (0.4.0) ファイル名だけではなく、Songオブジェクトを保持するように変更。
	// (0.3.0) ReverseメソッドとCanCancelWithメソッドがabstractです．
	#region [abstract]SongsCacheクラス
	public abstract class SongsCache : IOperationCache
	{
		public MutusDocument Document { get; protected set; }
		public ISet<Song> Songs
		{ get; protected set; }

		protected SongsCache(MutusDocument document, IEnumerable<Song> songs)
		{
			this.Document = document;
			this.Songs = new HashSet<Song>(songs);
		}

		public abstract void Reverse();
		public abstract bool CanCancelWith(IOperationCache other);

		/// <summary>
		/// FileNamesプロパティの中身が同一であればtrueを返します．
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool HasSameSongsWith(SongsCache other)
		{
			if (this.Songs.Count == other.Songs.Count)
			{
				return this.Songs.Select(s => s.ID).Except(other.Songs.Select(s => s.ID)).Count() == 0;
			}
			else
			{
				return false;
			}
		}
	}
	#endregion

	// (0.4.0) ファイル名だけではなく、Songオブジェクトを保持するように変更。
	// (0.3.0)
	#region SongsAddedCacheクラス
	public class SongsAddedCache : SongsCache
	{
		public SongsAddedCache(MutusDocument document, IEnumerable<Song> songs)
			: base(document, songs)
		{ }

		public override void Reverse()
		{
			Document.RemoveSongs(this.Songs);
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			return other is SongsRemovedCache
				&& ((SongsCache)other).Document == this.Document
				&& this.HasSameSongsWith((SongsCache)other);
		}
	}
	#endregion

	// (0.4.0) ファイル名だけではなく、Songオブジェクトを保持するように変更。
	// (0.3.0)
	#region SongsRemovedCacheクラス
	public class SongsRemovedCache : SongsCache
	{
		public SongsRemovedCache(MutusDocument document, IEnumerable<Song> songs)
			: base(document, songs)
		{ }

		public override void Reverse()
		{
			Document.AddSongs(this.Songs);
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			return other is SongsAddedCache
				&& ((SongsCache)other).Document == this.Document
				&& this.HasSameSongsWith((SongsCache)other);
		}
	}
	#endregion

}
