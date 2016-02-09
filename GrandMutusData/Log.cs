using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Linq;

namespace GrandMutus.Data
{

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

		// Player関連はおいておく．

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


		#region XML入出力関連

		// どこに置くかは未定．
		// ここに置くか，XML生成専用のクラスを作成するか...


		// このあたり本来はXNameなんだけど，手抜きをする．
		public const string ELEMENT_NAME = "log";
		const string ID_ATTRIBUTE = "id";
		const string CODE_ATTRIBUTE = "code";
		const string VALUE_ATTRIBUTE = "value";

		// (0.3.2)sabi_pos要素を出力．
		#region *XML要素を生成(GenerateElement)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="songs_root">曲ファイルを格納しているディレクトリのフルパスです．</param>
		/// <returns></returns>
		public XElement GenerateElement(string songs_root = null)
		{
			var element = new XElement(ELEMENT_NAME);
			element.Add(new XAttribute(ID_ATTRIBUTE, this.ID));
			return element;
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


}
