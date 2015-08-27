using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.ComponentModel;

namespace GrandMutus
{
	using Data;

	namespace Classic
	{
		// (0.3.2) WindowControllerクラスを導入．
		public abstract class WindowController : Aldentea.Wpf.Application.BasicWindow, INotifyPropertyChanged
		{
			// (0.3.2)コンストラクタを追加。
			protected WindowController() : base()
			{
				this.Initialized += Window_Initialized;
				this.Closed += Window_Closed;
			}

			// (0.3.2)現在未使用。
			/// <summary>
			/// InitializeComponentの後に呼び出したい処理を記述して下さい。
			/// 実装するクラスのコンストラクタで、InitializeComponentの後に呼び出して下さい。
			/// </summary>
			//protected virtual void Initialize()
			//{
			//}


			// 5. DataContextの設定？...は不要だった．
			public MutusDocument MyDocument
			{
				// AppのDocumentプロパティに設定したオブジェクトが，自動的にDataContextに設定される．
				// NewDocumentプロパティも同じものを指す．
				get { return (MutusDocument)NewDocument; }
			}




			// MySettingsに以下が設定されているものとする．
			// WriterSettingsIndent(bool)
			// WriterSettingsIndentChars(string，初期値は"ICA="にしておくとよい．)


			// 07/09/2014 by aldentea : Appを用いた実装に変更．これを各windowで実装すればよい？
			#region *MySettingsプロパティ
			internal Properties.Settings MySettings
			{
				get
				{
					return App.Current.MySettings;
					//return Properties.Settings.Default;
				}
			}
			#endregion


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
				MyDocument.WriterSettings.Indent = MySettings.WriterSettingsIndent;
				// Base64でエンコードされているので，それをデコードする．
				MyDocument.WriterSettings.IndentChars = Encoding.ASCII.GetString(System.Convert.FromBase64String(MySettings.WriterSettingsIndentChars));
				MyDocument.WriterSettings.NewLineHandling = MySettings.WriterSettingsNewLineHandling;

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


			// 09/09/2014 by aldentea
			// 保存処理はApp_Exitで行っているし，設定値の取得も，窓の状態関連以外はここでやる必要はないと思う．
			// →と思ったけど，AppからMutusDocumentプロパティを呼び出すのが面倒だね．
			// まあキャストするだけなんだけど，その手間がかかるということはこっちでやるべきだということを示唆しているのかもしれない．
			// ということで，ここで処理することにする．
			private void Window_Closed(object sender, EventArgs e)
			{
				// 設定を保存する準備
				MySettings.WriterSettingsIndent = MyDocument.WriterSettings.Indent;

				// ※この2つはread-onlyならいらないんだけど、まあいいか。
				// IndentCharsはbase64エンコードして保存する．
				MySettings.WriterSettingsIndentChars = System.Convert.ToBase64String(Encoding.ASCII.GetBytes(MyDocument.WriterSettings.IndentChars), System.Base64FormattingOptions.None);
				MySettings.WriterSettingsNewLineHandling = MyDocument.WriterSettings.NewLineHandling;

				MySettings.WindowMaximized = this.WindowState == System.Windows.WindowState.Maximized;
				MySettings.WindowRect = new Rect(new Point(this.Left, this.Top), new Size(this.Width, this.Height));

				//MySettings.FontSize = this.FontSize;

				// 設定を保存
				//MySettings.Save();
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

			// (0.3.2)
			#region INotifyPropertyChanged実装
			public event PropertyChangedEventHandler PropertyChanged = delegate { };

			protected void NotifyPropertyChanged(string propertyName)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}

			#endregion

		}

	}

}
