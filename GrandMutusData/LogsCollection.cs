using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace GrandMutus.Data
{


	#region LogsCollectionクラス
	public class LogsCollection : ObservableCollection<Order>
	{

		// (0.8.0.2)
		public bool IsEmpty
		{
			get
			{
				return Items.Count == 0;
			}
		}

		#region *コンストラクタ(LogsCollection)
		public LogsCollection()
		{
			this.CollectionChanged += LogsCollection_CollectionChanged;

		}
		#endregion


		public void Initialize()
		{
			// ClearItems()やItems.Clear()とはどう違うのかな？
			this.Clear();
		}

		// (0.9.2)
		/// <summary>
		/// すべてのログを取得します。
		/// </summary>
		public IEnumerable<Log> AllLog
		{
			get
			{
				return Items.SelectMany(order => order);
			}
		}

		// (0.4.5) NoChangedイベントハンドラの着脱を追加．
		// (0.4.1) Remove時の処理を追加(ほとんどSongsCollectionのコピペ)．
		private void LogsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (var item in e.NewItems)
					{
						var order = (Order)item;

						// IDを付与する．
						// (0.1.2)IDが既に設定されているかどうかを確認．
						if (order.ID < 0) // 無効な値．
						{
							order.ID = GenerateNewOrderID();
						}
						// ☆songのプロパティ変更をここで受け取る？MutusDocumentで行えばここでは不要？
						//question.PropertyChanging += Question_PropertyChanging;
						//question.PropertyChanged += Question_PropertyChanged;
						//order.NoChanged += Question_NoChanged;

						// これいる？
						order.OnAddedTo(this);
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					//IList<Order> logs = new List<Order>();
					foreach (var order in e.OldItems.Cast<Order>())
					{

						// ※↓これいるの？
						// 削除にあたって、変更通知機能を抑止。
						//order.PropertyChanging -= Song_PropertyChanging;
						//order.PropertyChanged -= Song_PropertyChanged;

						//logs.Add(order);

						order.OnAddedTo(null);
						order.LogAdded += Order_LogAdded;
						order.LogRemoved += Order_LogRemoved;
					}

					// QuestionのようにUIから直接削除される場合は想定していないので、
					// この処理は不要だと思う(SongsCollectionを参照)。よって、logsも不要。
					//if (logs.Count > 0)
					//{
					//	this.LogsRemoved(this, new ItemEventArgs<IEnumerable<Order>> { Item = logs });
					//}
					break;
			}
		}

		// (0.9.5)
		private void Order_LogAdded(object sender, LogEventArgs e)
		{
			this.LogAdded(sender, e);
		}


		// (0.9.5)
		private void Order_LogRemoved(object sender, LogEventArgs e)
		{
			this.LogRemoved(sender, e);
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


		// ここに書く？

		#region ログ追加関連

		// (0.9.4)public化。
		#region *CurrentOrderプロパティ
		/// <summary>
		/// 現在の問題に対応するOrder(もしくはnull)を返します．
		/// </summary>
		public Order CurrentOrder
		{
			// とりあえずidが最大のorderを返します．
			// orderが1つもなければ，nullを返します．
			get
			{
				return this.Items.OrderByDescending(order => order.ID).FirstOrDefault();
			}
		}
		#endregion

		/// <summary>
		/// 最初のOrderを追加します．
		/// 追加する必要があるかどうかの判定も行い，不要であれば追加しません．
		/// </summary>
		/// <returns></returns>
		public bool AddFirstOrder()
		{
			if (Items.Count == 0)
			{
				this.Add(new Order { ID = 0 });
				return true;
			}
			else
			{
				return false;
			}
		}

		public void AddOrder(int question_id)
		{
			this.Add(new Order { ID = GenerateNewOrderID(), QuestionID = question_id });
		}

		// (0.8.1.3)QuestionIDを返すように修正。
		// (0.8.1)
		/// <summary>
		/// AddOrderのアンドゥ操作としてのみ使われることが想定されています。QuestionIDが返ります。
		/// </summary>
		public int? RemoveOrder()
		{
			int n = this.Items.Count - 1;
			int? q_id = this.Items[n].QuestionID;
			this.RemoveAt(n);
			return q_id;
		}

		/// <summary>
		/// 現在の出題についてログを追加します．
		/// </summary>
		/// <param name="code"></param>
		/// <param name="value"></param>
		[Obsolete("1.0で廃止となります。3引数のバージョンを使用してください。")]
		public void AddLog(string code, decimal value)
		{
			AddLog(null, code, value);
		}

		// (0.9.1)
		/// <summary>
		/// 現在の出題についてログを追加します．
		/// </summary>
		/// <param name="code"></param>
		/// <param name="value"></param>
		public void AddLog(int? playerID, string code, decimal value)
		{
			var log = new Log { ID = GenerateNewLogID(), PlayerID = playerID, Code = code, Value = value };
			CurrentOrder.Add(log);
		}

		#endregion



		#region XML入出力関連

		public const string ELEMENT_NAME = "logs";

		#region *XML要素を生成(GenerateElement)
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public XElement GenerateElement()
		{
			XElement element = new XElement(ELEMENT_NAME);

			foreach (Order order in this.Items)
			{
				element.Add(order.GenerateElement());
			}

			return element;
		}
		#endregion

		#region XML要素から読み込み(LoadElement)
		public void LoadElement(XElement logsElement)
		{
			foreach (var order_element in logsElement.Elements(Order.ELEMENT_NAME))
			{
				this.Add(Order.Generate(order_element));
			}
		}
		#endregion

		#endregion


		#region ID管理関連

		// OrderのIDとLogのIDをここで管理する？

		// (0.2.1)無効なIDとして負の値を使うことにしたので，Anyに条件を付加．
		int GenerateNewOrderID()
		{
			int new_id = this.UsedOrderIDList.Any(n => n > 0) ? this.UsedOrderIDList.Max() + 1 : 1;
			// ↑Max()は，要素が空ならInvalidOperationExceptionをスローする．

			return new_id;
		}

		/// <summary>
		/// 使用されているOrderIDのリストを返します．
		/// (0からの連番であることが期待されるのですが...)
		/// </summary>
		IEnumerable<int> UsedOrderIDList
		{
			get
			{
				return Items.Select(order => order.ID);
			}
		}

		int GenerateNewLogID()
		{
			int new_id = this.UsedLogIDList.Any(n => n > 0) ? this.UsedLogIDList.Max() + 1 : 1;
			// ↑Max()は，要素が空ならInvalidOperationExceptionをスローする．

			return new_id;
		}

		/// <summary>
		/// 使用されているLogIDのリストを返します．
		/// (0からの連番であることが期待されるのですが...)
		/// </summary>
		IEnumerable<int> UsedLogIDList
		{
			get
			{
				return Items.SelectMany(order => order.UsedIDList);
			}
		}

		#endregion

	}
	#endregion


}
