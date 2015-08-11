using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GrandMutus.Classic
{

	// 1. Appの継承元を変更する(XAMLも忘れずに！)．

	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Aldentea.Wpf.Application.Application
	{
		// (0.3.7) DocumentのクラスをGrandMutusClassicDocumentに変更．
		// (0.3.4) Upgrade処理を追加。
		#region  2. お決まりの設定．(コピペでいいかも．)
		// 06/18/2014 by aldentea 
		protected App()
			: base()
		{
			this.Document = new Data.GrandMutusClassicDocument();
			this.Exit += new ExitEventHandler(App_Exit);

			if (MySettings.RequireUpgrade)
			{
				MySettings.Upgrade();
				MySettings.RequireUpgrade = false;
			}
		}

		void App_Exit(object sender, ExitEventArgs e)
		{
			MySettings.Save();
		}

		// 07/09/2014 by aldentea : Settingsクラスに合わせてinternalに設定．
		// 06/13/2014 by aldentea
		#region *MySettingsプロパティ
		/// <summary>
		/// アプリケーションの設定を取得します．
		/// </summary>
		internal Properties.Settings MySettings
		{
			get
			{
				// 単に"Properties"では通らない．
				return GrandMutus.Classic.Properties.Settings.Default;
			}
		}
		#endregion

		// 06/13/2014 by aldentea : これはその都度実装する必要がありますかねぇ．
		public new static App Current
		{
			get
			{
				return System.Windows.Application.Current as App;
			}
		}

		#endregion

		// ここらへんまでお決まり．

	}
}
