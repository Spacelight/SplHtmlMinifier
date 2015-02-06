using System.Globalization;
using System.Text;

namespace SplHtmlMinifier
{
	public static partial class HtmlCharCoding
	{
		internal static bool IsHtmlEscLit(this char chr)
		{
			return false
				|| (chr >= 'A' && chr <= 'Z')
				|| (chr >= 'a' && chr <= 'z');
		}
		internal static bool IsHtmlEscNum(this char chr)
		{
			return false
				|| (chr >= '0' && chr <= '9');
		}
		internal static bool IsHtmlEscHex(this char chr)
		{
			return false
				|| (chr >= '0' && chr <= '9')
				|| (chr >= 'A' && chr <= 'F')
				|| (chr >= 'a' && chr <= 'f');
		}
		public static string Decode(string textHtml)
		{
			var r = new CharReader(textHtml);
			textHtml = null;
			var sb = new StringBuilder();
			int escBeginIx = -1;
			int escCode = -1;
			r.Read();
		textCur: ;
			if (!r.IsEof && r.Chr != '&') {
				sb.Append(r.Chr);
				r.Read();
				goto textCur;
			}
			if (r.IsEof) {
				goto eofCur;
			}
			escBeginIx = r.Ix;
			r.Read();
			goto escCur;
		escCur: ;
			if (r.Chr.IsHtmlEscLit()) {
				goto escLitCur;
			}
			if (r.IsEof || r.Chr != '#') {
				goto escDoneCur;
			}
			r.Read();
			if (r.Chr.IsHtmlEscNum()) {
				goto escNumCur;
			}
			if (r.IsEof || (r.Chr != 'X' && r.Chr != 'x')) {
				goto escDoneCur;
			}
			r.Read();
			if (r.Chr.IsHtmlEscHex()) {
				goto escHexCur;
			}
			goto escDoneCur;
		escLitCur: ;
			if (r.Chr.IsHtmlEscLit()) {
				r.Read();
				goto escLitCur;
			}
			string escLit = r.Text.Substring(escBeginIx + 1, r.Ix - (escBeginIx + 1));
			if (!HtmlCharCodes.CharNameToCode.TryGetValue(escLit, out escCode)) {
				escCode = -1;
			}
			goto escDoneCur;
		escNumCur: ;
			if (r.Chr.IsHtmlEscNum()) {
				r.Read();
				goto escNumCur;
			}
			string escNum = r.Text.Substring(escBeginIx + 2, r.Ix - (escBeginIx + 2));
			if (!int.TryParse(escNum, out escCode) || escCode > 0xFFFD) {
				escCode = 0xFFFD;
			}
			goto escDoneCur;
		escHexCur: ;
			if (r.Chr.IsHtmlEscHex()) {
				r.Read();
				goto escHexCur;
			}
			string escHex = r.Text.Substring(escBeginIx + 3, r.Ix - (escBeginIx + 3));
			if (!int.TryParse(escHex, NumberStyles.HexNumber, null, out escCode) || escCode > 0xFFFD) {
				escCode = 0xFFFD;
			}
			goto escDoneCur;
		escDoneCur: ;
			if (escCode >= 0) {
				sb.Append((char)escCode);
				escCode = -1;
				if (r.Chr == ';') {
					r.Read();
				}
				escBeginIx = -1;
			}
			if (escBeginIx >= 0) {
				sb.Append(r.Text, escBeginIx, r.Ix - escBeginIx);
			}
			goto textCur;
		eofCur: ;
			return sb.ToString();
		}
	}
}
