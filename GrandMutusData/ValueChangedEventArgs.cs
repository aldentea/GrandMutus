using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Data
{
	// (0.4.4)
	#region ValueChangedEventArgs<T>クラス
	public class ValueChangedEventArgs<T> : EventArgs
	{
		public T PreviousValue { get;  private set; }
		public T CurrentValue { get; private set; }

		public ValueChangedEventArgs(T previousValue, T currentValue) : base()
		{
			this.PreviousValue = previousValue;
			this.CurrentValue = currentValue;
		}
	}
	#endregion

}
