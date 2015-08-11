using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Data;

namespace GrandMutus
{
	using Data;

	namespace Classic
	{
		#region IntroQuestionConverterクラス
		public class IntroQuestionConverter : IValueConverter
		{
			// ここでは設定できない．
			//string separator = " / ";

			// parameterにtrueが与えられると，Noも出力します．
			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				if (value is IntroQuestion)
				{
					var introQuestion = (IntroQuestion)value;
					if (parameter != null && ((bool)parameter) == true)
					{
						return string.Format("{1}. {0}", introQuestion.Answer, introQuestion.No.HasValue ? introQuestion.No.ToString() : "--");
					}
					else
					{
						return introQuestion.Answer;
					}
				}
				else
				{
					return "困った問題です";
				}
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}
		#endregion
	}
}
