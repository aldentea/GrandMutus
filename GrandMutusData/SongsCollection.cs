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

	public class SongsCollection : ObservableCollection<Song>
	{
		/// <summary>
		/// 曲ファイルを格納するディレクトリのフルパスを取得／設定します．
		/// </summary>
		public string RootDirectory { get; set; }

		public SongsCollection()
		{
			this.CollectionChanged += SongsCollection_CollectionChanged;
		}

		void SongsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (var item in e.NewItems)
					{
						var song = (Song)item;
						// IDを付与する．
						// (0.1.2)IDが既に設定されているかどうかを確認．
						if (song.ID < 0)
						{
							song.ID = GenerateNewID();
						}
						// ☆songのプロパティ変更をここで受け取る？MutusDocumentで行えばここでは不要？
						song.OnAddedTo(this);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (var item in e.OldItems)
					{
						var song = (Song)item;

						// どうにかする．
						song.OnAddedTo(null);
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					// ClearしたときにはResetが発生するのか？
					break;

			}
		}

		/// <summary>
		/// 初期化します．
		/// </summary>
		public void Initialize()
		{
			this.Clear();
			this.RootDirectory = string.Empty;

		}


		#region XML入出力関連

		public const string ELEMENT_NAME = "songs";
		const string PATH_ATTRIBUTE = "path";

		/// <summary>
		/// XMLファイルを出力するディレクトリです．
		/// </summary>
		/// <param name="destinationDir"></param>
		/// <returns></returns>
		public XElement GenerateElement(string destinationDir)
		{
			XElement element = new XElement(ELEMENT_NAME);
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
				element.Add(new XAttribute(PATH_ATTRIBUTE, songs_root));
			}

			foreach (Song song in this.Items)
			{
				element.Add(song.GenerateElement(songs_root));
			}

			return element;
		}

		// (0.1.2)
		public void LoadElement(XElement songsElement)
		{
			this.RootDirectory = (string)songsElement.Attribute(PATH_ATTRIBUTE);
			foreach (var song_element in songsElement.Elements(Song.ELEMENT_NAME))
			{
				this.Add(Song.Generate(song_element, this.RootDirectory));
			}
		}

		#endregion




		#region ID管理関連

		int GenerateNewID()
		{
			int new_id = this.UsedIDList.Any() ? this.UsedIDList.Max() + 1 : 1;
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

}
