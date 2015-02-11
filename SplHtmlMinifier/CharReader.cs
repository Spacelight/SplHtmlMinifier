using System.Diagnostics;

namespace SplHtmlMinifier
{
	public class CharReader
	{
		public string Text { get; private set; }
		public int Ix { get; private set; }
		public bool IsEof { get; private set; }
		public char Chr { get; private set; }
		public CharReader(string text)
		{
			Text = text;
			Ix = 0 - 1;
		}
		public void Read()
		{
			Debug.Assert(!IsEof);
			Ix++;
			IsEof = Ix >= Text.Length;
			Chr = !IsEof ? Text[Ix] : '\0';
		}
		public char Peek()
		{
			Debug.Assert(!IsEof);
			return Ix + 1 < Text.Length ? Text[Ix + 1] : '\0';
		}
		public void SkipTo(int ix)
		{
			Debug.Assert(ix >= Ix);
			Ix = ix - 1;
			Read();
		}
	}
}
