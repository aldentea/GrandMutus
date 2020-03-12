using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace GrandMutus.Base
{
	/// <summary>
	/// ExportDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class ExportDialog : Window
	{
		#region *[dependency]FileFilterプロパティ
		public string FileFilter
		{
			get
			{
				return (string)GetValue(FileFilterProperty);
			}
			set
			{
				SetValue(FileFilterProperty, value);
			}
		}
		public static readonly DependencyProperty FileFilterProperty
			= DependencyProperty.Register("FileFilter", typeof(string), typeof(ExportDialog),
					new PropertyMetadata("すべてのファイル|*"));
		#endregion

		#region *[dependency]Destinationプロパティ
		public string Destination
		{
			get
			{
				return (string)GetValue(DestinationProperty);
			}
			set
			{
				SetValue(DestinationProperty, value);
			}
		}
		public static readonly DependencyProperty DestinationProperty
			= DependencyProperty.Register("Destination", typeof(string), typeof(ExportDialog),
					new PropertyMetadata());
		#endregion

		public string SongDirectory
		{
			get
			{
				return radioButtonYes.IsChecked == true ? textBoxSongDirectory.Text : null;
			}
			set
			{
				if (value == null)
				{
					radioButtonNo.IsChecked = true;
				}
				else
				{
					radioButtonYes.IsChecked = true;
					textBoxSongDirectory.Text = value;
				}
			}
		}

		public ExportDialog()
		{
			InitializeComponent();
		}

		// コマンド
		public static RoutedCommand OKCommand = new RoutedCommand();
		public static RoutedCommand SelectDestinationCommand = new RoutedCommand();

	
		#region SelectDestinationコマンドハンドラ

    private void SelectDestination_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var dialog = new Microsoft.Win32.SaveFileDialog();
			dialog.Filter = this.FileFilter;
			if (dialog.ShowDialog() == true)
			{
				this.Destination = dialog.FileName;
			}
		}

		private void SelectDestination_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
    
    #endregion




		#region OKコマンドハンドラ

    private void OK_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.DialogResult = true;
			this.Close();
		}

		private void OK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = System.IO.Path.IsPathRooted(this.Destination);
		}
    
    #endregion

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			if (sender == radioButtonYes)
			{
				textBoxSongDirectory.IsEnabled = true;
			}
			else if (sender == radioButtonNo)
			{
				textBoxSongDirectory.IsEnabled = false;
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(this.Destination))
			{
				SelectDestinationCommand.Execute(null, this);
			}
		}


	}


}
