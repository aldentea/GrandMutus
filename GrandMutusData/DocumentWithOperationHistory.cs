using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Data
{
	// (0.2.0)新規作成．
	// Aldentea.Wpf.Document名前空間の同名クラスを試しに実装してみる．
	// ↑の2.3.0で反映させる予定．

	public abstract class DocumentWithOperationHistory : Aldentea.Wpf.Document.DocumentBase
	{

		#region *コンストラクタ(DocumentWithOperationHistory)
		public DocumentWithOperationHistory()
			: base()
		{
			this.Opened += new EventHandler(DocumentWithOperationHistory_Opened);

		
		}
		#endregion

		void DocumentWithOperationHistory_Opened(object sender, EventArgs e)
		{
			ClearOperationHistory();
		}

		// 07/13/2014 by aldentea : virtual→abstractとなったのに対応。
		#region *[override]初期化(Initialize)
		protected override void Initialize()
		{
			ClearOperationHistory();
		}
		#endregion

		// 08/22/2013 by aldentea : Documentクラスの仕様変更に伴い，プロパティ名をIsModifiedからIsDirtyに変更．
		#region *[override]IsDirtyプロパティ
		public override bool IsDirty
		{
			get
			{
				return this.OperationCount != 0;
			}
		}
		#endregion

		/// <summary>
		/// このクラスについては，このメソッドは例外を発生させます．
		/// ダーティフラグを立てるためには，操作履歴を操作してください
		/// (AddOperationHistoryメソッドなどを使用)．
		/// </summary>
		public override void SetDirty()
		{
			throw new InvalidOperationException();
		}

		// 1. ドキュメントに対する操作を行うと，その操作がoperationHistoryに記録されます．
		// そのときには，OperationCountが1つ増加し，undoHistoryがクリアされます．
		// 2. アンドゥを指示すると，operationHistoryから操作履歴をpopして，その逆操作を実行します．
		// その操作はundoHistoryにpushされ，またoperationHistoryが1つ減少します．

		// 例1. 保存→操作A→操作B→2回アンドゥ
		// 操作Bの時点ではOperationCount=2，operationHistory=[操作A, 操作B]となっています．
		// ここから2回アンドゥすることができ，その後にはOperationCount=0，すなわち非ダーティ状態となっています．

		// 例2. 保存→操作A→保存→アンドゥ→操作B
		// 操作Aの後に保存することで，OperationCount=0となりますが，operationHistoryには[操作A]が入っています．
		// 操作Aをアンドゥすると，OperationCount=-1となり，undoHistoryに[操作A]が入ります(operationHistoryは空になる)．
		// 次に操作Bを行うと，undoHistoryが空になり，OperationHistoryは(保存するまで)増えることはありません
		// (AddOperationHistoryメソッドの実装を参照)．つまり，非ダーティ状態に戻ることができません．

		// [リドゥ]を絡めてみます．

		// 例3. 保存→操作A→保存→アンドゥ→リドゥ
		// アンドゥ後は，OperationCount=-1となり，undoHistoryに[操作A]が入っている(operationHistoryは空である)のは例2と同じです．
		// 次にリドゥを行うと，操作AがundoHistoryからoperationHistoryに移り，OperationCountは1つ増えて0になります．
		// すなわち非ダーティ状態となります．
		// OperationCountが負の状態から増えるのは，リドゥの場合に限られます．


		#region 操作履歴／ダーティフラグ関連

		protected const string OPERATION_COUNT_PROPERTY = "OperationCount";
		//protected const string IS_MODIFIED_PROPERTY = "IsModified";

		Stack<IOperationCache> operationHistory = new Stack<IOperationCache>();
		Stack<IOperationCache> undoHistory = new Stack<IOperationCache>();

		// 08/16/2013 by aldentea : IsModifiedプロパティの変更通知を追加．
		#region *OperationCountプロパティ
		/// <summary>
		/// 非ダーティな状態から行った操作の数を取得／設定します．
		/// この値が0の場合にのみ，IsModifiedプロパティはfalseを返します．
		/// ※これ，publicじゃないほうがいいのでは？
		/// </summary>
		public int OperationCount
		{
			get
			{
				return operationCount;
			}
			set
			{
				if (operationCount != value)
				{
					operationCount = value;
					NotifyPropertyChanged(OPERATION_COUNT_PROPERTY);	// ※←これいるのか？
					NotifyPropertyChanged(IS_MODIFIED_PROPERTY);
				}
			}
		}

		int operationCount;
		#endregion

		#region *CanUndoプロパティ
		/// <summary>
		/// 現在のドキュメントに対してアンドゥが可能かどうかを示す値を取得します．
		/// </summary>
		public bool CanUndo
		{
			get
			{
				return operationHistory.Count > 0;
			}
		}
		#endregion

		// 10/02/2014 by aldentea
		#region *CanRedoプロパティ
		/// <summary>
		/// 現在のドキュメントに対してやり直し(リドゥ)が可能かどうかを示す値を取得します．
		/// </summary>
		public bool CanRedo
		{
			get
			{
				return undoHistory.Count > 0;
			}
		}
		#endregion


		#region *[override]ダーティフラグを倒す(ClearDirty)
		/// <summary>
		/// ダーティフラグをOFFにします．
		/// </summary>
		public override void ClearDirty()
		{
			OperationCount = 0;
		}
		#endregion

		#region *変更履歴を消去(ClearOperationHistory)
		/// <summary>
		/// 変更履歴を消去します．
		/// </summary>
		public void ClearOperationHistory()
		{
			operationHistory.Clear();
			undoHistory.Clear();
			ClearDirty();
		}
		#endregion

		// ★新たな実装では，アンドゥやリドゥの場合と統合しました．

		// 10/23/2014 by aldentea
		// リドゥ時も(アンドゥ時と同じように)スキップするように変更．
		// リドゥ時は，これらの処理をRedoメソッドで行います．
		// (リドゥの処理は，リドゥ以外にも使われるメソッドで行う場合とリドゥ専用メソッドで行う場合がある．
		// 前者の場合はそのメソッド内でAddOperationHistoryを呼び出すのが必然になるが、後者の場合はその必要はない．
		// 後者の場合にもAddOperationHistoryを呼び出すことを強制するよりは，リドゥの場合にAddOperationHistoryの処理を抑止して
		// Redoメソッドでまとめて行う方が見通しがいいと考えた．)
		#region *操作履歴を追加(AddOperationHistory)
		/// <summary>
		/// 操作履歴を追加します．
		/// </summary>
		/// <param name="item"></param>
		/*
		public void AddOperationHistory(IOperationCache item)
		{
			if (!_undoing && !_redoing)
			{
				operationHistory.Push(item);
				if (OperationCount >= 0)
				{
					OperationCount++;
				}
				undoHistory.Clear();
			}
		}
		 * */

		protected void AddOperationHistory(IOperationCache item)
		{
			if (operationHistory.Count > 0 && operationHistory.Peek().CanCancelWith(item))
			{
				// アンドゥ(相当の)操作を行った場合．

				// itemをpushする代わりに，operationHistoryの先頭(末尾？)の要素とキャンセルする．
				operationHistory.Pop();
				undoHistory.Push(item);

				OperationCount--;
			}
			else if (undoHistory.Count > 0 && undoHistory.Peek().CanCancelWith(item))
			{
				// リドゥ(相当の)操作を行った場合．

				undoHistory.Pop();
				operationHistory.Push(item);

				OperationCount++;
			}
			else
			{
				// それ以外の操作を行った場合．

				operationHistory.Push(item);
				// もはやリドゥはできないので，スタックを空にする．
				undoHistory.Clear();

				if (OperationCount >= 0)
				{
					OperationCount++;
				}
			}
		}
		#endregion


		/// <summary>
		/// アンドゥ中に立てるフラグ．
		/// アンドゥ動作をoperationCacheに入れないようにするため．
		/// </summary>
		//bool _undoing = false;


		// ★新たな実装では，ここではPopせずPeekを使っています．
		// Popの処理は，AddOperationHistoryメソッドで行います．
		#region *元に戻す(Undo)
		public void Undo()
		{
			if (this.CanUndo)
			{
				this.operationHistory.Peek().Reverse();
			}
		}
		#endregion

		// ★新たな実装では，ここではPopせずPeekを使っています．
		// Popの処理は，AddOperationHistoryメソッドで行います．

		// 10/23/2014 by aldentea : _redoingフラグをfalseにしてからOperationHistory周辺の処理をするように変更．
		// 10/20/2014 by aldentea : item.Doの後の2行を追加．(AutoSave～を切る前提なので，これを使わない場合とは相性が悪いかも．)
		// 10/02/2014 by aldentea
		#region *やり直し(Redo)
		/// <summary>
		/// アンドゥした操作を再度実行します．
		/// </summary>
		public void Redo()
		{
			if (this.CanRedo)
			{
				this.undoHistory.Peek().Reverse();
			}
		}

		//bool _redoing = false;
		#endregion

		#endregion

	}


	#region IOperationCacheインターフェイス
	public interface IOperationCache
	{
		/// <summary>
		/// 正方向実行．
		/// </summary>
		//void Do();

		// ↑いらなくなるかも！

		/// <summary>
		/// 逆方向実行．
		/// </summary>
		void Reverse();

		/// <summary>
		/// 自身とotherが相殺可能かどうかを表す値を返します．
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		bool CanCancelWith(IOperationCache other);

		// ↓これ使うの？

		/// <summary>
		/// 自身と対になるOperationCacheを返します．
		/// </summary>
		/// <returns></returns>
		//IOperationCache GetInverse();

	}
	#endregion

}
