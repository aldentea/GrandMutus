using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;
using System.Xml;
using System.IO;

using Aldentea.Wpf.Document;

namespace GrandMutus.Data
{
	// (0.5.0)継承元をAldentea.Wpf.Document.DocumentWIthOperationHistory(バージョン3の実装)に変更．
	// (0.2.0)継承元をDocumentWithOperationHistoryに変更．
	public class MutusDocument : DocumentWithOperationHistory
	{
		// とりあえずpublicにしてみる．
		public SongsCollection Songs { get { return _songs; } }
		readonly SongsCollection _songs;

		// (0.3.3)とりあえずIntroQuestionだけに対応している。
		public QuestionsCollection Questions { get { return _questions; } }
		readonly QuestionsCollection _questions;

		#region *WriterSettingsプロパティ
		public XmlWriterSettings WriterSettings
		{
			get { return _xmlWriterSettings; }
		}
		XmlWriterSettings _xmlWriterSettings;
		#endregion

		// (0.4.4.1)_songs.SongRemovedのハンドルを一旦解除(というか，イベント自体を削除した)．
		// (0.3.3)Questions関連処理を追加。
		#region *コンストラクタ(MutusDocument)
		public MutusDocument()
		{
			// Songs関連処理

			_songs = new SongsCollection();
			//_songs.CollectionChanged += Songs_CollectionChanged;
			_songs.ItemChanged += Songs_ItemChanged;
			//_songs.SongsRemoved += Songs_SongsRemoved;
			_songs.RootDirectoryChanged += Songs_RootDirectoryChanged;


			// Questions関連処理
			_questions = new QuestionsCollection(this);

			_questions.QuestionsRemoved += Questions_QuestionsRemoved;
			_questions.QuestionNoChanged += Questions_QuestionNoChanged;

			// XML出力関連処理
			_xmlWriterSettings = new XmlWriterSettings
			{
				Indent = true,
				NewLineHandling = NewLineHandling.Entitize
			};
		}

		#endregion

		// (0.4.4)
		void Songs_RootDirectoryChanged(object sender, ValueChangedEventArgs<string> e)
		{
			this.AddOperationHistory(new SongsRootDirectoryChangedCache(this.Songs, e.PreviousValue, e.CurrentValue));
		}

		// (0.2.0)Songs以外でも共通に使えるのではなかろうか？
		void Songs_ItemChanged(object sender, ItemEventArgs<IOperationCache> e)
		{
			var operationCache = (IOperationCache)e.Item;
			if (operationCache != null)
			{
				this.AddOperationHistory(operationCache);
			}
		}


		#region 曲関連

		// Songオブジェクトはここで(のみ)作るようにする？

		/// <summary>
		/// (0.3.4)現時点では未使用！
		/// </summary>
		List<string> _addedSongFiles = new List<string>();

		#region *曲を追加(AddSongs)
		/// <summary>
		/// このメソッドが直接呼び出されることは想定していません．
		/// 呼び出し元でAddSongsActionに設定されるActionの中で呼び出して下さい(ややこしい...)．
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public Song AddSong(string fileName)
		{
			Song song = new Song { FileName = fileName };
			LoadInformation(song);
			return this.AddSong(song);
		}

		private Song AddSong(Song song)
		{
			try
			{
				_songs.Add(song);	// この後の処理でSongDuplicateExceptionが発生する。
				_addedSongFiles.Add(song.FileName);
				//SongAdded(this, new ItemEventArgs<Song> { Item = song });
				return song;
			}
			catch (SongDuplicateException)
			{
				// この時点ではsongが_songsに追加された状態になっているので、ここで削除する。
				_songs.Remove(song);
				return null;
			}
		}


