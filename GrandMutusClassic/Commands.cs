using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GrandMutus.Classic
{
	// (0.2.6)
	public static class Commands
	{
		public static RoutedCommand AddQuestionsCommand = new RoutedCommand();

		public static RoutedCommand SetSabiPosCommand = new RoutedCommand();

		// (0.3.3)
		public static RoutedCommand SetRootDirectoryCommand = new RoutedCommand();

	}

}
