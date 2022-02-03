using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace GrandMutus.Net6.Base
{
	// (0.3.4)
	public interface ISongPlayer : INotifyPropertyChanged
	{
		double Volume { get; set; }
		TimeSpan CurrentPosition { get; set; }
		TimeSpan Duration { get; }
		SongPlayerState CurrentState { get; }

		//void Open(string fileName);
		void Close();
		void TogglePlayPause();


	}

	// (0.3.4)名前を変更。SongPlayerから独立。
	// 12/05/2013 by aldentea
	#region SongPlayerState構造体
	public enum SongPlayerState
	{
		/// <summary>
		/// ソースを開いていない状態です．
		/// </summary>
		Inactive,
		/// <summary>
		/// 再生中です．
		/// </summary>
		Playing,
		/// <summary>
		/// 一時停止中です．
		/// </summary>
		Paused
	}
	#endregion

}
