using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Xml.Linq;


namespace GrandMutus.Net6.Data
{
	// (0.4.2) 親をQuestionに変更．
	// (0.3.3)
	public class IntroQuestion : Question
	{

		// Songと同様に、インスタンスはDocumentの層で生成する。

		// (0.3.3)
		#region *SongIDプロパティ
		public int SongID
		{
			get
			{
				return _song_id;
			}
		}
		readonly int _song_id;
		#endregion

		// (0.3.3)
		public Song Song
		{
			get
			{
				return Parent.Document.GetSong(this.SongID);
			}
		}

		#region *PlayPosプロパティ
		/// <summary>
		/// 出題時の再生開始位置を取得／設定します．
		/// </summary>
		public TimeSpan PlayPos
		{
			get
			{
				return _playPos;
			}
			set
			{
				if (this.PlayPos != value)
				{
					// 負の値でないことをここでチェックした方がいいかな？

					NotifyPropertyChanging("PlayPos");
					this._playPos = value;
					NotifyPropertyChanged("PlayPos");
				}
			}
		}
		TimeSpan _playPos = TimeSpan.Zero;
		#endregion

		// (0.8.0.1)abstractなプロパティのオーバーライドとする．
		// (0.4.0)
		#region *Answerプロパティ
		/// <summary>
		/// (出題者が識別するための)問題の簡単な説明を取得します。
		/// </summary>
		public override string Answer
		{
			get { return string.Format("{0} / {1}", this.Song.Title, this.Song.Artist); }
		}
		#endregion

		// (0.3.3)
		#region *コンストラクタ(IntroQuestion)
		internal IntroQuestion(int songID)
		{
			this._song_id = songID;
		}
		#endregion



		#region XML入出力関連

		// このあたり本来はXNameなんだけど，手抜きをする．
		public const string ELEMENT_NAME = "intro";
		const string ID_ATTRIBUTE = "id";
		const string NO_ATTRIBUTE = "no";
		const string SONG_ID_ATTRIBUTE = "song_id";
		const string CATEGORY_ATTRIBUTE = "category";
		const string PLAY_POS_ATTRIBUTE = "play_pos";

		// (0.4.5.1)new XAttributeで値がnullの値を出力しないように修正。
		// (0.3.3)
		#region *XML要素を生成(GenerateElement)
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public XElement GenerateElement()
		{
			var element = new XElement(ELEMENT_NAME);
			element.Add(new XAttribute(ID_ATTRIBUTE, this.ID));
			if (this.No.HasValue)
			{
				element.Add(new XAttribute(NO_ATTRIBUTE, this.No));
			}
			if (string.IsNullOrEmpty(this.Category))
			{
				element.Add(new XAttribute(CATEGORY_ATTRIBUTE, this.Category));
			}
			element.Add(new XAttribute(SONG_ID_ATTRIBUTE, this.SongID));
			// (0.3.3)とりあえず従前のように秒数を出力しておく．
			element.Add(new XAttribute(PLAY_POS_ATTRIBUTE, this.PlayPos.TotalSeconds));

			return element;
		}
		#endregion

		// (1.6.0) .Net6対応。
		// (0.6.4.2)CategoryプロパティをNoプロパティより先にsetするように修正．
		// (0.3.3)
		#region *[static]XML要素からオブジェクトを生成(Generate)
		public static IntroQuestion Generate(XElement questionElement)
		{
			int song_id = (int?)questionElement.Attribute(SONG_ID_ATTRIBUTE) ?? -1;
			if (song_id < 1)
			{
				throw new Exception("問題にIDが設定されていない、もしくは不正な値が設定されています。");
			}
			IntroQuestion question = new IntroQuestion(song_id);

			// XMLからインスタンスを生成するならばIDは常にあるのでは？
			// →songをインポートする時とかはそうではないかもしれない？ので一応有無をチェックする．
			// →でも，インポートする時はXML経由ではなくオブジェクト経由で行った方がいいのでは？(ファイル名のパスの扱いとか...)
			var id_attribute = questionElement.Attribute(ID_ATTRIBUTE);
			if (id_attribute != null)
			{
				question.ID = (int)id_attribute;
			}

			question.Category = (string?)questionElement.Attribute(CATEGORY_ATTRIBUTE) ?? String.Empty;
			question.No = (int?)questionElement.Attribute(NO_ATTRIBUTE);
			// Noの後にCategoryをsetすると，Noがnullになってしまうのであった！
			// この挙動はQuestionオブジェクトだけで完結する(はず)なので，NowLoadingかどうかは関係ない．

			var play_pos = (double?)questionElement.Attribute(PLAY_POS_ATTRIBUTE);
			if (play_pos.HasValue)
			{ question.PlayPos = TimeSpan.FromSeconds(play_pos.Value); }

			return question;
		}
		#endregion

		#endregion



	}




}
