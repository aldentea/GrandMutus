using System.Windows.Input;

namespace GrandMutus.Base
{

	// (0.3.1)HyperMutusからコピペ．
	// 07/24/2012 by aldentea
	#region *[static]QuizCommandsクラス
	public static class QuizCommands
	{

		// 07/24/2012 by aldentea
		#region *[static]Startプロパティ
		/// <summary>
		/// 出題を開始するコマンドを表す値を取得します．
		/// </summary>
		public static RoutedCommand Start
		{
			get
			{
				return startCommand;
			}
		}
		#endregion

		// 07/24/2012 by aldentea
		#region *[static]Endプロパティ
		/// <summary>
		/// (解答を表明する者がおらずに)出題を終了するコマンドを表す値を取得します．
		/// </summary>
		public static RoutedCommand End
		{
			get
			{
				return endCommand;
			}
		}
		#endregion

		// 07/24/2012 by aldentea
		#region *[static]Stopプロパティ
		/// <summary>
		/// (早押しでボタンが押されるなどによって)出題を停止するコマンドを表す値を取得します．
		/// </summary>
		public static RoutedCommand Stop
		{
			get
			{
				return stopCommand;
			}
		}
		#endregion

		// 07/24/2012 by aldentea
		#region *[static]Followプロパティ
		/// <summary>
		/// 出題のフォローを行うコマンドを表す値を取得します．
		/// </summary>
		public static RoutedCommand Follow
		{
			get
			{
				return followCommand;
			}
		}
		#endregion

		// (0.3.4.1)
		#region *[static]NextQuestionプロパティ
		/// <summary>
		/// 次の出題を決定するコマンドを表す値を取得します．
		/// </summary>
		public static RoutedCommand NextQuestion
		{
			get
			{
				return nextQuestionCommand;
			}
		}
		#endregion

		// (0.3.4.1)
		#region *[static]Judgeプロパティ
		/// <summary>
		/// 解答の判定を行うコマンドを表す値を取得します．
		/// </summary>
		public static RoutedCommand Judge
		{
			get
			{
				return judgeCommand;
			}
		}
		#endregion


		static RoutedCommand startCommand = new RoutedCommand();
		static RoutedCommand endCommand = new RoutedCommand();
		static RoutedCommand stopCommand = new RoutedCommand();
		static RoutedCommand followCommand = new RoutedCommand();

		// ※とりあえずここに置いておくが，QuizCommandsに入れた方がいいかな？
		static RoutedCommand nextQuestionCommand = new RoutedCommand();
		static RoutedCommand judgeCommand = new RoutedCommand(); // 判定内容はパラメータで与える．

	}
	#endregion

}
