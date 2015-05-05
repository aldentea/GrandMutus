using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Data
{

	// (0.3.4) ReverseメソッドとCanCancelWithメソッドがabstractです．
	#region [abstract]QuestionsCacheクラス
	public abstract class QuestionsCache : IOperationCache
	{
		public MutusDocument Document { get; protected set; }
		public ISet<int> IDSet
		{ get; protected set; }

		protected QuestionsCache(MutusDocument document, IEnumerable<int> songIDs)
		{
			this.Document = document;
			this.IDSet = new HashSet<int>(songIDs);
		}

		public abstract void Reverse();
		public abstract bool CanCancelWith(IOperationCache other);

		/// <summary>
		/// FileNamesプロパティの中身が同一であればtrueを返します．
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool HasSameIDSetWith(QuestionsCache other)
		{
			if (this.IDSet.Count == other.IDSet.Count)
			{
				return this.IDSet.Except(other.IDSet).Count() == 0;
			}
			else
			{
				return false;
			}
		}
	}
	#endregion


	// (0.3.4)
	#region SongsAddedCacheクラス
	public class QuestionsAddedCache : QuestionsCache
	{
		public QuestionsAddedCache(MutusDocument document, IEnumerable<int> questionIDs)
			: base(document, questionIDs)
		{ }

		public override void Reverse()
		{
			//Document.RemoveQuestions(this.IDSet);
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			return false;
			//return other is SongsRemovedCache
			//	&& ((SongsCache)other).Document == this.Document
			//	&& this.HasSameFileNamesWith((SongsCache)other);
		}
	}
	#endregion

}
