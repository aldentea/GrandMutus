using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Xml.Linq;
using System.IO;

namespace GrandMutus.Data
{

	public class Song : INotifyPropertyChanged
	{
		// 問題1. IDをどうやって付与しようか？
		// →コンストラクタをinternalにする．
		// インスタンスはMutusDocumentでのみ作成するようにする．
		// そこでIDを付与する．
		// 付与するときも，internalなsetterを用いる．

		public int ID
		{
			get { return _id; }
			internal set { _id = value; }
		}
		int _id = -1;	// (0.1.2)-1は未設定であることを示す．

		#region *Titleプロパティ
		/// <summary>
		/// 曲のタイトルを取得／設定します．
		/// </summary>
		public string Title
		{
			get
			{
				return _title;
			}
			set
			{
				if (Title != value)
				{
					this._title = value;
					NotifyPropertyChanged("Title");
				}
			}
		}
		string _title = string.Empty;
		#endregion

		#region *FileNameプロパティ
		/// <summary>
		/// 曲のファイル名を(とりあえずフルパスで)取得／設定します．
		/// </summary>
		public string FileName
		{
			get
			{
				return _fileName;
			}
			set
			{
				if (FileName != value)
				{
					this._fileName = value;
					NotifyPropertyChanged("FileName");
				}
			}
		}
		string _fileName = string.Empty;
		#endregion

		#region *Artistプロパティ
		/// <summary>
		/// 曲のアーティストを取得／設定します．
		/// </summary>
		public string Artist
		{
			get
			{
				return _artist;
			}
			set
			{
				if (Artist != value)
				{
					this._artist = value;
					NotifyPropertyChanged("Artist");
				}
			}
		}
		string _artist = string.Empty;
		#endregion

		public string RelativeFileName
		{
			get
			{
				if (this.FileName.StartsWith(Parent.RootDirectory))
				{
					return this.FileName.Substring(Parent.RootDirectory.Length).TrimStart('\\');
				}
				else
				{
					return this.FileName;
				}
			}
		}


		// 問題2. 親要素への参照を設定できるか？

		#region *Parentプロパティ
		SongsCollection Parent
		{
			get { return _parent; }
			set
			{
				if (this.Parent != value)
				{
					this._parent = value;
					NotifyPropertyChanged("Parent");
					NotifyPropertyChanged("RelativeFileName");
				}
			}
		}
		SongsCollection _parent = null;
		#endregion

		// こういうプロパティを用意して，いつ設定するのか？
		// →こういうメソッドを用意してみては？

		/// ↓元のParentに依存した処理を行うならば，OnRemovedFromみたいなメソッドを用意する必要が出てきます．

		/// <summary>
		/// 自身がparentに追加された時に呼び出します．
		/// 除外されたときは引数をnullにして呼び出せばいいのかな？
		/// </summary>
		/// <param name="parent"></param>
		public virtual void OnAddedTo(SongsCollection parent)
		{
			this.Parent = parent;
		}

		// で，SongsCollectionのCollectionChangedから呼び出す，と．






		#region XML入出力関連

		// どこに置くかは未定．
		// ここに置くか，XML生成専用のクラスを作成するか...


		// このあたり本来はXNameなんだけど，手抜きをする．
		public const string ELEMENT_NAME = "song";	// (0.1.2)publicに変更．
		const string ID_ATTRIBUTE = "id";
		const string TITLE_ELEMENT = "title";
		const string ARTIST_ELEMENT = "artist";
		const string FILE_NAME_ELEMENT = "file_name";


		/// <summary>
		/// 
		/// </summary>
		/// <param name="songs_root">曲ファイルを格納しているディレクトリのフルパスです．</param>
		/// <returns></returns>
		public XElement GenerateElement(string songs_root = null)
		{
			var element = new XElement(ELEMENT_NAME);
			element.Add(new XAttribute(ID_ATTRIBUTE, this.ID));
			element.Add(new XElement(TITLE_ELEMENT, this.Title));
			element.Add(new XElement(ARTIST_ELEMENT, this.Artist));

			// XMLに出力する曲のファイル名．
			string file_name;
			if (!string.IsNullOrEmpty(songs_root) && this.FileName.Contains(songs_root))
			{
				// songs_rootからの相対パスを記録．
				file_name = this.FileName.Substring(songs_root.Length).TrimStart('\\');
			}
			else
			{
				// フルパスを記録．
				file_name = this.FileName;
			}
			element.Add(new XElement(FILE_NAME_ELEMENT, file_name));
			return element;
		}

		// (0.1.2)
		public static Song Generate(XElement songElement, string songsRoot = null)
		{
			Song song = new Song();

			// XMLからインスタンスを生成するならばIDは常にあるのでは？
			// →songをインポートする時とかはそうではないかもしれない？ので一応有無をチェックする．
			// →でも，インポートする時はXML経由ではなくオブジェクト経由で行った方がいいのでは？(ファイル名のパスの扱いとか...)
			var id_attribute = songElement.Attribute(ID_ATTRIBUTE);
			if (id_attribute != null)
			{
				song.ID = (int)id_attribute;
			}
			song.Title = (string)songElement.Element(TITLE_ELEMENT);
			song.Artist = (string)songElement.Element(ARTIST_ELEMENT);
			var file_name = (string)songElement.Element(FILE_NAME_ELEMENT);	// 相対パスをフルパスに直す作業が必要！
			if (!Path.IsPathRooted(file_name))
			{
				file_name = Path.Combine(songsRoot, file_name);
				if (!Path.IsPathRooted(file_name))
				{
					throw new ArgumentException("ファイル名が相対パスで記録されています．songsRootには，絶対パスを指定して下さい．", "songsRoot");
				}
			}
			song.FileName = file_name;
			return song;
		}


		#endregion


		#region INotifyPropertyChanged実装

		public event PropertyChangedEventHandler PropertyChanged = delegate {};

		protected void NotifyPropertyChanged(string propertyName)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

	}

}
