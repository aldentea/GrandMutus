using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Linq;

using Aldentea.Wpf.Document;

namespace GrandMutus.Data
{
	public class PlayersCollection : ObservableCollection<Player>
	{

		public PlayersCollection()
		{
			this.CollectionChanged += PlayersCollection_CollectionChanged;
		}

		// (0.9.0)
		public void Initialize()
		{
			// ClearItems()やItems.Clear()とはどう違うのかな？
			this.Clear();
		}

		// (0.9.0)
		public Player FindPlayer(string name)
		{
			return this.SingleOrDefault(player => player.Name == name);
		}

		// (0.9.2)
		public Player Get(int id)
		{
			return this.Single(player => player.ID == id);
		}

		#region コレクション変更関連

		// (0.9.0)
		/// <summary>
		/// 指定した名前のプレイヤーを追加します。
		/// nameが空文字列、あるいは既存のプレイヤー名と重複する場合は、ArgumentExceptionをスローします。
		/// </summary>
		/// <param name="name"></param>
		public void AddPlayer(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("名前を空文字列にすることは認めません。");
			}
			else if (this.Any(player => player.Name == name))
			{
				throw new ArgumentException("プレイヤー名を重複させることは認めません。");
			}
			else
			{
				var player = new Player { Name = name };
				this.Add(player);
			}
		}

		// (0.9.0)
		/// <summary>
		/// 指定した名前のプレイヤーを削除します。
		/// 該当プレイヤーが存在しない場合は、ArgumentExceptionをスローします。
		/// </summary>
		/// <param name="name"></param>
		public void RemovePlayer(string name)
		{
			var player = FindPlayer(name);
			if (player == null)
			{
				throw new ArgumentException($"'{name}'という名前のプレイヤーは存在しません。");
			}
			else
			{
				this.Remove(player);
			}
		}


		void PlayersCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (var item in e.NewItems)
					{
						var player = (Player)item;

						// IDを付与する．
						// (0.1.2)IDが既に設定されているかどうかを確認．
						if (player.ID <= 0) // 無効な値．
						{
							player.ID = GenerateNewID();
						}
						// ☆songのプロパティ変更をここで受け取る？MutusDocumentで行えばここでは不要？
						// ↑とりあえずこのクラスで使っています。
						player.PropertyChanging += Player_PropertyChanging;
						player.PropertyChanged += Player_PropertyChanged;
						player.OnAddedTo(this);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					// →UIから複数曲をまとめて削除すると、既定では1曲ずつこのイベントが呼び出される．それらをまとめてキャッシュする手段がない！？
					// →仕方がないので，UIの処理を修正し(0.3.4.1)，複数曲の削除をまとめて受け取るようにした．

					IList<Player> players = new List<Player>();
					foreach (var player in e.OldItems.Cast<Player>())
					{
						// 削除にあたって、変更通知機能を抑止。
						player.PropertyChanging -= Player_PropertyChanging;
						player.PropertyChanged -= Player_PropertyChanged;
						players.Add(player);
					}

					break;
				case NotifyCollectionChangedAction.Reset:
					// ClearしたときにはResetが発生するのか？←そのようです．
					break;

			}
		}

		#endregion

		#region アイテム変更関連

		/// <summary>
		/// 格納されているアイテムのプロパティ値が変化したときに発生します．
		/// </summary>
		public event EventHandler<ItemEventArgs<IOperationCache>> ItemChanged = delegate { };


		string _nameCache = string.Empty; 
		decimal _maruCache = 0;
		decimal _batsuCache = 0;
		decimal _scoreCache = 0;

		void Player_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
		{
			var player = (Player)sender;
			switch (e.PropertyName)
			{
				case "Name":
					this._nameCache = player.Name;
					break;
				case "Maru":
					this._maruCache = player.Maru;
					break;
				case "Batsu":
					this._batsuCache = player.Batsu;
					break;
				case "Score":
					this._scoreCache = player.Score;
					break;
			}
		}

		void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var player = (Player)sender;

			switch (e.PropertyName)
			{
				case "Name":
					this.ItemChanged(this, new ItemEventArgs<IOperationCache>
					{
						Item = new PlayerNameChangedCache(player, _nameCache, player.Name)
					});
					_nameCache = string.Empty;
					break;
				case "Maru":
					this.ItemChanged(this, new ItemEventArgs<IOperationCache>
					{
						Item = new PlayerMaruChangedCache(player, _maruCache, player.Maru)
					});
					_maruCache = 0;
					break;
				case "Batsu":
					this.ItemChanged(this, new ItemEventArgs<IOperationCache>
					{
						Item = new PlayerBatsuChangedCache(player, _batsuCache, player.Batsu)
					});
					_batsuCache = 0;
					break;
				case "Score":
					this.ItemChanged(this, new ItemEventArgs<IOperationCache>
					{
						Item = new PlayerScoreChangedCache(player, _scoreCache, player.Score)
					});
					_scoreCache = 0;
					break;
			}
			// ドキュメントにNotifyしたい！？
			//e.PropertyName
		}

		#endregion


		#region XML入出力関連

		public const string ELEMENT_NAME = "players";
		const string PATH_ATTRIBUTE = "path";

		// <players>
		//   <player id="2" maru="3" batsu="4" score="-5">
		//     <name>あるでん茶</name>
		//   </player>
		//   <player id="4" maru="1" batsu="0" score="2">
		//     <name>たぬき</name>
		//   </player>
		// </players>


		/// <summary>
		/// 
		/// </summary>
		/// <param name="destinationDir">XMLファイルを出力するディレクトリです．</param>
		/// <returns></returns>
		public XElement GenerateElement()
		{
			XElement element = new XElement(ELEMENT_NAME);

			foreach (Player player in this.Items)
			{
				element.Add(player.GenerateElement());
			}

			return element;
		}

		public void LoadElement(XElement playersElement)
		{
			foreach (var player_element in playersElement.Elements(Player.ELEMENT_NAME))
			{
				this.Add(Player.Generate(player_element));
			}
		}

		#endregion


		#region ID管理関連

		// (0.2.1)無効なIDとして負の値を使うことにしたので，Anyに条件を付加．
		int GenerateNewID()
		{
			int new_id = this.UsedIDList.Any(n => n > 0) ? this.UsedIDList.Max() + 1 : 1;
			// ↑Max()は，要素が空ならInvalidOperationExceptionをスローする．

			//UsedIDList.Add(new_id);
			return new_id;
		}

		IEnumerable<int> UsedIDList
		{
			get
			{
				return Items.Select(player => player.ID);
			}
		}

		#endregion

	}
}
