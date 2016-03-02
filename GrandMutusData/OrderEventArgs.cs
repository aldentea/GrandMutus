using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Data
{
	// (0.3.2)
	#region OrderEventArgsクラス
	public class OrderEventArgs : EventArgs
	{
		public int? QuestionID { get; private set; }

		public OrderEventArgs(int? questionID) : base()
		{
			this.QuestionID = questionID;
		}
	}
	#endregion

}
