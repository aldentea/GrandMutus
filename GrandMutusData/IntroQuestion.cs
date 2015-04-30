using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Xml.Linq;

namespace GrandMutus.Data
{

	// (0.3.3)
	public class IntroQuestion : INotifyPropertyChanged, INotifyPropertyChanging
	{

		// Songと同様に、インスタンスはDocumentの層で生成する。

		#region *IDプロパティ
		public int ID
		{
			get { return _id; }
			internal set { _id = value; }
		}
		int _id = -1;	// (0.1.2)-1は未設定であることを示す．
		#endregion

		#region *Noプロパティ
		/// <summary>
		/// プリセットされた出題順を取得／設定します。
		/// Categoryごとに1からの連番で与えられます。
		/// </summary>
		public int? No
		{
			get { return _no; }
			set
			{
				if (this.No != value)
				{
					if (value.HasValue && value.Value <= 0)
					{
						throw new ArgumentOutOfRangeException("Noは正の値かnullでなければなりません。");
					}
					else
					{
						NotifyPropertyChanging("No");
						this._no = value;
						NotifyPropertyChanged("No");
					}
				}
			}
		}
		int? _no = null;
		#endregion

		#region *Categoryプロパティ
		public string Category
		{
			get
			{
				return _category;
			}
			set
			{
				if (this.Category != value)
				{
					NotifyPropertyChanging("Category");
					this._category = value;
					NotifyPropertyChanged("Category");
				}
			}
		}
		string _category = string.Empty;
		#endregion

		// (0.3.3)
		#region *SongIDプロパティ
		public int SongID
		{
			get
			{
				return _song_id;
			}
		}
		readonly int _song_id;
		#endregion

		// (0.3.3)
		public Song Song
		{
			get
			{
				return Parent.Document.GetSong(this.SongID);
			}
		}

		public TimeSpan PlayPos
		{
			get
			{
				return _playPos;
			}
			set
			{
				if (this.PlayPos != value)
				{
					// 負の値でないことをここでチェックした方がいいかな？

					NotifyPropertyChanging("PlayPos");
					this._playPos = value;
					NotifyPropertyChanged("PlayPos");
				}
			}
		}
		TimeSpan _playPos = TimeSpan.Zero;

		// (0.3.3)
		#region *コンストラクタ(IntroQuestion)
		internal IntroQuestion(int songID)
		{
			this._song_id = songID;
		}
		#endregion


		#region Parentとの関係

		#region *Parentプロパティ
		QuestionsCollection Parent
		{
			get { return _parent; }
			set
			{
				if (this.Parent != value)
				{
					this._parent = value;
					NotifyPropertyChanged("Parent");
					//NotifyPropertyChanged("RelativeFileName");
				}
			}
		}
		QuestionsCollection _parent = null;
		#endregion

		// こういうプロパティを用意して，いつ設定するのか？
		// →こういうメソッドを用意してみては？

		/// ↓元のParentに依存した処理を行うならば，OnRemovedFromみたいなメソッドを用意する必要が出てきます．

		/// <summary>
		/// 自身がparentに追加された時に呼び出します．
		/// 除外されたときは引数をnullにして呼び出せばいいのかな？
		/// </summary>
		/// <param name="parent"></param>
		public virtual void OnAddedTo(QuestionsCollection parent)
		{
			this.Parent = parent;
		}

		// で，QuestionsCollectionのCollectionChangedから呼び出す，と．

		#endregion


		// このあたり本来はXNameなんだけど，手抜きをする．
		public const string ELEMENT_NAME = "intro";
		const string ID_ATTRIBUTE = "id";
		const string NO_ATTRIBUTE = "no";
		const string SONG_ID_ATTRIBUTE = "song_id";
		const string CATEGORY_ATTRIBUTE = "category";
		const string PLAY_POS_ATTRIBUTE = "play_pos";


		// (0.3.3)
		#region *[static]XML要素からオブジェクトを生成(Generate)
		public static IntroQuestion Generate(XElement questionElement)
		{
			int song_id = (int)questionElement.Attribute(SONG_ID_ATTRIBUTE);
			IntroQuestion question = new IntroQuestion(song_id);

			// XMLからインスタンスを生成するならばIDは常にあるのでは？
			// →songをインポートする時とかはそうではないかもしれない？ので一応有無をチェックする．
			// →でも，インポートする時はXML経由ではなくオブジェクト経由で行った方がいいのでは？(ファイル名のパスの扱いとか...)
			var id_attribute = questionElement.Attribute(ID_ATTRIBUTE);
			if (id_attribute != null)
			{
				question.ID = (int)id_attribute;
			}

			question.No = (int?)questionElement.Attribute(NO_ATTRIBUTE);
			question.Category = (string)questionElement.Attribute(CATEGORY_ATTRIBUTE);

			var play_pos = (double?)questionElement.Attribute(PLAY_POS_ATTRIBUTE);
			if (play_pos.HasValue)
			{ question.PlayPos = TimeSpan.FromSeconds(play_pos.Value); }

			return question;
		}
		#endregion


		// (0.3.3)Songからのコピペ。共通実装にしますか？
		#region INotifyPropertyChanged実装

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		protected void NotifyPropertyChanged(string propertyName)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		// (0.3.3)Songからのコピペ。共通実装にしますか？
		#region INotifyPropertyChanging実装
		public event PropertyChangingEventHandler PropertyChanging = delegate { };
		protected void NotifyPropertyChanging(string propertyName)
		{
			this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
		}
		#endregion


	}




}
