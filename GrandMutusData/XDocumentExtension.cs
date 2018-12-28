using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace GrandMutus.Data
{

	// (1.0.0)async化にともない用意した。
	#region [static]XDocumentExtensionクラス
	public static class XDocumentExtension
	{
		// .Net CoreではこのメソッドがXDocumentに実装されているので、
		// .Net Frameworkでも不要になるかもしれない。
		public static Task<XDocument> LoadAsync(XmlReader reader, LoadOptions loadOptions = LoadOptions.PreserveWhitespace)
		{
			return Task.Run(() =>
		 {
				 return XDocument.Load(reader, loadOptions);
		 });
		}

		public static Task WriteToAsync(this XDocument doc, XmlWriter writer)
		{
			return Task.Run(() =>
			{
				doc.WriteTo(writer);
			});
		}

	}
	#endregion

}
