using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using System.IO;

using System.Threading.Tasks;

namespace GrandMutus.Net6.Data
{
	public class Player : INotifyPropertyChanged
	{

		// インスタンスはMutusDocumentでのみ作成するようにする．
		// そこでIDを付与する．
		// 付与するときも，internalなsetterを用いる．

		#region *IDプロパティ
		public int ID
		{
			get { return _id; }
			internal set { _id = value; }
		}
		int _id = -1; // (0.1.2)-1は未設定であることを示す．
		#endregion

		#region *Nameプロパティ
		/// <summary>
		/// プレイヤーの名前を取得／設定します．
		/// </summary>
		public string Name
		{
			get
			{
				return _name ?? string.Empty;
			}
			set
			{
				string new_value = string.IsNullOrEmpty(value) ? string.Empty : value;
				if (Name != new_value)
				{
					NotifyPropertyChanging("Name");
					this._name = new_value;
					NotifyPropertyChanged("Name");
				}
			}
		}
		string _name = string.Empty;
		#endregion




		#region *Maruプロパティ
		public decimal Maru
		{
			get
			{ return _maru; }
			set
			{
				if (Maru!= value)
				{
					NotifyPropertyChanging("Maru");
					this._maru = value;
					NotifyPropertyChanged("Maru");
				}
			}
		}
		decimal _maru = 0;
		#endregion

		#region *Batsuプロパティ
		public decimal Batsu
		{
			get
			{ return _batsu; }
			set
			{
				if (Batsu != value)
				{
					NotifyPropertyChanging("Batsu");
					this._batsu = value;
					NotifyPropertyChanged("Batsu");
				}
			}
		}
		decimal _batsu = 0;
		#endregion

		#region *Scoreプロパティ
		public decimal Score
		{
			get
			{
				return _score;
			}
			set
			{
				if (Score != value)
				{
					NotifyPropertyChanging("Score");
					this._score = value;
					NotifyPropertyChanged("Score");
				}
			}
		}
		decimal _score = 0;
		#endregion



		#region *Parentプロパティ
		PlayersCollection Parent
		{
			get { return _parent; }
			set
			{
				if (this.Parent != value)
				{
					this._parent = value;
					NotifyPropertyChanged("Parent");
				}
			}
		}
		PlayersCollection _parent = null;
		#endregion

		/// <summary>
		/// 自身がparentに追加された時に呼び出します．
		/// 除外されたときは引数をnullにして呼び出せばいいのかな？
		/// </summary>
		/// <param name="parent"></param>
		//public virtual void OnAddedTo(PlayersCollection parent)
		//{
		//	this.Parent = parent;
		//}

		// で，PlayersCollectionのCollectionChangedから呼び出す，と．




		#region XML入出力関連

		// どこに置くかは未定．
		// ここに置くか，XML生成専用のクラスを作成するか...

		// <player id="2" maru="3" batsu="4" score="-5">
		//   <name>あるでん茶</name>
		// </player>


		// このあたり本来はXNameなんだけど，手抜きをする．
		public const string ELEMENT_NAME = "player";  // (0.1.2)publicに変更．
		const string ID_ATTRIBUTE = "id";
		const string NAME_ELEMENT = "name";
		const string MARU_ATTRIBUTE = "maru";
		const string BATSU_ATTRIBUTE = "batsu";
		const string SCORE_ATTRIBUTE = "score";

		#region *XML要素を生成(GenerateElement)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="songs_root">曲ファイルを格納しているディレクトリのフルパスです．</param>
		/// <returns></returns>
		public XElement GenerateElement()
		{
			var element = new XElement(ELEMENT_NAME,
				new XAttribute(ID_ATTRIBUTE, this.ID),
				new XElement(NAME_ELEMENT, this.Name),
				new XAttribute(MARU_ATTRIBUTE, this.Maru),
				new XAttribute(BATSU_ATTRIBUTE, this.Batsu),
				new XAttribute(SCORE_ATTRIBUTE, this.Score)
			);
			return element;
		}
		#endregion

		#region *[static]XML要素からオブジェクトを生成(Generate)
		public static Player Generate(XElement playerElement)
		{
			var player = new Player();

			// XMLからインスタンスを生成するならばIDは常にあるのでは？
			// →songをインポートする時とかはそうではないかもしれない？ので一応有無をチェックする．
			// →でも，インポートする時はXML経由ではなくオブジェクト経由で行った方がいいのでは？(ファイル名のパスの扱いとか...)
			var id_attribute = playerElement.Attribute(ID_ATTRIBUTE);
			if (id_attribute != null)
			{
				player.ID = (int)id_attribute;
			}
			player.Name = (string?)playerElement.Element(NAME_ELEMENT) ?? String.Empty;
			var maru = (decimal?)playerElement.Attribute(MARU_ATTRIBUTE);
			if (maru.HasValue)
			{ player.Maru = maru.Value; }
			var batsu = (decimal?)playerElement.Attribute(BATSU_ATTRIBUTE);
			if (batsu.HasValue)
			{ player.Batsu = batsu.Value; }
			var score = (decimal?)playerElement.Attribute(SCORE_ATTRIBUTE);
			if (score.HasValue)
			{ player.Score = score.Value; }

			return player;
		}
		#endregion


		#endregion




		#region INotifyPropertyChanged実装

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		protected void NotifyPropertyChanged(string propertyName)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region INotifyPropertyChanging実装
		public event PropertyChangingEventHandler PropertyChanging = delegate { };
		protected void NotifyPropertyChanging(string propertyName)
		{
			this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
		}
		#endregion

	}
}
