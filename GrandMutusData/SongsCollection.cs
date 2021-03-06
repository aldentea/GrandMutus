﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using System.Collections.Specialized;
using System.Xml.Linq;
//using System.Diagnostics;
using Aldentea.Wpf.Document;

namespace GrandMutus.Data
{

	#region SongsCollectionクラス
	public class SongsCollection : ObservableCollection<Song>, ISongsCollection
	{


		// (0.4.4)set時に操作履歴に追加。
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
					var previous_value = this._rootDirectory;
					this._rootDirectory = value;
					UpdateRelativeFileNames();
					this.RootDirectoryChanged(this, new ValueChangedEventArgs<string>(previous_value, value));
				}
			}
		}
		string _rootDirectory = string.Empty;

		// (0.4.3)
		void UpdateRelativeFileNames()
		{
			foreach (Song song in this.Items)
			{
				song.UpdateRelativeFileName();
			}
		}
		#endregion

		// (0.4.4)RootDirecotyプロパティの値が変更されたときに発生します。
		public event EventHandler<ValueChangedEventArgs<string>> RootDirectoryChanged = delegate { };



		#region *コンストラクタ(SongsCollection)
		public SongsCollection()
		{
			this.CollectionChanged += SongsCollection_CollectionChanged;
			
		}
		#endregion



		#region コレクション変更関連

		// ☆曲をRemoveしてからUndoすると，IDが変わってしまう．
		// これはこういう仕様でいいでしょうか？後から困ったことにならないかなぁ？


		// (0.4.4.1)↓UIから曲を削除する場合もMutusDocumentを経由する方針になり，イベントの用途がなくなったので一旦削除．
		// (0.3.1)
		/// <summary>
		/// 曲が削除された時に発生します．
		/// </summary>
		//public event EventHandler<ItemEventArgs<IEnumerable<Song>>> SongsRemoved = delegate { };


		// (0.4.4.1)↓SongsRemovedイベントの発生処理を削除．
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
					// →UIから複数曲をまとめて削除すると、既定では1曲ずつこのイベントが呼び出される．それらをまとめてキャッシュする手段がない！？
					// →仕方がないので，UIの処理を修正し(0.3.4.1)，複数曲の削除をまとめて受け取るようにした．

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

					// (0.4.4.1)↓UIから曲を削除する場合もMutusDocumentを経由する方針になったので，この処理を削除．
					// (MutusDocumentを経由せずに)UIから削除される場合もあるので，
					// ここでOperationCacheの処理を行うことにした．
					//if (songs.Count > 0)
					//{
					//	this.SongsRemoved(this, new ItemEventArgs<IEnumerable<Song>> { Item = songs });
					//}
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

		// (0.6.4)RootDirectoryの処理を変更(SweetMutusDocumentと同様に...)．
		// path="."とpath属性がない場合を区別．
		// (0.4.0.2)RootDirectoryが指定されていなければここでオーバーライドしないように修正。
		// (0.1.2)
		public void LoadElement(XElement songsElement, string source_directory)
		{
			var path = (string)songsElement.Attribute(PATH_ATTRIBUTE);
			string songs_root;
			if (string.IsNullOrEmpty(path))
			{
				// RootDirectoryプロパティは設定されない．
				songs_root = source_directory;	// ロードするファイルのあるディレクトリが入っているはずである．
			}
			else
			{
				// 何らかの形でRootDirectoryプロパティが設定される．
				if (System.IO.Path.IsPathRooted(path))
				{
					this.RootDirectory = path;
				}
				else if (path == ".")
				{
					this.RootDirectory = source_directory;
				}
				else
				{
					this.RootDirectory = System.IO.Path.Combine(source_directory, path);
				}
				songs_root = this.RootDirectory;
			}

			foreach (var song_element in songsElement.Elements(Song.ELEMENT_NAME))
			{
				this.Add(Song.Generate(song_element, songs_root));
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
