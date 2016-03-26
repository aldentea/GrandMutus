using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Data
{
	// とりあえずMutusDocumentクラスとは分けて実装してみる．

	#region MutusGameDocumentクラス
	public class MutusGameDocument : MutusDocument
	{
		// とりあえずpublicにしてみる．
		public LogsCollection Logs { get { return _logs; } }
		readonly LogsCollection _logs;


	}
	#endregion

}
