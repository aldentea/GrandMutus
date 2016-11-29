using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Data
{
	// (0.9.0)
	#region PlayerEventArgsクラス
	public class PlayerEventArgs : EventArgs
	{
		//public int? QuestionID { get; private set; }

		public PlayerEventArgs() : base()
		{
			//this.QuestionID = questionID;
		}
	}
	#endregion

}
