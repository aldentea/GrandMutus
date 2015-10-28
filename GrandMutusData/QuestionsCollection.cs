using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Linq;
using System.ComponentModel;

namespace GrandMutus.Data
{
	// (0.3.3)
	public class QuestionsCollection : ObservableCollection<Question>
	{
		#region *コンストラクタ(QuestionsCollection)
		QuestionsCollection()
		{
			this.CollectionChanged += QuestionsCollection_CollectionChanged;
		}

		public QuestionsCollection(MutusDocument document) : this()
		{
			this._document = document;
		}
		#endregion

		// (0.6.1)
		#region *Categoriesプロパティ

		/// <summary>
		/// 使われているカテゴリの一覧を取得します．
		/// </summary>
		public IEnumerable<string> Categories
		{
			get
			{
				return Items.Select(q => q.Category).Distinct();
			}
		}

		#endregion

		#region Documentとの関係

		public MutusDocument Document { get { return _document; } }
		readonly MutusDocument _document;

		#endregion


		#region コレクション変更関連

		// (0.4.1)
		/// <summary>
		/// 問題が削除された時に発生します．
		/// </summary>
		public event EventHandler<ItemEventArgs<IEnumerable<Question>>> QuestionsRemoved = delegate { };

		// (0.6.1) Categoriesプロパティ変更通知のイベントを発生．
		// (0.4.5) NoChangedイベントハンドラの着脱を追加．
		// (0.4.1) Remove時の処理を追加(ほとんどSongsCollectionのコピペ)．
		private void QuestionsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(new PropertyChangedEventArgs("Categories"));

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (var item in e.NewItems)
					{
						var question = (Question)item;

						// IDを付与する．
						// (0.1.2)IDが既に設定されているかどうかを確認．
						if (question.ID <= 0)	// 無効な値．
						{
							question.ID = GenerateNewID();
						}
						// ☆songのプロパティ変更をここで受け取る？MutusDocumentで行えばここでは不要？
						//question.PropertyChanging += Question_PropertyChanging;
						//question.PropertyChanged += Question_PropertyChanged;
						question.NoChanged += Question_NoChanged;

						question.OnAddedTo(this);
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					IList<Question> questions = new List<Question>();
					foreach (var question in e.OldItems.Cast<Question>())
					{
						// Questionでは、変更通知機能がまだ(ちょっと↑で)実装されていない．
						// 削除にあたって、変更通知機能を抑止。
						//question.PropertyChanging -= Song_PropertyChanging;
						//question.PropertyChanged -= Song_PropertyChanged;
						question.NoChanged -= Question_NoChanged;

						questions.Add(question);
					}
					// (MutusDocumentを経由せずに)UIから削除される場合もあるので，
					// ここでOperationCacheの処理を行うことにした．
					if (questions.Count > 0)
					{
						this.QuestionsRemoved(this, new ItemEventArgs<IEnumerable<Question>> { Item = questions });
					}
					break;
			}
		}

		#endregion

		// (0.6.1)Category変更時に，Categoriesプロパティ変更通知のイベントを発生．
		// (0.4.5)整番処理に着手。
		// (0.3.3)未使用。
		#region アイテム変更関連

