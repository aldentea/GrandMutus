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

namespace GrandMutus
{
	using Data;

	namespace Classic
	{
		// (0.2.0)
		// 3. 継承元を変更する(XAMLも忘れずに！)．

		/// <summary>
		/// MainWindow.xaml の相互作用ロジック
		/// </summary>
		public partial class MainWindow : Aldentea.Wpf.Application.BasicWindow
		{
			public MainWindow()
			{
				InitializeComponent();
			}

			private void Button_Click(object sender, RoutedEventArgs e)
			{
				((MutusDocument)this.DataContext).SaveAs(@"B:\classic.mtq");
			}

			private void ButtonLoad_Click(object sender, RoutedEventArgs e)
			{
				((MutusDocument)this.DataContext).Open(@"B:\classic.mtq");

			}

			// 4. 抽象メンバを実装する．
			// ...ファイル履歴関連のメンバなんですが，
			// これって，Windowに実装するべきなのでしょうか？Appに実装するべきだという気がしているのですが，
			// UIに実装する上で何か不都合があるでしょうか？

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

			// 5. DataContextの設定？...は不要だった．
			public MutusDocument MyDocument
			{
				// AppのDocumentプロパティに設定したオブジェクトが，自動的にDataContextに設定される．
				// NewDocumentプロパティも同じものを指す．
				get { return (MutusDocument)NewDocument; }
			}

		}
	}
}