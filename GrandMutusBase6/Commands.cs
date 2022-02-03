using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GrandMutus.Net6.Base
{
	// (0.2.6)
	public static class Commands
	{

		// (1.0.2)
		#region よくわからないけど、昔HyperMutusで定義していたコマンドをこっちに集約した。

		// 07/30/2013 by aldentea : IntroMutusで定義したコマンドをここに集約．

		/// <summary>
		/// 出題曲リスト／曲ファイルをエクスポートします．
		/// </summary>
		public static RoutedCommand ExportCommand = new RoutedCommand();
		
		public static RoutedCommand SearchSongCommand = new RoutedCommand();

		public static RoutedCommand EditCommand = new RoutedCommand();

		/// <summary>
		/// 曲を再生します．こういうコマンドは既に定義されていそうですが...
		/// </summary>
		public static RoutedCommand PlayCommand = new RoutedCommand();

		public static RoutedCommand AddQuestionsCommand = new RoutedCommand();

		public static RoutedCommand AddSongsCommand = new RoutedCommand();

		public static RoutedCommand NextSongCommand = new RoutedCommand();


		// 08/21/2013 by aldente
		/// <summary>
		/// 曲のファイル名を変更するダイアログを開きます．
		/// パラメータには変更元のファイル名を渡します．
		/// </summary>
		public static RoutedCommand ChangeSongFileNameCommand = new RoutedCommand();

		/// <summary>
		/// 問題を編集します．というか，Questionみたいな目的語はいるのかな？
		/// </summary>
		public static RoutedCommand EditQuestionCommand = new RoutedCommand();
		public static RoutedCommand PlayQuestionCommand = new RoutedCommand();

		// 08/20/2013 by aldentea
		/// <summary>
		/// 問題にカテゴリを設定します．
		/// </summary>
		public static RoutedCommand SetCategoryCommand = new RoutedCommand();


		public static RoutedCommand AddNormalQuestionCommand = new RoutedCommand();

		// 08/01/2013 by aldentea
		/// <summary>
		/// バージョン情報を表示します．
		/// </summary>
		public static RoutedCommand ShowVersionCommand = new RoutedCommand();



		// 08/02/2013 by aldentea
		// they comes from DialogWithSong class.

		/// <summary>
		/// 設定されたサビ位置に頭出しします．
		/// </summary>
		public static RoutedCommand SeekSabiCommand = new RoutedCommand();

		// 12/04/2013 by aldentea
		/// <summary>
		/// 再生位置を任意の位置に移動します．移動先はパラメータで与えます(実装先に応じた型を適宜使って下さい)．
		/// </summary>
		public static RoutedCommand SeekCommand = new RoutedCommand();

		// 01/09/2014 by aldentea
		/// <summary>
		/// 再生位置を任意の位置に移動します．現在位置の相対的な移動量はパラメータで与えます(実装先に応じた型を適宜使って下さい)．
		/// </summary>
		public static RoutedCommand SeekRelativeCommand = new RoutedCommand();

		/// <summary>
		/// 再生と一時停止を切り替えます．
		/// </summary>
		public static RoutedCommand SwitchPlayPauseCommand = new RoutedCommand();

		/// <summary>
		/// 曲ファイルのルートを設定します．
		/// </summary>
		public static RoutedCommand SetSongsRootCommand = new RoutedCommand();


		// 08/29/2014 by aldentea

		/// <summary>
		/// 前方にスキップします．
		/// </summary>
		public static RoutedCommand SkipForwardCommand = new RoutedCommand();

		/// <summary>
		/// 後方にスキップします．
		/// </summary>
		public static RoutedCommand SkipBackwardCommand = new RoutedCommand();


		// ↓これここか？

		// 11/08/2013 by aldentea

		/// <summary>
		/// アプリケーションを終了します．
		/// </summary>
		public static RoutedCommand EndCommand = new RoutedCommand();


		// 12/05/2013 by aldentea


		// インクリメント量などはハンドラで指定します．
		public static RoutedCommand IncrementCommand = new RoutedCommand();
		public static RoutedCommand DecrementCommand = new RoutedCommand();

		#endregion


		public static RoutedCommand SetSabiPosCommand = new RoutedCommand();

		// (0.3.3)
		public static RoutedCommand SetRootDirectoryCommand = new RoutedCommand();

	}

}