		public void AddSongs(IEnumerable<string> fileNames)
		{
			// この時点では既に入っているファイルと重複している可能性があるので，ここで除いておく．
			var excepted_file_names = fileNames.Where(fileName => !_songs.Any(s => s.FileName == fileName));
			IList<Song> added_songs;

			if (AddSongsAction == null)
			{
				// 同期的に実行．
				added_songs = new List<Song>();
				foreach (var fileName in excepted_file_names)
				{
					var song = AddSong(fileName);
					if (song != null)
					{ added_songs.Add(song); }
				}
			}
			else
			{
				// 通常は呼び出し元に制御を渡して，UIを表示する．
				added_songs = AddSongsAction.Invoke(excepted_file_names);
			}
			
			AddOperationHistory(new SongsAddedCache(this, added_songs.ToArray()));
		}

		// 曲削除をアンドゥしたときに使うことを想定しています。
		public void AddSongs(IEnumerable<Song> songs)
		{
			// ここは同期実行でいいでしょう。
			// 同期的に実行．
			var added_songs = new List<Song>();
			foreach (var song in songs)
			{
				var added_song = AddSong(song);
				if (added_song != null)
				{ added_songs.Add(added_song); }
			}
			AddOperationHistory(new SongsAddedCache(this, added_songs.ToArray()));

		}
		#endregion

		public Func<IEnumerable<string>, IList<Song>> AddSongsAction = null;

		// (0.3.0)
		#region *曲を削除(RemoveSongs)
		public void RemoveSongs(IEnumerable<string> fileNames)
		{
			RemoveSongs(fileNames.Select(fileName => _songs.FirstOrDefault(s => s.FileName == fileName)).Where(s => s != null));
		}

		// (0.4.4.1)↓UIからの削除もこのメソッドを経由することにしたので，OperationCacheの処理をここで行う．
		// (0.3.1)OperationCacheの追加はSongsRemovedイベントハンドラで行うことにする
		// (曲の削除はUIから直接行われることが想定されるため)．
		// (0.3.0)
		public void RemoveSongs(IEnumerable<Song> songs)
		{
			IList<Song> removed_songs = new List<Song>();

			// songsで直接foreachしたいが，songs自身が変更されるのでそれは不可！
			var song_ids = songs.Select(song => song.ID).ToArray();
			foreach (var song_id in song_ids)
			{
				var song = _songs.First(s => s.ID == song_id);
				if (song != null)
				{
					_songs.Remove(song);
					removed_songs.Add(song);
				}
			}
			AddOperationHistory(new SongsRemovedCache(this, removed_songs));
		}
		#endregion

		// (0.4.4.1)ここにあった処理をRemoveSongsメソッドに移動．
		// (0.3.1)
		//void Songs_SongsRemoved(object sender, ItemEventArgs<IEnumerable<Song>> e)
		//{
			//AddOperationHistory(new SongsRemovedCache(this, e.Item));
		//}

		// (0.4.3.1)サビ位置が読み込まれていなかったのを修正。
		// HyperMutusからのパクリ．古いメソッドだけど，とりあえずそのまま使う．
		// 場所も未定．とりあえずstatic化してここに置いておく．
		#region *[static]ファイルからメタデータを読み込み(LoadInformation)
		/// <summary>
		/// songのFileNameプロパティで指定されたファイルからメタデータを読み込みます．
		/// </summary>
		static void LoadInformation(Song song)
		{
			SPP.Aldente.IID3Tag tag = SPP.Aldente.AldenteMP3TagAccessor.ReadFile(song.FileName);
			song.Title = tag == null ? string.Empty : tag.Title;
			song.Artist = tag == null ? string.Empty : tag.Artist;
			song.SabiPos = tag == null ? TimeSpan.Zero : TimeSpan.FromSeconds(Convert.ToDouble(tag.SabiPos));
		}
		#endregion



		/// <summary>
		/// 曲が追加された時に発生します．
		/// </summary>
		//public event EventHandler<ItemEventArgs<Song>> SongAdded = delegate { };

		public Song GetSong(int id)
		{
			return Songs.SingleOrDefault(song => song.ID == id);
		}

		#endregion


		#region 問題関連

