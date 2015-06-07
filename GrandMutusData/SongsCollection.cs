using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using System.Collections.Specialized;
using System.Xml.Linq;
//using System.Diagnostics;

namespace GrandMutus.Data
{

	#region SongsCollectionクラス
	public class SongsCollection : ObservableCollection<Song>
	{

		// TODO: set時には、操作履歴に追加する！
		// (0.4.3)実装を変更し、この変更を各Songに通知するように修正。
		#region *RootDirectoryプロパティ
		/// <summary>
		/// 曲ファイルを格納するディレクトリのフルパスを取得／設定します．
		/// </summary>
		public string RootDirectory {
			get
			{
				return this._rootDirectory;
			}
			set
			{
				if (this._rootDirectory != value)
				{
					this._rootDirectory = value;
					UpdateRelativeFileNames();
				}
			}
		}
		string _rootDirectory = string.Empty;

		void UpdateRelativeFileNames()
		{
			foreach (Song song in this.Items)
			{
				song.UpdateRelativeFileName();
			}
		}
		#endregion


		#region *コンストラクタ(SongsCollection)
		public SongsCollection()
		{
			this.CollectionChanged += SongsCollection_CollectionChanged;
		}
		#endregion



		#region コレクション変更関連

		// ☆曲をRemoveしてからUndoすると，IDが変わってしまう．
		// これはこういう仕様でいいでしょうか？後から困ったことにならないかなぁ？


		// (0.3.1)
		/// <summary>
		/// 曲が削除された時に発生します．
		/// </summary>
		public event EventHandler<ItemEventArgs<IEnumerable<Song>>> SongsRemoved = delegate { };


		// (0.3.1)曲の削除時にSongsRemovedイベントを発生． 
		// (0.3.0)すでにある曲を追加したときの処理を追加．
		void SongsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (var item in e.NewItems)
					{
						var song = (Song)item;

						if (this.Items.Any(s => s != song && s.FileName == song.FileName))
						{
							// 既にある曲ファイルと重複している！
							//this.Remove(song); // ←CollectionChangedイベントハンドラ内でこれはできない！
							throw new SongDuplicateException { FileName = song.FileName };
						}

						// IDを付与する．
						// (0.1.2)IDが既に設定されているかどうかを確認．
						if (song.ID <= 0)	// 無効な値．
						{
							song.ID = GenerateNewID();
						}
						// ☆songのプロパティ変更をここで受け取る？MutusDocumentで行えばここでは不要？
						// ↑とりあえずこのクラスで使っています。
						song.PropertyChanging += Song_PropertyChanging;
						song.PropertyChanged += Song_PropertyChanged;
						song.OnAddedTo(this);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					//★IList<string> song_files = new List<string>();
					IList<Song> songs = new List<Song>();
					foreach (var song in e.OldItems.Cast<Song>())
					{
						// 削除にあたって、変更通知機能を抑止。
						song.PropertyChanging -= Song_PropertyChanging;
						song.PropertyChanged -= Song_PropertyChanged;
						//★song_files.Add(song.FileName);
						songs.Add(song);
						// どうにかする．
						//song.OnAddedTo(null);
					}
					// (MutusDocumentを経由せずに)UIから削除される場合もあるので，
					// ここでOperationCacheの処理を行うことにした．
					if (songs.Count > 0)
					{
						this.SongsRemoved(this, new ItemEventArgs<IEnumerable<Song>> { Item = songs });
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					// ClearしたときにはResetが発生するのか？←そのようです．
					break;

			}
		}

		#endregion

		#region アイテム変更関連

		/// <summary>
		/// 格納されているアイテムのプロパティ値が変化したときに発生します．
		/// </summary>
		public event EventHandler<ItemEventArgs<IOperationCache>> ItemChanged = delegate { };


		string _titleCache = string.Empty;	// 手抜き．Songオブジェクト自体もキャッシュするべき．
		string _artistCache = string.Empty;
		TimeSpan _sabiPosCache = TimeSpan.Zero;

