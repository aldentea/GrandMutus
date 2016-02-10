using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Data
{
	// (0.8.0.2)
	public interface IMutusGameDocument
	{
		LogsCollection Logs { get; }

		//void AddOrder(QuestionBase question);
	}

}