		// (0.3.4)とりあえず．
		public void AddIntroQuestions(IEnumerable<Song> songs)
		{
			var added_questions = new List<IntroQuestion>();
			foreach (var song in songs)
			{
				var question = new IntroQuestion(song.ID);
				_questions.Add(question);
				added_questions.Add(question);
			}
			// ここで操作履歴処理を行う．(削除の場合と異なりUIから直接，というのは考えられない．)
			AddOperationHistory(new QuestionsAddedCache(this, added_questions.ToArray()));
		}

		// (0.4.1)
		#region *問題を追加(AddQuestions)
		private Question AddQuestion(Question question)
		{
			_questions.Add(question);	// ←失敗することは通常想定されないよね？
			return question;
		}

		/// <summary>
		/// 問題削除をアンドゥしたときに使うことを想定しています。
		/// </summary>
		/// <param name="questions"></param>
		public void AddQuestions(IEnumerable<Question> questions)
		{
			var added_questions = new List<Question>();
			foreach (var question in questions)
			{
				var added_song = AddQuestion(question);
				if (added_song != null)
				{ added_questions.Add(added_song); }
			}
			AddOperationHistory(new QuestionsAddedCache(this, added_questions.ToArray()));

		}
		#endregion

		// (0.4.1)
		void Questions_QuestionsRemoved(object sender, ItemEventArgs<IEnumerable<Question>> e)
		{
			AddOperationHistory(new QuestionsRemovedCache(this, e.Item));
		}


		#region *問題を削除(RemoveQuestions)

		// (0.4.1)OperationCacheの追加はSongsRemovedイベントハンドラで行うことにする
		// (曲の削除はUIから直接行われることが想定されるため)．
		public void RemoveQuestions(IEnumerable<Question> questions)
		{
			IList<string> removed_song_files = new List<string>();
			foreach (var question in questions)
			{
				if (_questions.Remove(question))
				{
					// ↓これって何かに使っているっけ？
					//removed_song_files.Add(question.FileName);
				}
			}
		}

		#endregion

		// (0.4.5.1)
		void Questions_QuestionNoChanged(object sender, ValueChangedEventArgs<int?> e)
		{
			var question = (Question)sender;
			AddOperationHistory(new QuestionNoChangedCache(question, e.PreviousValue, e.CurrentValue));
		}


		#endregion



		#region XML出力関連

		public const string ROOT_ELEMENT_NAME = "mutus";
		const string VERSION_ATTERIBUTE = "version";

		// (0.3.3.1)Questionsの出力を追加．
		public XDocument GenerateXml(string destination)
		{
			XDocument xdoc = new XDocument(new XElement(ROOT_ELEMENT_NAME, new XAttribute(VERSION_ATTERIBUTE, "3.0")));
			xdoc.Root.Add(Songs.GenerateElement(System.IO.Path.GetDirectoryName(destination)));
			xdoc.Root.Add(Questions.GenerateElement());
			return xdoc;
		}

		#endregion


		#region DocumentBase実装

		// (0.5.0)継承元のメソッド名変更に対応．
		protected override void InitializeDocument()
		{
			Songs.Initialize();
		}

		// (0.4.0.1)Songs.RootDirectoryの設定を追加。
		protected override bool LoadDocument(string fileName)
		{
			using (XmlReader reader = XmlReader.Create(fileName))
			{
				var xdoc = XDocument.Load(reader);
				var root = xdoc.Root;

				// ☆ここから下は，継承先でオーバーライドできるようにしておきましょう．
				if (root.Name == ROOT_ELEMENT_NAME)
				{
					decimal? version = (decimal?)root.Attribute(VERSION_ATTERIBUTE);
					if (version.HasValue)
					{
						if (version >= 3.0M)
						{
							this.Songs.RootDirectory = Path.GetDirectoryName(fileName);	// RootDirectoryのデフォルトの値を設定する。
							this.Songs.LoadElement(root.Element(SongsCollection.ELEMENT_NAME));
							this.Questions.LoadElement(root.Element(QuestionsCollection.ELEMENT_NAME));
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
