using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Linq;

namespace GrandMutus.Net6.Data
{

	#region Logクラス
	public class Log : INotifyPropertyChanged
	{
		// ログに関する情報を保持します．
		// 問題に関する情報は，Logの親要素が持つことにします．

		#region *IDプロパティ
		public int ID
		{
			get { return _id; }
			internal set { _id = value; }
		}
		int _id = -1; // (0.1.2)-1は未設定であることを示す．
		#endregion

		// (0.9.1)とりあえず従来通りの実装をする。
		#region *PlayerIDプロパティ
		public int? PlayerID
		{
			get
			{
				return _playerID;
			}
			set
			{
				if (_playerID != value)
				{
					_playerID = value;
					NotifyPropertyChanged("PlayerID");
				}
			}
		}
		int? _playerID;
		#endregion

		#region *Codeプロパティ
		public string Code
		{
			get
			{
				return _code;
			}
			set
			{
				if (_code != value)
				{
					_code = value;
					NotifyPropertyChanged("Code");
				}
			}
		}
		string _code = string.Empty;
		#endregion

		#region *Valueプロパティ
		public decimal Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (_value != value)
				{
					_value = value;
					NotifyPropertyChanged("Value");
				}
			}
		}
		decimal _value = 0;
		#endregion

		// 「コメント」はどうする？

		// (0.9.1)PlayerID関連を実装。
		#region XML入出力関連

		// どこに置くかは未定．
		// ここに置くか，XML生成専用のクラスを作成するか...


		// このあたり本来はXNameなんだけど，手抜きをする．
		public const string ELEMENT_NAME = "log";
		const string ID_ATTRIBUTE = "id";
		const string PLAYER_ID_ATTRIBUTE = "player_id";
		const string CODE_ATTRIBUTE = "code";
		const string VALUE_ATTRIBUTE = "value";

		// (0.9.1)PlayerID関連を実装。
		#region *XML要素を生成(GenerateElement)
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public XElement GenerateElement()
		{
			var element = new XElement(ELEMENT_NAME);
			element.Add(new XAttribute(ID_ATTRIBUTE, this.ID));
			if (PlayerID.HasValue)
			{
				element.Add(new XAttribute(PLAYER_ID_ATTRIBUTE, this.PlayerID.Value));
			}
			element.Add(new XAttribute(CODE_ATTRIBUTE, this.Code));
			element.Add(new XAttribute(VALUE_ATTRIBUTE, this.Value));
			return element;
		}
		#endregion

		// (0.9.1)PlayerID関連を実装。
		#region *[static]XML要素からオブジェクトを生成(Generate)
		public static Log Generate(XElement logElement)
		{
			Log log = new Log();

			// XMLからインスタンスを生成するならばIDは常にあるのでは？
			// songのようにインポートの問題も考えなくていいし...

			var id_attribute = logElement.Attribute(ID_ATTRIBUTE);
			if (id_attribute != null)
			{
				log.ID = (int)id_attribute;
			}
			var player_id_attribute = logElement.Attribute(PLAYER_ID_ATTRIBUTE);
			if (player_id_attribute != null)
			{
				log.PlayerID = (int)player_id_attribute;
			}

			log.Code = (string?)logElement.Attribute(CODE_ATTRIBUTE) ?? String.Empty;
			log.Value = (decimal?)logElement.Attribute(VALUE_ATTRIBUTE) ?? 0;

			return log;
		}
		#endregion

		#endregion

		// Songからのコピペ．
		#region INotifyPropertyChanged実装

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		protected void NotifyPropertyChanged(string propertyName)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

	}
	#endregion

}