		private void Question_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void Question_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var question = (Question)sender;
			switch(e.PropertyName)
			{
				case "No":
					// 整番処理は複雑なのでここでは行わない(Question_NoChangedで行う)。
					break;
				case "Category":
					OnPropertyChanged(new PropertyChangedEventArgs("Categories"));
					break;
			}
		}

		// (0.4.6.0)QuestionNoChangeCompletedイベントの発生を追加．
		// (0.4.5.2)で、カテゴリを考慮。
		// (0.4.5.1)まずはカテゴリを考慮しない形で整番処理を記述．
		#region 整番処理関連

		bool _noChangingFlag = false;

		void Question_NoChanged(object sender, ValueChangedEventArgs<int?> e)
		{
			if (_noChangingFlag)
			{
				return;
			}
			_noChangingFlag = true;

			try
			{
				var self = (Question)sender;
				int? old_no = e.PreviousValue;
				int? new_no = e.CurrentValue;
				if (new_no.HasValue)
				{
					int n = Items.Count(q => { return q.Category == self.Category && q.No.HasValue; });
					if (new_no.Value > n)
					{
						self.No = new_no = n;
					}
				}

				// このあたりで操作履歴に加えておく。
				// (ここまでの処理で変更になる可能性があるので、Questionから直接MutusDocumentに通知することはしない。)
				this.QuestionNoChanged(self, new ValueChangedEventArgs<int?>(old_no, new_no));

				if (old_no.HasValue)
				{
					if (new_no.HasValue)
					{
						// M -> N
						int m = old_no.Value;
						int n = new_no.Value;

						if (m < n)
						{
							// M < N
							// (M+1)からNを1つずつ減らす。
							foreach (var question in Items.Where(q => { return q.Category == self.Category && q.No.HasValue && q.No > m && q.No <= n && q != self; }))
							{
								question.No -= 1;
							}
						}
						else
						{
							// M > N
							// Nから(M-1)を1つずつ増やす。
							foreach (var question in Items.Where(q => { return q.Category == self.Category && q.No.HasValue && q.No < m && q.No >= n && q != self; }))
							{
								question.No += 1;
							}
						}
					}
					else
					{
						// N -> null
						// Nより大きい番号を1ずつ減らす．
						int n = old_no.Value;

						foreach (var question in Items.Where(q => { return q.Category == self.Category && q.No.HasValue && q.No > n; }))
						{
							question.No -= 1;
						}
					}
				}
				else
				{
					// new_no should has value.
					int n = new_no.Value;

					// null -> N
					// N以上の番号を1つずつ増やす。
					foreach (var question in Items.Where(q => { return q.Category == self.Category && q.No.HasValue && q.No >= n && q != self; }))
					{
						question.No += 1;
					}
					
				}

				this.QuestionNoChangeCompleted(self, new ValueChangedEventArgs<int?>(old_no, new_no));

			}
			finally
			{
				_noChangingFlag = false;
			}
		}

		// (0.4.5.1)
		/// <summary>
		/// 問題の番号が変更になったときに発生します。(操作履歴管理用かな？)
		/// senderはQuestionsCollectionではなくQuestionであることに一応注意。
		/// </summary>
		public event EventHandler<ValueChangedEventArgs<int?>> QuestionNoChanged = delegate{};

		// 新しいNoの決定→変更→QuestionNoChanged→他の問題のNoの処理→QuestionNoChangeCompletedの順．

		// (0.4.6.0)
		/// <summary>
		/// 問題番号の変更処理が完了したときに発生します（他の問題の番号スライド処理の完了後）。
		/// senderはQuestionsCollectionではなくQuestionであることに一応注意。
		/// </summary>
		public event EventHandler<ValueChangedEventArgs<int?>> QuestionNoChangeCompleted = delegate { };

		#endregion

		#endregion




		#region XML入出力関連

		public const string ELEMENT_NAME = "questions";
		const string PATH_ATTRIBUTE = "path";

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public XElement GenerateElement()
		{
			XElement element = new XElement(ELEMENT_NAME);

			foreach (IntroQuestion question in this.Items)
			{
				element.Add(question.GenerateElement());
			}

			return element;
		}


		// (0.3.3)とりあえずIntroQuestionのみ。
		public void LoadElement(XElement questionsElement)
		{
			foreach (var question_element in questionsElement.Elements())
			{
				// ☆ここの処理は動的に分岐を生成するようにしたい！
				switch (question_element.Name.LocalName)
				{
					case "intro":
						this.Add(IntroQuestion.Generate(question_element));
						break;
					default:
						// ※知らない要素が出てきたらどうしますか？
						break;
				}
			}
		}

		#endregion



		// (0.3.3)SongsCollectionからのコピペ。共通実装にしますか？
		#region ID管理関連

		int GenerateNewID()
		{
			int new_id = this.UsedIDList.Any(n => n > 0) ? this.UsedIDList.Max() + 1 : 1;
			// ↑Max()は，要素が空ならInvalidOperationExceptionをスローする．

			//UsedIDList.Add(new_id);
			return new_id;
		}

		IEnumerable<int> UsedIDList
		{
			get
			{
				return Items.Select(question => question.ID);
			}
		}

		#endregion


	}

}
