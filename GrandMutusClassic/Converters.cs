﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Data;

namespace GrandMutus.Classic
{

	// (0.3.4.2)QuestionsのNo列にはこれを使わないと、nullを設定することができない。
	#region NullableIntConverterクラス
	public class NullableIntConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value == null ? string.Empty : value.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is string)
			{
				string v = (string)value;
				if (string.IsNullOrEmpty(v))
				{
					return null;
				}
				else
				{
					int n;
					if (int.TryParse(v, out n))
					{
						return n;
					}
				}
			}
			throw new ArgumentException();
		}
	}
	#endregion

	// (0.3.5)
	public class VisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is bool)
			{
				return (bool)value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
			}
			else
			{
				// falseと解釈する。
				return System.Windows.Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value is System.Windows.Visibility && ((System.Windows.Visibility)value) == System.Windows.Visibility.Visible;
		}
	}

}
