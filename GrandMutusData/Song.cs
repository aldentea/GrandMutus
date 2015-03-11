using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Xml.Linq;

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
		int _id;

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



		#region XML出力関連

		// どこに置くかは未定．
		// ここに置くか，XML生成専用のクラスを作成するか...


		// このあたり本来はXNameなんだけど，手抜きをする．
		const string ELEMENT_NAME = "song";
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
