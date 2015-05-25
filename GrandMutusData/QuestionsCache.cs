using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Data
{

	// (0.4.1) SongsCacheと同様の実装に変更．
	// (0.3.4) ReverseメソッドとCanCancelWithメソッドがabstractです．
	#region [abstract]QuestionsCacheクラス
	public abstract class QuestionsCache : IOperationCache
	{
		public MutusDocument Document { get; protected set; }
		public ISet<Question> Questions
		{ get; protected set; }

		protected QuestionsCache(MutusDocument document, IEnumerable<Question> questions)
		{
			this.Document = document;
			this.Questions = new HashSet<Question>(questions);
		}

		public abstract void Reverse();
		public abstract bool CanCancelWith(IOperationCache other);

		/// <summary>
		/// Questionsプロパティの中身が同一であればtrueを返します．
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool HasSameQuestionsWith(QuestionsCache other)
		{
			if (this.Questions.Count == other.Questions.Count)
			{
				return this.Questions.Except(other.Questions).Count() == 0;
			}
			else
			{
				return false;
			}
		}
	}
	#endregion


	// (0.3.4)
	#region QuestionsAddedCacheクラス
	public class QuestionsAddedCache : QuestionsCache
	{
		public QuestionsAddedCache(MutusDocument document, IEnumerable<Question> questions)
			: base(document, questions)
		{ }

		public override void Reverse()
		{
			Document.RemoveQuestions(this.Questions);
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			//return false;
			return other is QuestionsRemovedCache
				&& ((QuestionsCache)other).Document == this.Document
				&& this.HasSameQuestionsWith((QuestionsCache)other);
		}
	}
	#endregion


	// (0.4.1)
	#region QuestionsRemovedCacheクラス
	public class QuestionsRemovedCache : QuestionsCache
	{
		public QuestionsRemovedCache(MutusDocument document, IEnumerable<Question> questions)
			: base(document, questions)
		{ }


		public override void Reverse()
		{
			Document.AddQuestions(this.Questions);
		}

		public override bool CanCancelWith(IOperationCache other)
		{
			return other is QuestionsAddedCache
				&& ((QuestionsCache)other).Document == this.Document
				&& this.HasSameQuestionsWith((QuestionsCache)other);
		}
	}
	#endregion

}
