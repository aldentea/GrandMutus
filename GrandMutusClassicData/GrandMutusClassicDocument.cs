﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus
{
	using Data;
	namespace Classic.Data
	{
		// (0.1.0)
		#region GrandMutusClassicDocumentクラス
		public class GrandMutusClassicDocument : MutusDocument
		{

			// (0.1.1)QuestionNoChangeCompletedイベントハンドラの設定を追加．
			// (0.1.0)
			#region *コンストラクタ(GrandMutusClassicDocument)
			public GrandMutusClassicDocument()
				: base()
			{
				this.Initialized += GrandMutusClassicDocument_Initialized;
				this.Opened += GrandMutusClassicDocument_Opened;
				this.Questions.CollectionChanged += Questions_CollectionChanged;
				this.Questions.QuestionNoChangeCompleted += Questions_QuestionNoChangeCompleted;
				// QuestionNoChangedではなくQuestionNoChangeCompletedを使うことで，
				// リストボックス内で正しくソートされなかった問題が解消した．
			}

			// (0.1.1)
			void Questions_QuestionNoChangeCompleted(object sender, ValueChangedEventArgs<int?> e)
			{
				Question question = (Question)sender;
				if (question.Category == this.CurrentCategory)
				{
					NotifyPropertyChanged("CurrentCategoryQuestions");
					NotifyPropertyChanged("CurrentNumberedQuestions");
					if (!e.PreviousValue.HasValue || !e.CurrentValue.HasValue)
					{
						NotifyPropertyChanged("CurrentUnnumberedQuestions");
					}
				}
			}

			// (0.1.0)
			void Questions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
			{
				NotifyPropertyChanged("CurrentCategoryQuestions");
				NotifyPropertyChanged("CurrentUnnumberedQuestions");
				NotifyPropertyChanged("CurrentNumberedQuestions");
			}

			// (0.1.0)
			void GrandMutusClassicDocument_Opened(object sender, EventArgs e)
			{
				CurrentCategory = string.Empty;
			}

			// (0.1.0)
			void GrandMutusClassicDocument_Initialized(object sender, EventArgs e)
			{
				CurrentCategory = string.Empty;
			}
			#endregion

			// (0.1.0)
			#region *CurrentCategoryプロパティ
			/// <summary>
			/// 現在のカテゴリを取得／設定します．
			/// </summary>
			public string CurrentCategory
			{
				get
				{
					return _currentCategory;
				}
				set
				{
					if (string.IsNullOrEmpty(value))
					{
						value = string.Empty;
					}
					if (this.CurrentCategory != value)
					{
						this._currentCategory = value;
						// このときは，OperationCacheをどうにかする必要がありそう？
						// でも，View用のプロパティなんだから，そんなことしなくていいんじゃない？
						// (実質的な変化を及ぼすものではないということ．)
						NotifyPropertyChanged("CurrentCategory");
						NotifyPropertyChanged("CurrentCategoryQuestions");
						NotifyPropertyChanged("CurrentUnnumberedQuestions");
						NotifyPropertyChanged("CurrentNumberedQuestions");
					
					}
				}
			}
			string _currentCategory = string.Empty;
			#endregion

			// (0.1.0)
			#region *CurrentCategoryQuestionsプロパティ
			/// <summary>
			/// CurrentCategoryに属する問題を取得します．
			/// </summary>
			public IEnumerable<Question> CurrentCategoryQuestions
			{
				get
				{
					return this.Questions.Where(q => q.Category == CurrentCategory);
				}
			}
			#endregion

			// (0.1.0)
			#region *CurrentUnnumberedQuestionsプロパティ
			/// <summary>
			/// CurrentCategoryに属しており，Noの設定されていない問題を取得します．
			/// </summary>
			public IEnumerable<Question> CurrentUnnumberedQuestions
			{
				get
				{
					return this.CurrentCategoryQuestions.Where(q => !q.No.HasValue);
				}
			}
			#endregion

			// (0.1.0)
			#region *CurrentNumberedQuestionsプロパティ
			/// <summary>
			/// CurrentCategoryに属しており，Noの設定された問題を，Noの昇順で取得します．
			/// </summary>
			public IEnumerable<Question> CurrentNumberedQuestions
			{
				get
				{
					return this.CurrentCategoryQuestions.Where(q => q.No.HasValue).OrderBy(q => q.No.Value);
				}
			}
			#endregion

		}
		#endregion

		/*
		class CurrentCategoryChangedOperationCache : PropertyChangedCache<string>
		{
			public void Reverse()
			{
				throw new NotImplementedException();
			}

			public bool CanCancelWith(IOperationCache other)
			{
			
			}
		}
		*/
	}
}
