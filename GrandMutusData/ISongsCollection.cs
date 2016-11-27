using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandMutus.Data
{
	public interface ISongsCollection
	{
		#region *RootDirectoryプロパティ
		/// <summary>
		/// 曲ファイルを格納するディレクトリのフルパスを取得／設定します．
		/// </summary>
		string RootDirectory
		{
			get; set;
		}
		#endregion
	}
}

