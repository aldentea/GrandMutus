﻿using System;
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
		/// <summary>
		/// MainWindow.xaml の相互作用ロジック
		/// </summary>
		public partial class MainWindow : Window
		{
			public MainWindow()
			{
				InitializeComponent();

				this.DataContext = new MutusDocument();
			}

			private void Button_Click(object sender, RoutedEventArgs e)
			{
				((MutusDocument)this.DataContext).SaveAs(@"B:\classic.mtq");
			}

			private void ButtonLoad_Click(object sender, RoutedEventArgs e)
			{
				((MutusDocument)this.DataContext).Open(@"B:\classic.mtq");

			}
		}
	}
}