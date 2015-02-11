using System.Linq;
using System.Text;

namespace SplHtmlMinifier
{
	public static partial class HtmlMinifier
	{
		static string EncodeText(Text text, Encoding encoding, bool trimWses, out bool leftWsesTrimmed, out bool rightWsesTrimmed)
		{
			return HtmlCharCoding.Encode(text.Val, encoding, '<', trimWses, out leftWsesTrimmed, out rightWsesTrimmed);
		}
		static bool HasHtmlWhiteSpaceOrGt(this string text)
		{
			return text.Any(chr => chr.IsHtmlWhiteSpace() || chr == '>');
		}
		static string EncodeTag(Tag tag, Encoding encoding)
		{
			var sb = new StringBuilder();
			var padAttr = false;
			foreach (var attr in tag.Attrs) {
				if (padAttr) {
					padAttr = false;
					sb.Append(' ');
				}
				if (attr.Val == null) {
					sb.Append(attr.Name);
					padAttr = true;
					continue;
				}
				bool leftWsesTrimmed;
				bool rightWsesTrimmed;
				if (attr.Val != "" && attr.Val[0] != '"' && attr.Val[0] != '\'' && !attr.Val.HasHtmlWhiteSpaceOrGt()) {
					sb.AppendFormat("{0}={1}", attr.Name, HtmlCharCoding.Encode(attr.Val, encoding, '>', false, out leftWsesTrimmed, out rightWsesTrimmed));
					padAttr = true;
					continue;
				}
				sb.AppendFormat("{0}={1}", attr.Name, (new[] {
					string.Format("\"{0}\"", HtmlCharCoding.Encode(attr.Val, encoding, '"', false, out leftWsesTrimmed, out rightWsesTrimmed)),
					attr.Val.IndexOf('"') >= 0 ? string.Format("'{0}'", HtmlCharCoding.Encode(attr.Val, encoding, '\'', false, out leftWsesTrimmed, out rightWsesTrimmed)) : null
					})
					.Where(x => x != null).OrderBy(x => x.Length).First());
				padAttr = false;
			}
			var attrsHtml = sb.ToString();
			return string.Format("<{0}{1}{2}>", tag.Name, attrsHtml != "" && attrsHtml != "/" ? " " : "", attrsHtml);
		}
		static string EncodeInlay(Inlay inlay, Encoding encoding)
		{
			return inlay.Val;
		}
	}
}
