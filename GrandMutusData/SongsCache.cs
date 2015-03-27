using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Data
{

	// (0.3.0) ReverseメソッドとCanCancelWithメソッドがabstractです．
	#region [abstract]SongsCacheクラス
	public abstract class SongsCache : IOperationCache
	{
		public MutusDocument Document { get; protected set; }
		public ISet<string> FileNames
		{ get; protected set; }

		protected SongsCache(MutusDocument document, IEnumerable<string> fileNames)
		{
			this.Document = document;
			this.FileNames = new HashSet<string>(fileNames);
		}

		public abstract void Reverse();
		public abstract bool CanCancelWith(IOperationCache other);

		/// <summary>
		/// FileNamesプロパティの中身が同一であればtrueを返します．
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool HasSameFileNamesWith(SongsCache other)
		{
			if (this.FileNames.Count == other.FileNames.Count)
			{
				return this.FileNames.Except(other.FileNames).Count() == 0;
			}
			else
			{
				return false;
			}
		}
	}
	#endregion

	// (0.3.0)
	#region SongsAddedCacheクラス
	public class SongsAddedCache : SongsCache
	{
		public SongsAddedCache(MutusDocument document, IEnumerable<string> fileNames)
			: base(document, fileNames)
		{ }

		public override void Reverse()
		{
			Document.RemoveSongs(this.FileNames);
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			return other is SongsRemovedCache
				&& ((SongsCache)other).Document == this.Document
				&& this.HasSameFileNamesWith((SongsCache)other);
		}
	}
	#endregion

	// (0.3.0)
	#region SongsRemovedCacheクラス
	public class SongsRemovedCache : SongsCache
	{
		public SongsRemovedCache(MutusDocument document, IEnumerable<string> fileNames)
			: base(document, fileNames)
		{ }

		public override void Reverse()
		{
			Document.AddSongs(this.FileNames);
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			return other is SongsAddedCache
				&& ((SongsCache)other).Document == this.Document
				&& this.HasSameFileNamesWith((SongsCache)other);
		}
	}
	#endregion

}
