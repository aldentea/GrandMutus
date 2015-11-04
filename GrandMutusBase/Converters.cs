using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Data;

// (0.2.0)GrandMutusBaseに移動．
namespace GrandMutus.Base
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

	// (0.2.1)
	#region NullableIntvalidationRuleクラス
	public class NullableIntValidationRule : System.Windows.Controls.ValidationRule
	{
		public override System.Windows.Controls.ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			if (value is string)
			{
				string v = (string)value;
				int n;
				if (string.IsNullOrEmpty(v) || int.TryParse(v, out n))
				{
					return new System.Windows.Controls.ValidationResult(true, null);
				}
				else
				{
					return new System.Windows.Controls.ValidationResult(false, "整数値(あるいはnull)を入力して下さい．");
				}
			}
			return new System.Windows.Controls.ValidationResult(false, "こんなん文字列に決まってるやろ！");
		}
	}
	#endregion

	// (0.2.2)リバーシブルにしてみる？
	// (*0.3.5)
	#region VisivilityConverterクラス
	public class VisibilityConverter : IValueConverter
	{
		// bool→VisibilityでもVisibility→boolでも同じように使うことができる．

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is bool && targetType == typeof(System.Windows.Visibility))
			{
				return (bool)value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
			}
			else if (value is System.Windows.Visibility && targetType == typeof(bool))
			{
				return ((System.Windows.Visibility)value) == System.Windows.Visibility.Visible;
			}
			else
			{
				// falseと解釈する。
				return System.Windows.Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			//return value is System.Windows.Visibility && ((System.Windows.Visibility)value) == System.Windows.Visibility.Visible;
			return Convert(value, targetType, parameter, culture);
		}
	}
	#endregion

	// (0.3.6)
	#region SabiPosConverterクラス
	public class SabiPosConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is TimeSpan)
			{
				if (parameter is string && !string.IsNullOrEmpty((string)parameter))
				{
					//return ((TimeSpan)value).ToString(@"m\:ss\.ff");
					return ((TimeSpan)value).ToString((string)parameter);
				}
				else
				{
					return ((TimeSpan)value).ToString();
				}
			}
			else
			{
				throw new ArgumentException("TimeSpan型の値を渡して下さい。");
			}

		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is string)
			{
				return TimeSpan.Parse("0:" + (string)value);	// ※Converterのparameterと対応しないなぁ。
			}
			else
			{
				throw new ArgumentException("string型の値を渡して下さい。");
			}
		}
	}
	#endregion

	// (0.2.1)
	#region SabiPosValidationRuleクラス
	public class SabiPosValidationRule : System.Windows.Controls.ValidationRule
	{
		public override System.Windows.Controls.ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			if (value is string)
			{
				TimeSpan result;
				if (TimeSpan.TryParse("0:" + (string)value, out result))
				{
					return new System.Windows.Controls.ValidationResult(true, null);
				}
				else
				{
					return new System.Windows.Controls.ValidationResult(false, "時刻に変換できる文字列('m:ss.xxx'のようなもの)を入れて下さい．");
				}
			}
			return new System.Windows.Controls.ValidationResult(false, "こんなん文字列に決まってるやろ！");
		}
	}
	#endregion

}
