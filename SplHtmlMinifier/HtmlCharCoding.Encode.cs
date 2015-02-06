using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SplHtmlMinifier
{
	public static partial class HtmlCharCoding
	{
		internal static bool IsHtmlWhiteSpace(this char chr)
		{
			// From http://www.w3.org/TR/html401/struct/text.html#whitespace
			return false
				|| chr == '\x0009'
				|| chr == '\x000A'
				|| chr == '\x000C'
				|| chr == '\x000D'
				|| chr == '\x0020'
				|| chr == '\x200B';
		}
		static bool IsInEncoding(this char chr, Encoding encoding)
		{
			if (encoding == Encoding.UTF8 && (int)chr < 0xD800) {
				return true;
			}
			return encoding.GetChars(encoding.GetBytes(new char[] { chr }))[0] == chr;
		}
		static bool IsHtmlEscBeginning(this char chr)
		{
			return false
				|| chr == '#'
				|| (chr >= 'A' && chr <= 'Z')
				|| (chr >= 'a' && chr <= 'z');
		}
		static string HtmlEscAsLit(this char chr)
		{
			if (!HtmlCharCodes.CodeToCharName.ContainsKey((int)chr)) {
				return null;
			}
			return HtmlCharCodes.CodeToCharName[(int)chr];
		}
		static string HtmlEscAsNum(this char chr)
		{
			return string.Format("#{0}", (int)chr);
		}
		static string HtmlEscAsHex(this char chr)
		{
			return string.Format("#x{0:x}", (int)chr);
		}
		public static string Encode(string text, Encoding encoding, char escMode, bool trimWses, out bool leftWsesTrimmed, out bool rightWsesTrimmed)
		{
			Trace.Assert(escMode == '"' || escMode == '\'' || escMode == '<' || escMode == '>');
			Trace.Assert(escMode != '>' || !trimWses);
			var r = new CharReader(text);
			text = null;
			leftWsesTrimmed = false;
			rightWsesTrimmed = false;
			var sb = new StringBuilder();
			bool isLeftmostNonWs = true;
			bool isWsDeferred = false;
			while (true) {
				r.Read();
				if (r.IsEof) {
					rightWsesTrimmed = isWsDeferred;
					isWsDeferred = false;
					break;
				}
				if (trimWses && r.Chr.IsHtmlWhiteSpace()) {
					isWsDeferred = true;
					continue;
				}
				if (isLeftmostNonWs) {
					isLeftmostNonWs = false;
					leftWsesTrimmed = isWsDeferred;
					isWsDeferred = false;
				}
				if (isWsDeferred) {
					isWsDeferred = false;
					sb.Append(' ');
				}
				if (true
					&& r.Chr != '&'
					&& r.Chr != escMode
					&& (escMode != '>' || !r.Chr.IsHtmlWhiteSpace())
					&& r.Chr.IsInEncoding(encoding)
					) {
					sb.Append(r.Chr);
					continue;
				}
				char peek = r.Peek();
				if (false
					|| (r.Chr == '&' && !peek.IsHtmlEscBeginning())
					|| (r.Chr == '<' && !peek.IsHtmlTagBeginning())
					) {
					sb.Append(r.Chr);
					continue;
				}
				string chrEscAsLit = r.Chr.HtmlEscAsLit();
				string chrEsc = (new string[] {
					chrEscAsLit != null ? chrEscAsLit + (peek.IsHtmlEscLit() ? ";" : "") : null,
					r.Chr.HtmlEscAsNum() + (peek.IsHtmlEscNum() ? ";" : ""),
					r.Chr.HtmlEscAsHex() + (peek.IsHtmlEscHex() ? ";" : ""),
					})
					.Where(x => x != null).OrderBy(x => x.Length).First();
				sb.AppendFormat("&{0}", chrEsc);
			}
			return sb.ToString();
		}
	}
}
