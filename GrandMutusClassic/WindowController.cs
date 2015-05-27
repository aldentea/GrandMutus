using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Classic
{
	// (0.3.2) WindowControllerクラスを導入．
	public abstract class WindowController : Aldentea.Wpf.Application.BasicWindow
	{

		/// <summary>
		/// InitializeComponentの後に呼び出したい処理を記述して下さい。
		/// 実装するクラスのコンストラクタで、InitializeComponentの後に呼び出して下さい。
		/// </summary>
		protected virtual void Initialize()
		{
		}

		/*

		// 09/09/2014 by aldentea : MySettingsのUpgradeをAppクラスに移動．
		private void Window_Initialized(object sender, EventArgs e)
		{
			//if (MySettings.RequireUpgrade)
			//{
			//  MySettings.Upgrade();
			//  MySettings.RequireUpgrade = false;
			//}

			// MySettingsに以下が設定されているものとする．
			// WriterSettingsIndent(bool)
			// WriterSettingsIndentChars(string，初期値は"ICA="にしておくとよい．)
			// WriterSettingsNewLineHandling(System.Xml.NewLineHandling)
			// WindowMaximized(bool)
			// WindowRect(System.Windows.Rect; System.Drawing.Rectではないので注意！)


			// WriterSettingsの設定
			MutusDocument.WriterSettings.Indent = MySettings.WriterSettingsIndent;
			// Base64でエンコードされているので，それをデコードする．
			MutusDocument.WriterSettings.IndentChars = Encoding.ASCII.GetString(System.Convert.FromBase64String(MySettings.WriterSettingsIndentChars));
			MutusDocument.WriterSettings.NewLineHandling = MySettings.WriterSettingsNewLineHandling;

			// Windowの位置と大きさを設定．
			if (MySettings.WindowMaximized)
			{
				this.WindowState = System.Windows.WindowState.Maximized;
			}
			if (!MySettings.WindowRect.Size.IsEmpty)
			{
				this.Left = MySettings.WindowRect.Left;
				this.Top = MySettings.WindowRect.Top;
				this.Width = MySettings.WindowRect.Width;
				this.Height = MySettings.WindowRect.Height;
			}

		}

		// MySettingsに以下が設定されているものとする．
		// WriterSettingsIndent(bool)
		// WriterSettingsIndentChars(string，初期値は"ICA="にしておくとよい．)

		 * */

		// 07/09/2014 by aldentea : Appを用いた実装に変更．これを各windowで実装すればよい？
		#region *MySettingsプロパティ
		Properties.Settings MySettings
		{
			get
			{
				return App.Current.MySettings;
				//return Properties.Settings.Default;
			}
		}
		#endregion


		// 4. 抽象メンバを実装する．
		// ...ファイル履歴関連のメンバなんですが，
		// これって，Windowに実装するべきなのでしょうか？Appに実装するべきだという気がしているのですが，
		// UIに実装する上で何か不都合があるでしょうか？

		#region BasicWindow抽象メンバの実装

		protected override System.Collections.Specialized.StringCollection FileHistory
		{
			get
			{
				return MySettings.FileHistory;
			}
			set
			{
				MySettings.FileHistory = value;
			}
		}

		protected override byte FileHistoryCount
		{
			get { return MySettings.FileHistoryCount; }
		}

		protected override byte FileHistoryDisplayCount
		{
			get { return MySettings.FileHistoryDisplayCount; }
		}

		#endregion

	}



}
