using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace GrandMutus.Data
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


		// コンストラクタはとりあえずデフォルトのものを使用する．


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
			if (this.ID > 0)
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
			int id = (int)orderElement.Attribute(ID_ATTRIBUTE);
			int? question_id = (int?)orderElement.Attribute(QUESTION_ID_ATTRIBUTE);
			var order = new Order { ID = id, QuestionID = question_id };

			foreach (var log in orderElement.Elements(Log.ELEMENT_NAME))
			{
				order.Items.Add(Log.Generate(log));
			}
			return order;
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
