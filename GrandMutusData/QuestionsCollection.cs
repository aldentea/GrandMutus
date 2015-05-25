using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Linq;

namespace GrandMutus.Data
{
	// (0.3.3)
	public class QuestionsCollection : ObservableCollection<IntroQuestion>
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



		#region Documentとの関係

		public MutusDocument Document { get { return _document; } }
		readonly MutusDocument _document;

		#endregion


		#region コレクション変更関連

		// (0.4.1)
		/// <summary>
		/// 問題が削除された時に発生します．
		/// </summary>
		public event EventHandler<ItemEventArgs<IEnumerable<IntroQuestion>>> QuestionsRemoved = delegate { };


		// (0.4.1) Remove時の処理を追加(ほとんどSongsCollectionのコピペ)．
		private void QuestionsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (var item in e.NewItems)
					{
						var question = (IntroQuestion)item;

						// IDを付与する．
						// (0.1.2)IDが既に設定されているかどうかを確認．
						if (question.ID <= 0)	// 無効な値．
						{
							question.ID = GenerateNewID();
						}
						// ☆songのプロパティ変更をここで受け取る？MutusDocumentで行えばここでは不要？
						//question.PropertyChanging += Question_PropertyChanging;
						//question.PropertyChanged += Question_PropertyChanged;
						question.OnAddedTo(this);
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					IList<IntroQuestion> questions = new List<IntroQuestion>();
					foreach (var question in e.OldItems.Cast<IntroQuestion>())
					{
						// Questionでは、変更通知機能がまだ(ちょっと↑で)実装されていない．
						// 削除にあたって、変更通知機能を抑止。
						//question.PropertyChanging -= Song_PropertyChanging;
						//question.PropertyChanged -= Song_PropertyChanged;

						questions.Add(question);
					}
					// (MutusDocumentを経由せずに)UIから削除される場合もあるので，
					// ここでOperationCacheの処理を行うことにした．
					if (questions.Count > 0)
					{
						this.QuestionsRemoved(this, new ItemEventArgs<IEnumerable<IntroQuestion>> { Item = questions });
					}
					break;
			}
		}
		#endregion

		// (0.3.3)未使用。
		#region アイテム変更関連

		private void Question_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void Question_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			throw new NotImplementedException();
		}

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
