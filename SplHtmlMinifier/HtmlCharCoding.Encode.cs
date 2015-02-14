using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SplHtmlMinifier
{
	public static partial class HtmlCharCoding
	{
		internal static bool IsHtmlWhiteSpace(this char chr)
		{
			// From http://www.w3.org/TR/html401/struct/text.html
			return false
				|| chr == '\u0009'
				|| chr == '\u000A'
				|| chr == '\u000C'
				|| chr == '\u000D'
				|| chr == '\u0020'
				|| chr == '\u200B';
		}
		static bool IsHtmlEscBeginning(this char chr)
		{
			return false
				|| chr == '#'
				|| (chr >= 'A' && chr <= 'Z')
				|| (chr >= 'a' && chr <= 'z');
		}
		static bool IsInEncoding(this char chr, Encoding encoding)
		{
			if (encoding == Encoding.UTF8 && chr < 0xD800) {
				return true;
			}
			return encoding.GetChars(encoding.GetBytes(new[] { chr }))[0] == chr;
		}
		static string HtmlEscAsLit(this char chr)
		{
			string name;
			if (HtmlCharCodes.CharCodeToName.TryGetValue(chr, out name)) {
				return name;
			}
			return null;
		}
		static string HtmlEscAsNum(this char chr)
		{
			return string.Format("#{0}", ((int)chr).ToString("D", CultureInfo.InvariantCulture));
		}
		static string HtmlEscAsHex(this char chr)
		{
			return string.Format("#x{0}", ((int)chr).ToString("X", CultureInfo.InvariantCulture));
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
			var isMetNonWs = false;
			var isWsDeferred = false;
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
				if (!isMetNonWs) {
					isMetNonWs = true;
					leftWsesTrimmed = isWsDeferred;
					isWsDeferred = false;
				}
				if (isWsDeferred) {
					isWsDeferred = false;
					sb.Append(' ');
				}
				var peek = r.Peek();
				if (true
					&& (r.Chr != '&' || !peek.IsHtmlEscBeginning())
					&& (r.Chr != escMode || (escMode == '<' && !peek.IsHtmlTagBeginning()))
					&& (escMode != '>' || !r.Chr.IsHtmlWhiteSpace())
					&& r.Chr.IsInEncoding(encoding)
					) {
					sb.Append(r.Chr);
					continue;
				}
				var chrEscAsLit = r.Chr.HtmlEscAsLit();
				var chrEsc = (new[] {
					chrEscAsLit != null ? chrEscAsLit + (peek.IsHtmlEscLit() ? ";" : "") : null,
					r.Chr.HtmlEscAsNum() + (peek.IsHtmlEscNum() ? ";" : ""),
					r.Chr.HtmlEscAsHex() + (peek.IsHtmlEscHex() ? ";" : "")
					})
					.Where(x => x != null).OrderBy(x => x.Length).First();
				sb.AppendFormat("&{0}", chrEsc);
			}
			return sb.ToString();
		}
	}
}
