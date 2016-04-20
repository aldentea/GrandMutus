using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using Aldentea.Wpf.Controls;

namespace GrandMutus
{
	using Data;

	namespace Base
	{
		public static class Helpers
		{
			// (0.3.0)HyperMutusからコピペ．
			// 11/08/2013 by aldentea : static化し，QuestionWindowからHelpersに移動．
			// 02/29/2012 by aldentea : 汎用の非同期処理メソッド．
			#region *[static]コレクションに対する非同期処理を実行(WorkBackground)
			/// <summary>
			/// コレクションに対する処理を非同期で実行します．
			/// 進捗報告ダイアログが表示され，途中でキャンセルすることも可能です．
			/// </summary>
			/// <typeparam name="T">コレクションの要素の型です．</typeparam>
			/// <param name="collection">処理対象のコレクションです．</param>
			/// <param name="action">コレクションの各要素に対して行うアクションです．</param>
			public static void WorkBackground<T>(IEnumerable<T> collection, Action<T> action)
			{
				BackgroundWorkerDialog bwd = new BackgroundWorkerDialog { Total = collection.Count() };
				BackgroundWorker bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
				bw.DoWork += new DoWorkEventHandler(
					(sender, e) =>
					{
						BackgroundWorker worker = (BackgroundWorker)sender;
						int i = 0;
						foreach (var item in (IEnumerable<T>)e.Argument)
						{
							if (bw.CancellationPending)
							{
								return;
							}
							// ここで個別の処理を行う．
							action.Invoke(item);

							worker.ReportProgress(++i);
						}
					});

				bw.ProgressChanged += (sender, e) => { bwd.Current = e.ProgressPercentage; };
				bw.RunWorkerCompleted += (sender, e) => { bwd.Close(); };
				bwd.CancelClicked += (sender, e) => { bw.CancelAsync(); };
				bw.RunWorkerAsync(collection);
				bwd.ShowDialog();
			}
			#endregion

			// (0.3.0)HyperMutusからコピペ．
			// 07/15/2014 by aldentea
			#region *[static]コレクションに対する非同期処理を並列実行(WorkBackgroundParallel)
			/// <summary>
			/// コレクションに対する処理を非同期で実行します．
			/// 進捗報告ダイアログが表示され，途中でキャンセルすることも可能です．
			/// </summary>
			/// <typeparam name="T">コレクションの要素の型です．</typeparam>
			/// <param name="collection">処理対象のコレクションです．</param>
			/// <param name="action">コレクションの各要素に対して行うアクションです．</param>
			public static void WorkBackgroundParallel<T>(IEnumerable<T> collection, Action<T> action)
			{
				BackgroundWorkerDialog bwd = new BackgroundWorkerDialog { Total = collection.Count() };
				BackgroundWorker bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
				int i = 0;
				Action<T, ParallelLoopState> loopAction = (item, loopState) =>
				{
					if (bw.CancellationPending)
					{
						loopState.Stop();
					}
					action.Invoke(item);
					bw.ReportProgress(Interlocked.Increment(ref i));
				};

				bw.DoWork += new DoWorkEventHandler(
					(sender, e) =>
					{
						Parallel.ForEach((IEnumerable<T>)e.Argument, loopAction);
					});

				bw.ProgressChanged += (sender, e) => { bwd.Current = e.ProgressPercentage; };
				bw.RunWorkerCompleted += (sender, e) => { bwd.Close(); };
				bwd.CancelClicked += (sender, e) => { bw.CancelAsync(); };
				bw.RunWorkerAsync(collection);
				bwd.ShowDialog();
			}
			#endregion


			// (0.3.3)song.FileNameを変更する必要はないので，それ用の引数を追加する．
			// (0.3.0)HyperMutusからコピペ．HyperMutusのISongからGrandMutusのISongに変更．overwriting引数を追加．
			// 11/20/2013 by aldentea : Helper化．
			// 08/19/2013 by aldentea : ★★RealFileNameプロパティの廃止に伴い，FileNameプロパティで代替．
			// 02/29/2012 by aldentea
			#region *[static]曲ファイルをエクスポート(ExportAllSongs)
			/// <summary>
			/// ドキュメントの曲ファイルを指定した場所にコピーします．
			/// </summary>
			/// <param name="destination">コピー先のディレクトリ．</param>
			public static void ExportAllSongs(IEnumerable<ISong> songs, string destination, FileOverwriting overwriting = FileOverwriting.IfNew, bool changeSongFileName = true)
			{
				if (changeSongFileName)
				{
					Helpers.WorkBackground<ISong>(
						songs,
						(song) =>
						{
						// 1.ファイルをコピーする．
						// 2.songのFileNameを変更する．
						song.FileName = CopyFileTo(song.FileName, destination, overwriting);
						}
					);
				}
				else
				{
					Helpers.WorkBackground<ISong>(
						songs,
						(song) =>
						{
							// 1.ファイルをコピーする．
							CopyFileTo(song.FileName, destination, overwriting);
						}
					);

				}
			}
			#endregion

