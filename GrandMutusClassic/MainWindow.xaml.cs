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
using System.Windows.Threading;	// for DispatcherTimer.

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
		public partial class MainWindow : WindowController
		{
			public MainWindow()
			{
				InitializeComponent();

				_songPlayer.Volume = App.Current.MySettings.SongPlayerVolume;
				_songPlayer.MediaOpened += SongPlayer_MediaOpened;

				this.MyDocument.Initialized += MyDocument_Initialized;
				// ↓この設定を忘れると，曲ファイル追加時に画面が固まるかも．
				this.MyDocument.AddSongsAction = this.AddSongsParallel;

				
				// ファイル履歴ショートカットの作成位置を指定する．
				this.FileHistoryShortcutParent = menuItemHistory;
			}

			// (0.3.1)起動時、「新規作成」時に呼び出されるはず。
			void MyDocument_Initialized(object sender, EventArgs e)
			{
				_songPlayer.Close();
				_currentSong = null;
			}

			// (0.3.1)
			private void MainWindow_Closed(object sender, EventArgs e)
			{
				App.Current.MySettings.SongPlayerVolume = _songPlayer.Volume;
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

			// (0.3.3)
			private void SetRootDirectory_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				Aldentea.Wpf.Controls.FolderBrowserDialog dialog
					= new Aldentea.Wpf.Controls.FolderBrowserDialog
					{
						AllowNew = false,
						Description = "曲ファイルが格納されているフォルダを指定して下さい。",
						DisplaySpecialFolders = Aldentea.Wpf.Controls.SpecialFoldersFlag.Personal | Aldentea.Wpf.Controls.SpecialFoldersFlag.MyMusic,
						SelectedPath = MyDocument.Songs.RootDirectory
					};

				if (dialog.ShowDialog() == true)
				{
					MyDocument.Songs.RootDirectory = dialog.SelectedPath;
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

			#region Undo

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

			#endregion


			// (0.3.4)
			#region AddQuestions

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
				MyDocument.AddIntroQuestions(songs);
			}

			// (0.3.4)
			void AddQuestions_CanExecute(object sender, CanExecuteRoutedEventArgs e)
			{
				e.CanExecute = e.Parameter is Song || e.Parameter is IEnumerable<Song> || e.Parameter is System.Collections.IList;
			}

			#endregion

			#endregion


			#region 曲再生関連

			#region *SongPlayerプロパティ
			public HyperMutus.SongPlayer SongPlayer
			{
				get
				{
					return _songPlayer;
				}
			}
			HyperMutus.SongPlayer _songPlayer = new SongPlayer();
			#endregion

			// (0.3.2)プロパティ化。
			#region *CurrentSongプロパティ
			public Song CurrentSong
			{
				get { return _currentSong; }
				set
				{
					if (_currentSong != value)
					{
						_currentSong = value;
						NotifyPropertyChanged("CurrentSong");
					}
				}
			}
			Song _currentSong = null;
			#endregion


			//DispatcherTimer _songPlayerTimer = null;

			// (0.3.2)
			void SongPlayer_MediaOpened(object sender, EventArgs e)
			{
				if (_songPlayer.Duration.HasValue)
				{
					this.labelDuration.Content = _songPlayer.Duration.Value;
					this.sliderSeekSong.Maximum = _songPlayer.Duration.Value.TotalSeconds;
				}
			}


			#region Playコマンド

			void Play_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				if (e.Parameter is Song)
				{
					Song song = (Song)e.Parameter;
					_songPlayer.Open(song.FileName);
					
					//_songPlayerTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.25) };	// 可変にする？
					//_songPlayerTimer.Tick += SongPlayerTimer_Tick;
					//_songPlayerTimer.IsEnabled = true;

					this.CurrentSong = song;
					_songPlayer.Play();
				}
			}

			void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
			{
				e.CanExecute = e.Parameter is Song;
			}

			#endregion

			#region SwitchPlayPauseコマンド

			void SwitchPlayPause_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				if (_songPlayer.CurrentState != SongPlayer.State.Inactive)
				{
					_songPlayer.TogglePlayPause();
				}
			}

			#endregion


			void SeekRelative_executed(object sender, ExecutedRoutedEventArgs e)
			{
 				if (_songPlayer.CurrentState != HyperMutus.SongPlayer.State.Inactive)
				{
					double sec;
					if (Double.TryParse(e.Parameter.ToString(), out sec))
					{
						_songPlayer.CurrentPosition = _songPlayer.CurrentPosition.Add(TimeSpan.FromSeconds(sec));
					}
				}
			}


			void SeekSabi_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				if (_songPlayer.CurrentState != SongPlayer.State.Inactive)
				{
					_songPlayer.CurrentPosition = _currentSong.SabiPos;
				}

			}

			void SongPlayer_CanExecute(object sender, CanExecuteRoutedEventArgs e)
			{
				e.CanExecute = _songPlayer.CurrentState != SongPlayer.State.Inactive;
			}

			#region SetSabiPosコマンド

			void SetSabiPos_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				if (_songPlayer.CurrentState != SongPlayer.State.Inactive)
				{
					_currentSong.SabiPos = _songPlayer.CurrentPosition;
				}
			}

			#endregion

			private void UpDownControl_UpClick(object sender, RoutedEventArgs e)
			{
				if (CurrentSong != null)
				{
					CurrentSong.SabiPos += TimeSpan.FromSeconds(0.1);
				}
			}

			private void UpDownControl_DownClick(object sender, RoutedEventArgs e)
			{
				if (CurrentSong != null)
				{
					CurrentSong.SabiPos += TimeSpan.FromSeconds(-0.1);
				}

			}

			#endregion



		}
		#endregion

	}

}