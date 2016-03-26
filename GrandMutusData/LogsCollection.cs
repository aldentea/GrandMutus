using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;


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
			//this.CollectionChanged += LogsCollection_CollectionChanged;

		}
		#endregion


		public void Initialize()
		{
			// ClearItems()やItems.Clear()とはどう違うのかな？
			this.Clear();
		}


		// ここに書く？

		#region ログ追加関連

		#region *CurrentOrderプロパティ
		/// <summary>
		/// 現在の問題に対応するOrder(もしくはnull)を返します．
		/// </summary>
		private Order CurrentOrder
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
				this.Items.Add(new Order { ID = 0 });
				return true;
			}
			else
			{
				return false;
			}
		}

		public void AddOrder(int question_id)
		{
			this.Items.Add(new Order { ID = GenerateNewOrderID(), QuestionID = question_id });
		}

		/// <summary>
		/// 現在の出題についてログを追加します．
		/// </summary>
		/// <param name="code"></param>
		/// <param name="value"></param>
		public void AddLog(string code, decimal value)
		{
			var log = new Log { ID = GenerateNewLogID(), Code = code, Value = value };
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
