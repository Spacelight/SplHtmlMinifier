using System.Diagnostics;

namespace SplHtmlMinifier
{
	public class CharReader
	{
		readonly string text;
		public string Text { get { return text; } }
		public CharReader(string text)
		{
			this.text = text;
		}
		int ix = 0 - 1;
		bool isEof;
		char chr;
		public int Ix { get { return ix; } }
		public bool IsEof { get { return isEof; } }
		public char Chr { get { return chr; } }
		public void Read()
		{
			Debug.Assert(!isEof);
			ix++;
			isEof = ix >= text.Length;
			chr = !isEof ? text[ix] : '\0';
		}
		public char Peek()
		{
			Debug.Assert(!isEof);
			return ix + 1 < text.Length ? text[ix + 1] : '\0';
		}
		public void SkipTo(int ix)
		{
			Debug.Assert(!isEof);
			Debug.Assert(ix >= this.ix);
			this.ix = ix - 1;
			Read();
		}
	}
}
