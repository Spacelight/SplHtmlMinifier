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
		static bool HasHtmlWhiteSpaceOrGt(this string name)
		{
			foreach (var chr in name) {
				if (chr.IsHtmlWhiteSpace() || chr == '>') {
					return true;
				}
			}
			return false;
		}
		static string EncodeTag(Tag tag, Encoding encoding)
		{
			var sb = new StringBuilder();
			bool padNextAttr = false;
			foreach (var attr in tag.Attrs) {
				if (padNextAttr) {
					padNextAttr = false;
					sb.Append(' ');
				}
				sb.Append(attr.Name);
				padNextAttr = true;
				if (attr.Val == null) {
					continue;
				}
				bool leftWsesTrimmed;
				bool rightWsesTrimmed;
				if (attr.Val != "" && attr.Val[0] != '"' && attr.Val[0] != '\'' && !attr.Val.HasHtmlWhiteSpaceOrGt()) {
					sb.AppendFormat("={0}", HtmlCharCoding.Encode(attr.Val, encoding, '>', false, out leftWsesTrimmed, out rightWsesTrimmed));
					continue;
				}
				string attrValHtml = (new string[] {
					string.Format("\"{0}\"", HtmlCharCoding.Encode(attr.Val, encoding, '"', false, out leftWsesTrimmed, out rightWsesTrimmed)),
					attr.Val.IndexOf('"') >= 0 ? string.Format("'{0}'", HtmlCharCoding.Encode(attr.Val, encoding, '\'', false, out leftWsesTrimmed, out rightWsesTrimmed)) : null,
					})
					.Where(x => x != null).OrderBy(x => x.Length).First();
				sb.AppendFormat("={0}", attrValHtml);
				padNextAttr = false;
			}
			string attrsHtml = sb.ToString();
			sb.Clear();
			return string.Format("<{0}{1}{2}>", tag.Name, attrsHtml != "" && attrsHtml != "/" ? " " : "", attrsHtml);
		}
		static string EncodeInlay(Inlay inlay, Encoding encoding)
		{
			return inlay.Val;
		}
	}
}
