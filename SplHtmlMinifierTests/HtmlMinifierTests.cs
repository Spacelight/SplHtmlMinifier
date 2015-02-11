using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SplHtmlMinifier.Tests
{
	[TestClass]
	public class HtmlMinifierTests
	{
		[TestMethod]
		public void MinifyBatch()
		{
			var inDirPath = @"..\..\Res\In";
			var outDirPath = @"..\..\Res\Out";
			var sampleDirPath = @"..\..\Res\Sample";
			foreach (var inPath in Directory.GetFiles(inDirPath, "*.htm")) {
				var encoding = Encoding.UTF8;
				var htmlText = File.ReadAllText(inPath, encoding);
				htmlText = HtmlMinifier.Minify(htmlText, encoding);
				File.WriteAllText(Path.Combine(outDirPath, Path.GetFileName(inPath)), htmlText, encoding);
				var samplePath = Path.Combine(sampleDirPath, Path.GetFileName(inPath));
				if (File.Exists(samplePath)) {
					Assert.AreEqual(htmlText, File.ReadAllText(samplePath, encoding));
				}
			}
		}
	}
}
