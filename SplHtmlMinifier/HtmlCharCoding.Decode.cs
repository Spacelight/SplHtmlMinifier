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
			var escIx = -1;
			var escCode = -1;
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
			escIx = r.Ix;
			r.Read();
			if (r.Chr.IsHtmlEscLit()) {
				goto escLitCur;
			}
			if (r.Chr != '#') {
				goto escDoneCur;
			}
			r.Read();
			if (r.Chr.IsHtmlEscNum()) {
				goto escNumCur;
			}
			if (r.Chr != 'X' && r.Chr != 'x') {
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
			var escLit = r.Text.Substring(escIx + 1, r.Ix - (escIx + 1));
			if (!HtmlCharCodes.CharNameToCode.TryGetValue(escLit, out escCode)) {
				escCode = -1;
			}
			goto escDoneCur;
		escNumCur: ;
			if (r.Chr.IsHtmlEscNum()) {
				r.Read();
				goto escNumCur;
			}
			var escNum = r.Text.Substring(escIx + 2, r.Ix - (escIx + 2));
			if (!int.TryParse(escNum, NumberStyles.Integer, CultureInfo.InvariantCulture, out escCode) || escCode > 0xFFFD) {
				escCode = 0xFFFD;
			}
			goto escDoneCur;
		escHexCur: ;
			if (r.Chr.IsHtmlEscHex()) {
				r.Read();
				goto escHexCur;
			}
			var escHex = r.Text.Substring(escIx + 3, r.Ix - (escIx + 3));
			if (!int.TryParse(escHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out escCode) || escCode > 0xFFFD) {
				escCode = 0xFFFD;
			}
		escDoneCur: ;
			if (escCode >= 0) {
				sb.Append((char)escCode);
				escCode = -1;
				if (r.Chr == ';') {
					r.Read();
				}
				escIx = -1;
			}
			if (escIx >= 0) {
				sb.Append(r.Text, escIx, r.Ix - escIx);
				escIx = -1;
			}
			goto textCur;
		eofCur: ;
			return sb.ToString();
		}
	}
}
