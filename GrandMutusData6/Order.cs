using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace GrandMutus.Net6.Data
{
	#region *Orderクラス
	public class Order : ObservableCollection<Log>
	{
		// 出題順を表します．
		// logs要素に含まれ，log要素を含みます．

		// idは0から始まります．

		// id=0の要素は，Questionに対応しません．
		// idが1以上の要素は，1つのQuestionに対応します．


		#region *IDプロパティ
		public int ID
		{
			get { return _id; }
			internal set { _id = value; }
		}
		int _id = -1; // (0.1.2)-1は未設定であることを示す．
		#endregion

		#region *QuestionIDプロパティ
		public int? QuestionID
		{
			// このインスタンスがいつ生成されるか...
			// インスタンスを生成してから，QuestionIDが変わるケースなんて考えられるのかな？
			// →とりあえずそれも考慮しておくことにしましょう．
			get { return _question_id; }
			set { _question_id = value; }
		}
		int? _question_id;
		#endregion


		// (0.9.5) 未使用。
		public LogsCollection? Parent { get; protected set; }

		// (0.9.5)
		public void OnAddedTo(LogsCollection? parent)
		{
			this.Parent = parent;
		}

		// (0.9.5)
		public Order()
		{
			this.CollectionChanged += Order_CollectionChanged;
		}

		// (0.9.5)
		private void Order_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					if (e.NewItems == null)
					{
						throw new Exception("e.NewItemsがnullです（困りましたね）。");
					}
					foreach (var log in e.NewItems.Cast<Log>())
					{
						this.LogAdded(this, new LogEventArgs(log.Code, log.Value, log.PlayerID, this.QuestionID));
					}

					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					if (e.OldItems == null)
					{
						throw new Exception("e.OldItemsがnullです（困りましたね）。");
					}
					foreach (var log in e.OldItems.Cast<Log>())
					{
						this.LogRemoved(this, new LogEventArgs(log.Code, log.Value, log.PlayerID, this.QuestionID));
					}
					break;
			}
		}

		// (0.9.5)
		/// <summary>
		/// ログが追加されたときに発生します。
		/// </summary>
		public event EventHandler<LogEventArgs> LogAdded = delegate { };


		// (0.9.5)
		/// <summary>
		/// ログが削除されたときに発生します。
		/// </summary>
		public event EventHandler<LogEventArgs> LogRemoved = delegate { };

		#region XML入出力関連

		public const string ELEMENT_NAME = "order";
		const string ID_ATTRIBUTE = "id";
		const string QUESTION_ID_ATTRIBUTE = "question_id";

		#region *XML要素を生成する(GenerateElement)
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public XElement GenerateElement()
		{
			XElement element = new XElement(ELEMENT_NAME);
			element.Add(new XAttribute(ID_ATTRIBUTE, this.ID));
			if (this.ID > 0 && this.QuestionID > 0)
			{
				element.Add(new XAttribute(QUESTION_ID_ATTRIBUTE, this.QuestionID));
			}

			foreach (Log log in this.Items)
			{
				element.Add(log.GenerateElement());
			}

			return element;
		}
		#endregion

		#region *[static]XML要素からオブジェクトを生成(Generate)
		/// <summary>
		/// XML要素からインスタンスを生成します．
		/// </summary>
		/// <param name="orderElement"></param>
		public static Order Generate(XElement orderElement)
		{

			int? id = (int?)orderElement.Attribute(ID_ATTRIBUTE);
			if (id.HasValue)
			{
				int? question_id = (int?)orderElement.Attribute(QUESTION_ID_ATTRIBUTE);
				var order = new Order { ID = id.Value, QuestionID = question_id };

				foreach (var log in orderElement.Elements(Log.ELEMENT_NAME))
				{
					order.Items.Add(Log.Generate(log));
				}
				return order;
			}
			else
			{
				throw new Exception("order要素にid属性がないか、id属性が不正な値を持っています（自然数のみが認められます）。");
			}
		}
		#endregion


		#endregion

		internal IEnumerable<int> UsedIDList
		{
			get
			{
				return Items.Select(log => log.ID);
			}
		}

	}
		#endregion

	}
