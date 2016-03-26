using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus
{
	namespace Data
	{
		// (0.8.2) IsRehearsalプロパティを追加。
		// (0.8.0.2)
		public interface IMutusGameDocument
		{
			LogsCollection Logs { get; }

			/// <summary>
			/// リハーサルモードであるかどうかの値を取得します。
			/// </summary>
			bool IsRehearsal { get; set; }

			// (0.8.1.1)型を変更。
			// (0.8.1)
			/// <summary>
			/// Orderが追加されたときに発生します。
			/// </summary>
			event EventHandler<OrderEventArgs> OrderAdded;

			// (0.8.1.1)型を変更。
			// (0.8.1)
			/// <summary>
			/// Orderの追加がUndoされたときに発生します(他にOrderが削除される場合は想定していません)。
			/// </summary>
			event EventHandler<OrderEventArgs> OrderRemoved;


			// (0.8.1)
			void AddOrder(int? questionID);
			/* 実装はコピペでいいと思います。
			{
				Logs.AddOrder(questionID);
				this.OrderAdded(this, EventArgs.Empty);
			}
			*/


			// (0.8.1)
			void RemoveOrder();
			/* 実装はコピペでいいと思います。
			{
				Logs.RemoveOrder();
				this.OrderRemoved(this, EventArgs.Empty);
			}
			*/
		}

	}
}