			// (0.3.0)HyperMutusからコピペ．overwrite引数の型をboolからFileOverwritingに変更．
			// 11/20/2013 by aldentea : Helpersのstaticメソッドに変更．
			// 08/19/2013 by aldentea : ★★RealFileNameプロパティの廃止に伴い，FileNameプロパティで代替．
			// 02/29/2012 by aldentea : Songのメソッド．
			#region *ファイルを指定したディレクトリにコピー(CopyFileTo)
			/// <summary>
			/// 曲ファイルを指定したディレクトリにコピーします．
			/// </summary>
			/// <param name="source">コピー元のファイル名です(フルパスを推奨)．</param>
			/// <param name="destination">コピー先のディレクトリ名です(フルパスを推奨)．存在しない場合は新規に作成します．</param>
			/// <param name="overwrite">コピー先に同名の処理があった場合の処理を指定します．</param>
			public static string CopyFileTo(string source, string destination, FileOverwriting overwrite = FileOverwriting.Always)
			{
				if (!Directory.Exists(destination))
				{
					Directory.CreateDirectory(destination);
				}
				var newFileName = Path.Combine(destination, Path.GetFileName(source));
				if (File.Exists(newFileName))
				{
					if (overwrite == FileOverwriting.Always ||
						(overwrite == FileOverwriting.IfNew && File.GetLastWriteTime(newFileName) > File.GetLastWriteTime(source)))
					File.Copy(source, newFileName, true);
				}
				else
				{
					File.Copy(source, newFileName, false);	// 一応，予期しない上書きが行われないようにする．
				}
				return newFileName;
			}

			// (0.3.0)
			#region FileOverwriting列挙体
			public enum FileOverwriting
			{
				/// <summary>
				/// 常に上書きします．
				/// </summary>
				Always,
				/// <summary>
				/// コピー元がコピー先より新しければ上書きします．
				/// </summary>
				IfNew,
				/// <summary>
				/// 上書きしません．
				/// </summary>
				Never
			}
			#endregion

			#endregion


			// (0.3.0)HyperMutusからコピペ．
			// 07/01/2014 by aldentea : デフォルトのフィルタを変更(Oggファイルにも対応)．
			// 11/08/2013 by aldentea
			#region *[static]曲ファイルをダイアログから選択(SelectSongFiles)
			/// <summary>
			/// ファイルダイアログを開いて，曲ファイルを選択します．
			/// キャンセルされた場合はnullを返します．
			/// </summary>
			/// <returns>選択されたファイル名の配列，あるいはnull．</returns>
			public static string[] SelectSongFiles()
			{
				return SelectSongFiles("オーディオファイル(*.mp3;*.rmp;*.ogg;*.oga;*.ogx)", "mp3ファイル(*.mp3)");
			}

			/// <summary>
			/// ファイルダイアログを開いて，曲ファイルを選択します．
			/// キャンセルされた場合はnullを返します．
			/// </summary>
			/// <param name="filters">ダイアログに与えるフィルタを与えます．
			/// 文字列のカッコに挟まれた部分は自動的に展開しますので，"mp3ファイル(*.mp3)"のような文字列を与えればOKです．
			/// "すべてのファイル(*)"は自動的に付加されます．</param>
			/// <returns>選択されたファイル名の配列，あるいはnull．</returns>
			public static string[] SelectSongFiles(params string[] filters)
			{
				string filter_string = string.Empty;

				var filter_pattern = @"\((.+)\)";
				foreach (var filter in filters)
				{
					var m = System.Text.RegularExpressions.Regex.Match(filter, filter_pattern);
					if (m.Success)
					{
						filter_string = filter_string + string.Format("{0}|{1}|", filter, m.Groups[1].Value);
					}
				}

				Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
				dialog.Filter = filter_string + "すべてのファイル|*";
				dialog.Multiselect = true;
				if (dialog.ShowDialog() == true)
				{
					return dialog.FileNames;
				}
				else
				{
					return null;
				}

			}
			#endregion


			// 10/29/2014 by aldentea : 引数にfontSizeを追加。
			// 11/08/2013 by aldentea
			#region *[static]曲ルートを選択(SelectSongsRoot)
			/// <summary>
			/// 曲ルートを選択するためのダイアログを表示し，選択されたディレクトリを返します．
			/// ※これを解除するための手段ってあるのだろうか？
			/// </summary>
			/// <param name="currentSongsRoot">現在の曲ルート．</param>
			/// <returns>選択されたディレクトリの名前，またはnull．nullは「キャンセル」の意味で用いられます．</returns>
			/// <example>
			/// IntroMutusでは以下のように使用しています．
			/// <code>
			/// var newSongsRoot = Helpers.SelectSongsRoot(IntroMutusDocument.SongsRoot);
			/// if (newSongsRoot != null) {
			///		IntroMutusDocument.SongsRoot = newSongsRoot;
			///	}
			///	</code>
			/// </example>
			public static string SelectSongsRoot(string currentSongsRoot)
			{
				FolderBrowserDialog dialog = new FolderBrowserDialog
				{
					Description = "曲ファイルが保存されているディレクトリを選択して下さい．",
					DisplaySpecialFolders = SpecialFoldersFlag.Personal | SpecialFoldersFlag.MyMusic
				};
				return SelectSongsRoot(currentSongsRoot, dialog);
			}

			public static string SelectSongsRoot(string currentSongsRoot, double fontSize)
			{
				FolderBrowserDialog dialog = new FolderBrowserDialog
				{
					Description = "曲ファイルが保存されているディレクトリを選択して下さい．",
					DisplaySpecialFolders = SpecialFoldersFlag.Personal | SpecialFoldersFlag.MyMusic,
					FontSize = fontSize
				};
				return SelectSongsRoot(currentSongsRoot, dialog);
			}

			static string SelectSongsRoot(string currentSongsRoot, FolderBrowserDialog dialog)
			{
				if (!string.IsNullOrEmpty(currentSongsRoot))
				{
					dialog.SelectedPath = currentSongsRoot;
				}
				if (dialog.ShowDialog() == true)
				{
					return dialog.SelectedPath;
				}
				else
				{
					return null;
				}
			}
			#endregion

		}
	}
}