		void Song_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
		{
			Song song = (Song)sender;
			switch(e.PropertyName)
			{
				case "Title":
					this._titleCache = song.Title;
					break;
				case "Artist":
					this._artistCache = song.Artist;
					break;
				case "SabiPos":
					this._sabiPosCache = song.SabiPos;
					break;
			}
		}

		void Song_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var song = (Song)sender;

			switch (e.PropertyName)
			{
				case "Title":
					this.ItemChanged(this, new ItemEventArgs<IOperationCache>
					{
						Item = new SongTitleChangedCache(song, _titleCache, song.Title)
					});
					_titleCache = string.Empty;
					break;
				case "Artist":
					this.ItemChanged(this, new ItemEventArgs<IOperationCache>
					{
						Item = new SongArtistChangedCache(song, _artistCache, song.Artist)
					});
					_artistCache = string.Empty;
					break;
				case "SabiPos":
					this.ItemChanged(this, new ItemEventArgs<IOperationCache>
					{
						Item = new SongSabiPosChangedCache(song, _sabiPosCache, song.SabiPos)
					});
					_sabiPosCache = TimeSpan.Zero;
					break;
			}
			// ドキュメントにNotifyしたい！？
			//e.PropertyName
		}

		#endregion


		/// <summary>
		/// 初期化します．
		/// </summary>
		public void Initialize()
		{
			this.Clear();
			this.RootDirectory = string.Empty;

		}

		// (0.4.2.1)RootDirectoryと出力ディレクトリが同じならpath属性を出力しないように修正。
		#region XML入出力関連

		public const string ELEMENT_NAME = "songs";
		const string PATH_ATTRIBUTE = "path";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="destinationDir">XMLファイルを出力するディレクトリです．</param>
		/// <returns></returns>
		public XElement GenerateElement(string destinationDir)
		{
			XElement element = new XElement(ELEMENT_NAME);
			// ※ロジックをもっと簡略化できそうだが…。
			string songs_root = null;
			if (!string.IsNullOrEmpty(RootDirectory))
			{
				if (RootDirectory.Contains(destinationDir))
				{
					songs_root = RootDirectory.Substring(destinationDir.Length).TrimStart('\\');	// 相対パス化．
				}
				else
				{
					songs_root = RootDirectory;
				}

				if (!string.IsNullOrEmpty(songs_root))
				{
					element.Add(new XAttribute(PATH_ATTRIBUTE, songs_root));
				}
			}

			foreach (Song song in this.Items)
			{
				element.Add(song.GenerateElement(songs_root));
			}

			return element;
		}

		// (0.4.0.2)RootDirectoryが指定されていなければここでオーバーライドしないように修正。
		// (0.1.2)
		public void LoadElement(XElement songsElement)
		{
			if (songsElement.Attribute(PATH_ATTRIBUTE) != null)
			{
				this.RootDirectory = (string)songsElement.Attribute(PATH_ATTRIBUTE);
			}

			foreach (var song_element in songsElement.Elements(Song.ELEMENT_NAME))
			{
				this.Add(Song.Generate(song_element, this.RootDirectory));
			}
		}

		#endregion




		#region ID管理関連

		// (0.2.1)無効なIDとして負の値を使うことにしたので，Anyに条件を付加．
		int GenerateNewID()
		{
			int new_id = this.UsedIDList.Any(n => n > 0) ? this.UsedIDList.Max() + 1 : 1;
			// ↑Max()は，要素が空ならInvalidOperationExceptionをスローする．

			//UsedIDList.Add(new_id);
			return new_id;
		}

		IEnumerable<int> UsedIDList
		{
			get
			{
				return Items.Select(song => song.ID);
			}
		}

		#endregion

	}
	#endregion


	// (0.3.0)
	#region SongDuplicateExceptionクラス
	public class SongDuplicateException : ApplicationException
	{
		public string FileName { get; set; }
	}
	#endregion

}
