﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

namespace GrandMutus.Data
{
	// (0.4.7)QuestionBaseと分離する．
	// (0.4.2)
	public class Question : QuestionBase<QuestionsCollection>
	{
	}

	// (0.4.7.2)やっぱやめた．
	// (0.4.7.1)abstract化．
	public class QuestionBase<T> : INotifyPropertyChanged, INotifyPropertyChanging
		where T : class
	{
		// (0.4.7.2)やっぱやめた．苦肉の策で，setterをpublicにする．
		// (0.4.7.1)abstract化．
		#region *IDプロパティ
		/// <summary>
		/// IDを取得します．
		/// 設定することもできますが，所定の場所以外で行った場合の動作は保証されません．
		/// </summary>
		public int ID
		{
			get { return _id; }
			set { _id = value; }
		}
		int _id = -1;	// (0.1.2)-1は未設定であることを示す．
		#endregion

		// (0.4.5)変更時にNoChangedイベントを発生します．
		#region *Noプロパティ
		/// <summary>
		/// プリセットされた出題順を取得／設定します。
		/// Categoryごとに1からの連番で与えられます。
		/// </summary>
		public int? No
		{
			get { return _no; }
			set
			{
				if (this.No != value)
				{
					if (value.HasValue && value.Value <= 0)
					{
						throw new ArgumentOutOfRangeException("Noは正の値かnullでなければなりません。");
					}
					else
					{
						var previousValue = this.No;
						NotifyPropertyChanging("No");
						this._no = value;
						NotifyPropertyChanged("No");

						this.NoChanged(this, new ValueChangedEventArgs<int?>(previousValue, this.No));

					}
				}
			}
		}
		int? _no = null;
		#endregion

		// (0.4.8)setterにnullが与えられたときにstring.Emptyをsetする．
		// (0.4.5.2)Category変更前に、Noをnullにする。
		#region *Categoryプロパティ
		public string Category
		{
			get
			{
				return _category;
			}
			set
			{
				string new_value = string.IsNullOrEmpty(value) ? string.Empty : value;
				if (this.Category != new_value)
				{

					this.No = null;

					NotifyPropertyChanging("Category");
					this._category = new_value;
					NotifyPropertyChanged("Category");
				}
			}
		}
		string _category = string.Empty;
		#endregion


		// (0.4.5)Noについては処理が複雑なので，専用のイベントを使った方がいいのでは？ということで用意．
		/// <summary>
		/// Noが変更になったときに発生します．
		/// </summary>
		public event EventHandler<ValueChangedEventArgs<int?>> NoChanged = delegate { };


		#region Parentとの関係

		// (0.4.1.1) privateからprotectedに変更．
		#region *Parentプロパティ
		protected T Parent
		{
			get { return _parent; }
			set
			{
				if (this.Parent != value)
				{
					this._parent = value;
					NotifyPropertyChanged("Parent");
					//NotifyPropertyChanged("RelativeFileName");
				}
			}
		}
		T _parent = null;
		#endregion

		// こういうプロパティを用意して，いつ設定するのか？
		// →こういうメソッドを用意してみては？

		/// ↓元のParentに依存した処理を行うならば，OnRemovedFromみたいなメソッドを用意する必要が出てきます．

		/// <summary>
		/// 自身がparentに追加された時に呼び出します．
		/// 除外されたときは引数をnullにして呼び出せばいいのかな？
		/// </summary>
		/// <param name="parent"></param>
		public virtual void OnAddedTo(T parent)
		{
			this.Parent = parent;
		}

		// で，QuestionsCollectionのCollectionChangedから呼び出す，と．

		#endregion

		// (0.3.3)Songからのコピペ。共通実装にしますか？
		#region INotifyPropertyChanged実装

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		protected void NotifyPropertyChanged(string propertyName)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		// (0.3.3)Songからのコピペ。共通実装にしますか？
		#region INotifyPropertyChanging実装
		public event PropertyChangingEventHandler PropertyChanging = delegate { };
		protected void NotifyPropertyChanging(string propertyName)
		{
			this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
		}
		#endregion

	}


}
