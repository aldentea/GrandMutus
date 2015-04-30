using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace GrandMutus.Data
{

	namespace Check
	{


		class Program
		{
			static Properties.Settings MySettings
			{
				get
				{
					return Properties.Settings.Default;
				}
			}

			static void Main(string[] args)
			{
				MutusDocument document = new MutusDocument();

				//document.SongAdded += (s, e) => { Console.WriteLine("{0} 「{1}」が追加されました．", e.Item.ID, e.Item.Title); };

				document.AddSong(System.IO.Path.Combine(MySettings.SongsRoot, "JOY01156.mp3"));
				document.AddSong(System.IO.Path.Combine(MySettings.SongsRoot, "JOY01181.mp3"));
				document.AddSong(System.IO.Path.Combine(MySettings.SongsRoot, "JOY02418.mp3"));

				document.Songs.RootDirectory = MySettings.SongsRoot;

				document.SaveAs(MySettings.FileDestination);

				Console.ReadKey();
			}

		}
	}

}
