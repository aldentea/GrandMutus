using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Net6.Data
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

	// (0.9.4)
	#region LogEventArgsクラス
	public class LogEventArgs : EventArgs
	{
		public int? QuestionID { get; private set; }
		public int? PlayerID { get; private set; }
		public string Code { get; private set; }
		public decimal Value { get; set; }

		public LogEventArgs(string code, decimal value, int? player_id = null, int? question_id = null) : base()
		{
			this.QuestionID = question_id;
			this.PlayerID = player_id;
			this.Code = code;
			this.Value = value;
		}
	}

	#endregion


}
