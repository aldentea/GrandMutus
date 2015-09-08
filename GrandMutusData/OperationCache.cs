using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aldentea.Wpf.Document;

namespace GrandMutus.Data
{

	// (0.2.0)新規作成．



	// ↓こんなのいるの？むしろsnippetで対応すべき案件ではないのか？
	public abstract class PropertyChangedCache<P> : IOperationCache
	{
		// Songもジェネリックにして，リフレクションを使ってプロパティをsetすることもできるが，
		// そこまでする必要はありますかねぇ？

		//Song _song;
		//string _propertyName;
		protected P _previousValue;
		protected P _currentValue;

		protected PropertyChangedCache(P from, P to)
		{
			//this._song = song;
			//this._propertyName = propertyName;
			this._previousValue = from;
			this._currentValue = to;
		}

		//public abstract void Do();
		public abstract void Reverse();
		public abstract bool CanCancelWith(IOperationCache other);
		//public abstract IOperationCache GetInverse();
	}



}
