using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace GrandMutus.Data
{

	public class MutusDocument : Aldentea.Wpf.Document.DocumentBase
	{
		// とりあえずpublicにしてみる．
		public SongsCollection Songs { get { return _songs; } }
		readonly SongsCollection _songs;

		#region *WriterSettingsプロパティ
		public XmlWriterSettings WriterSettings
		{
			get { return _xmlWriterSettings; }
		}
		XmlWriterSettings _xmlWriterSettings;
		#endregion

		public MutusDocument()
		{
			_songs = new SongsCollection();


			_xmlWriterSettings = new XmlWriterSettings
			{
				Indent = true,
				NewLineHandling = NewLineHandling.Entitize
			};
		}


		//public System.Collections.ObjectModel.ReadOnlyObservableCollection<Song>



		#region 曲関連

		// Songオブジェクトはここで(のみ)作るようにする？

		public Song AddSong(string fileName)
		{
			Song song = new Song { FileName = fileName };
			LoadInformation(song);
			_songs.Add(song);
			SongAdded(this, new ItemEventArgs<Song> { Item = song });
			return song;
		}

		// HyperMutusからのパクリ．古いメソッドだけど，とりあえずそのまま使う．
		// 場所も未定．とりあえずstatic化してここに置いておく．
		#region *ファイルからメタデータを読み込み(LoadInformation)
		/// <summary>
		/// songのFileNameプロパティで指定されたファイルからメタデータを読み込みます．
		/// </summary>
		static void LoadInformation(Song song)
		{
			SPP.Aldente.IID3Tag tag = SPP.Aldente.AldenteMP3TagAccessor.ReadFile(song.FileName);
			song.Title = tag == null ? string.Empty : tag.Title;
			song.Artist = tag == null ? string.Empty : tag.Artist;
			//song.SabiPos = tag == null ? 0.0M : tag.SabiPos;
		}
		#endregion

		/// <summary>
		/// 曲が追加された時に発生します．
		/// </summary>
		public event EventHandler<ItemEventArgs<Song>> SongAdded = delegate { };

		#endregion



		#region XML出力関連

		public const string ROOT_ELEMENT_NAME = "mutus";
		const string VERSION_ATTERIBUTE = "version";

		public XDocument GenerateXml(string destination)
		{
			XDocument xdoc = new XDocument(new XElement(ROOT_ELEMENT_NAME, new XAttribute(VERSION_ATTERIBUTE, "3.0")));
			xdoc.Root.Add(Songs.GenerateElement(System.IO.Path.GetDirectoryName(destination)));

			return xdoc;
		}

		#endregion


		#region DocumentBase実装

		protected override void Initialize()
		{
			Songs.Initialize();
		}

		protected override bool LoadDocument(string fileName)
		{
			using (XmlReader reader = XmlReader.Create(fileName))
			{
				var xdoc = XDocument.Load(reader);
				var root = xdoc.Root;

				// ☆ここから下は，継承先でオーバーライドできるようにしておく．
				if (root.Name == ROOT_ELEMENT_NAME)
				{
					decimal? version = (decimal?)root.Attribute(VERSION_ATTERIBUTE);
					if (version.HasValue)
					{
						if (version >= 3.0M)
						{
							this.Songs.LoadElement(root.Element(SongsCollection.ELEMENT_NAME));
							return true;
						}
					}
				}


			}
			return false;

		}

		protected override bool SaveDocument(string destination)
		{
			using (XmlWriter writer = XmlWriter.Create(destination, this.WriterSettings))
			{
				GenerateXml(destination).WriteTo(writer);
			}
			// 基本的にtrueを返せばよろしい．
			// falseを返すべきなのは，保存する前にキャンセルした時とかかな？
			return true;
		}
		#endregion


	}



}
