using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using HyperMutus;

namespace GrandMutus
{
	using Data;

	namespace Classic
	{
		// (0.2.0)
		// 3. 継承元を変更する(XAMLも忘れずに！)．

		#region MainWindowクラス
		/// <summary>
		/// MainWindow.xaml の相互作用ロジック
		/// </summary>
		public partial class MainWindow : Aldentea.Wpf.Application.BasicWindow
		{
			public MainWindow()
			{
				InitializeComponent();

				// ↓この設定を忘れると，曲ファイル追加時に画面が固まるかも．
				this.MyDocument.AddSongsAction = this.AddSongsParallel;

				
				// ファイル履歴ショートカットの作成位置を指定する．
				this.FileHistoryShortcutParent = menuItemHistory;
			}

			// 4. 抽象メンバを実装する．
			// ...ファイル履歴関連のメンバなんですが，
			// これって，Windowに実装するべきなのでしょうか？Appに実装するべきだという気がしているのですが，
			// UIに実装する上で何か不都合があるでしょうか？

			#region BasicWindow抽象メンバの実装

			protected override System.Collections.Specialized.StringCollection FileHistory
			{
				get
				{
					return App.Current.MySettings.FileHistory;
				}
				set
				{
					App.Current.MySettings.FileHistory = value;
				}
			}

			protected override byte FileHistoryCount
			{
				get { return App.Current.MySettings.FileHistoryCount; }
			}

			protected override byte FileHistoryDisplayCount
			{
				get { return App.Current.MySettings.FileHistoryDisplayCount; }
			}

			#endregion

			// 5. DataContextの設定？...は不要だった．
			public MutusDocument MyDocument
			{
				// AppのDocumentプロパティに設定したオブジェクトが，自動的にDataContextに設定される．
				// NewDocumentプロパティも同じものを指す．
				get { return (MutusDocument)NewDocument; }
			}

			// 6. XAML側では，Titleの設定をしましょう．(これはAldenteaWpfUtility.dllが必要になる．)


			#region コマンドハンドラ

			// (0.2.0)
			private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				this.Close();
			}

			// (0.2.1)
			private void AddSongs_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				var fileNames = Helpers.SelectSongFiles();
				if (fileNames != null)
				{
					AddSongs(fileNames);
					//SayInfo("曲追加完了！");
				}
			}

			#endregion


			// (0.2.3)実質の処理をAddSongsParallelに移動(Document側の仕様変更に対応)．
			// (0.2.1)HyperMutusから導入．AldenteaBackgroundWorkerDialog(1.1.0.0)が必要．
			// 10/20/2014 by aldentea : AddSongの仕様変更に対応．
			// 10/06/2014 by aldentea : UseAutoSaveOperationHistoryプロパティの制御を追加．
			// 07/15/2014 by aldentea : 並列化？
			// 11/08/2013 by aldentea : BackgroundWorkerDialog周辺の処理をHelpersに移動．
			// 09/07/2011 by aldentea
			#region 曲を追加(AddSongs)
			public void AddSongs(IEnumerable<string> fileNames)
			{
				this.MyDocument.AddSongs(fileNames);
			}
			
			IList<Song> AddSongsParallel(IEnumerable<string> fileNames)
			{
				//Dictionary<int, string> fileDictionary = new Dictionary<int, string>();
				List<Song> added_songs = new List<Song>();

				//this.MyDocument.UseAutoSaveOperationHistory = false;
				try
				{
					Action<string> action = (fileName) =>
					{
						try
						{
							// ObservableCollectionに対する操作は，それが作られたスレッドと同じスレッドで行う必要がある．

							var song = this.Dispatcher.Invoke(
								new Func<string, Song>(delegate(string f) { return MyDocument.AddSong(f); }), fileName);
							if (song is Song)
							{
								added_songs.Add((Song)song);
							}
							// idを返すのって何のためだっけ？
							//var id = this.Dispatcher.Invoke(
							//	new Func<string, int>(delegate(string f) { return MyDocument.AddSong(f).ID; }), fileName);
							//if (id.HasValue)
							//{
							//	fileDictionary.Add(id.Value, fileName);
							//}
						}
						catch (SongDuplicateException)
						{
							// 何か知らせる？
						}
					};
					Helpers.WorkBackgroundParallel<string>(fileNames, action);
					//this.MyDocument.AddOperationHistory(
					//	new AddSongsOperationCache(this.MyDocument, fileDictionary)
					//);
				}
				finally
				{
					//this.MyDocument.UseAutoSaveOperationHistory = true;
				}
				return added_songs;
			}
			#endregion


			#region コマンドハンドラ

			private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				if (MyDocument.CanUndo)
				{
					MyDocument.Undo();
				}
			}

			private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
			{
				e.CanExecute = MyDocument.CanUndo;
			}


			// (0.3.4)
			void AddQuestions_Execute(object sender, ExecutedRoutedEventArgs e)
			{
				if (e.Parameter is Song)
				{ }
				else if (e.Parameter is IEnumerable<Song>)
				{
					AddQuestions((IEnumerable<Song>)e.Parameter);
				}
				else if (e.Parameter is System.Collections.IList)
				{
					AddQuestions(((System.Collections.IList)e.Parameter).Cast<Song>());
				}

			}

			void AddQuestions(IEnumerable<Song> songs)
			{
				//MyDocument
			}

			// (0.3.4)
			void AddQuestions_CanExecute(object sender, CanExecuteRoutedEventArgs e)
			{
				e.CanExecute = e.Parameter is Song || e.Parameter is IEnumerable<Song> || e.Parameter is System.Collections.IList;
			}


			#endregion

		}
		#endregion

	}

}