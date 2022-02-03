using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Threading;

namespace GrandMutus.Net6.Base
{
	// 08/02/2015 by aldentea : HyperMutusの同名クラスをコピペ。
	// 12/04/2013 by aldentea : DialogWithSongから，MediaPlayerや現在位置更新用のタイマを分離．

	// (0.3.4)親インターフェイスをISongPlayerに変更。
	#region SongPlayerクラス
	public class SongPlayer : ISongPlayer
	{
		protected MediaPlayer _mPlayer = new MediaPlayer();			// とりあえずprotectedにしておく．
		DispatcherTimer _updatePositionTimer;

		#region イベント

		// 07/23/2012 by aldentea
		/// <summary>
		/// メディアを開くときに発生します．
		/// MediaOpenedまでの間にUIを無効化するのに使いましょう．
		/// </summary>
		//public event EventHandler OpeningMedia = delegate { };

		// 07/23/2012 by aldentea
		/// <summary>
		/// メディアを開いた時に発生します．
		/// 曲操作や曲情報に関するUIの状態変更などに使いましょう．
		/// </summary>
		public event EventHandler MediaOpened = delegate { };

		// 12/04/2013 by aldentea
		/// <summary>
		/// メディアの再生が終了したときに発生します．
		/// </summary>
		public event EventHandler MediaEnded = delegate { };

		#endregion

		// (0.4.0)再生終了時にポインタを先頭に戻さないようにする。
		// (0.1.0)再生終了後にCurrentPositionの通知をするように変更。
		#region *コンストラクタ(SongPlayer)
		public SongPlayer()
		{
			_mPlayer.MediaOpened += (sdr, ea) =>
			{
				MediaOpened(this, EventArgs.Empty);
			};

			// (0.4.0)再生終了時にポインタを先頭に戻さないようにする。
			// 再生終了位置に達したときは，ポインタを先頭に戻して一時停止状態にする．
			_mPlayer.MediaEnded += (sdr, ea) =>
			{
				Pause();
				//CurrentPosition = TimeSpan.Zero;
				MediaEnded(this, EventArgs.Empty);
				this.NotifyPropertyChanged("CurrentPosition");
			};

			_updatePositionTimer = new DispatcherTimer(DispatcherPriority.Normal);
			_updatePositionTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);	// ※これを可変にしたい！
			_updatePositionTimer.Tick += (sdr, ea) => { this.NotifyPropertyChanged("CurrentPosition"); };

		}
		#endregion

		// 12/04/2013 by aldentea : DialogWithSongから移動．名前をCurrentPositionに変更．
		// 10/12/2011 by aldentea : 独立モード化に対応．
		// 09/14/2011 by aldentea
		#region *CurrentPositionプロパティ
		/// <summary>
		/// 現在の曲再生位置を取得／設定します．
		/// </summary>
		public TimeSpan CurrentPosition
		{
			get
			{
				return _mPlayer.Position;
			}
			set
			{
				_mPlayer.Position = value;
				NotifyPropertyChanged("CurrentPosition");
			}
		}
		#endregion

		// (0.3.4)TimeSpan型に変更。nullになる可能性を考えないことにした。
		// 12/04/2013 by aldentea : DialogWithSongから移動．
		#region *Durationプロパティ
		public TimeSpan Duration
		{
			get
			{
				// 基本的に曲ファイルなので，NaturalDurationがTimeSpanでないことは考えにくい。
				// →ので、考えないことにする。
				return _mPlayer.NaturalDuration.TimeSpan;
			}
		}
		#endregion

		// 12/04/2013 by aldentea : DialogWithSongから移動．
		// 08/13/2012 by aldentea
		#region *Volumeプロパティ
		/// <summary>
		/// 音量を取得／設定します(最小0、最大1、線形スケール)。
		/// </summary>
		public double Volume
		{
			get
			{
				return _mPlayer.Volume;
			}
			set
			{
				if (Volume != value)
				{
					_mPlayer.Volume = value;
					NotifyPropertyChanged("Volume");
				}
			}
		}
		#endregion


		#region *MediaSourceプロパティ
		/// <summary>
		/// 再生中のソースのUriを取得します．
		/// ※設定するにはOpenメソッドを使用して下さい．
		/// </summary>
		public Uri MediaSource
		{
			get
			{
				return _mPlayer.Source;
			}
		}
		#endregion

		// 12/05/2013 by aldentea
		#region *IsActiveプロパティ
		public bool IsActive
		{
			get { return CurrentState != SongPlayerState.Inactive; }
		}
		#endregion

		// 12/05/2013 by aldentea
		#region *CurrentStateプロパティ
		public SongPlayerState CurrentState
		{
			get
			{
				if (MediaSource == null)
				{
					return SongPlayerState.Inactive;
				}
				else
				{
					// _mPlayerからは再生中か否かを取得できない？
					return _updatePositionTimer.IsEnabled ? SongPlayerState.Playing : SongPlayerState.Paused;
				}
			}
		}
		#endregion

		// (1.0.1)
		public bool IsRandomRantro
		{
			get => _isRandomRantro;
			set
			{
				if (_isRandomRantro != value)
				{
					_isRandomRantro = value;
					NotifyPropertyChanged("IsRandomRantro");
				}
			}
		}
		bool _isRandomRantro = false;


		/// <summary>
		/// ファイルを開きます．
		/// </summary>
		/// <param name="fileName"></param>
		public void Open(string fileName)
		{
			// ☆MediaPlayer.Openメソッドは，オープンに失敗しても例外を投げない！
			// 所定の時間内にMediaOpenedイベントが発生しなかったらタイムアウト，みたいな実装をする必要がある．
			_mPlayer.Open(new Uri(fileName));
			NotifyPropertyChanged("CurrentState");
			NotifyPropertyChanged("IsActive");
		}

		public void Play()
		{
			_mPlayer.Play();
			_updatePositionTimer.Start();
			NotifyPropertyChanged("CurrentState");
		}

		public void Pause()
		{
			_mPlayer.Pause();
			_updatePositionTimer.Stop();
			NotifyPropertyChanged("CurrentState");
		}

		// 12/05/2013 by aldentea
		#region *再生と一時停止を切替(TogglePlayPause)
		public void TogglePlayPause()
		{
			switch (CurrentState)
			{
				case SongPlayerState.Playing:
					Pause();
					break;
				case SongPlayerState.Paused:
					Play();
					break;
				default:
					// ※とりあえず無処理．例外投げる？
					break;
			}
		}
		#endregion

		// (1.0.3) ランダムラントロモードのチェックを廃止。
		// (1.0.1)
		#region *シークして再生開始(SeekPlay)
		/// <summary>
		/// 曲をシークした後に、再生を開始します。
		/// </summary>
		/// <param name="startPosition"></param>
		public void SeekPlay(TimeSpan playPosition)
		{
			CurrentPosition = playPosition;
			Play();
		}
		#endregion

		public void Close()
		{
			var volumeCache = this.Volume;
			_mPlayer.Close();	// CloseするとVolumeが0.5に戻ってしまう！
			// このときのMediaSource(→CurrentState)はどうなる？
			NotifyPropertyChanged("CurrentState");
			NotifyPropertyChanged("IsActive");
			this.Volume = volumeCache;
		}

		#region INotifyPropertyChanged実装

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		protected void NotifyPropertyChanged(string propertyName)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
	#endregion

}
