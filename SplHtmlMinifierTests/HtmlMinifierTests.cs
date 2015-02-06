using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace SplHtmlMinifier.Tests
{
	[TestClass]
	public class HtmlMinifierTests
	{
		[TestMethod]
		public void MinifyBatch()
		{
			string inDirPath = @"..\..\Res\In";
			string outDirPath = @"..\..\Res\Out";
			string sampleDirPath = @"..\..\Res\Sample";
			foreach (var inPath in Directory.GetFiles(inDirPath, "*.htm")) {
				Encoding encoding = Encoding.UTF8;
				string htmlText = File.ReadAllText(inPath, encoding);
				htmlText = HtmlMinifier.Minify(htmlText, encoding);
				File.WriteAllText(Path.Combine(outDirPath, Path.GetFileName(inPath)), htmlText, encoding);
				string samplePath = Path.Combine(sampleDirPath, Path.GetFileName(inPath));
				if (File.Exists(samplePath)) {
					Assert.AreEqual(htmlText, File.ReadAllText(samplePath, encoding));
				}
			}
		}
	}
}
