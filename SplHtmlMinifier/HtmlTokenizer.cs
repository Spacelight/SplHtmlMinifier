using System;
using System.Collections.Generic;
using System.Text;

namespace SplHtmlMinifier
{
	public enum HtmlInlayType
	{
		Comments,
		Script,
		Style
	}
	public static class HtmlTokenizer
	{
		public class Attr
		{
			public string Name { get; private set; }
			public string Val { get; private set; }
			public Attr(string name, string val)
			{
				Name = name;
				Val = val;
			}
		}
		public interface IWriter
		{
			void Text(string text);
			void Tag(string name, IEnumerable<Attr> attrs);
			void Inlay(string text, HtmlInlayType type);
			void Eof();
		}
		internal static bool EqualsIgnoreCase(this string x, string y)
		{
			return x.Equals(y, StringComparison.OrdinalIgnoreCase);
		}
		internal static bool IsHtmlTagBeginning(this char chr)
		{
			// From manual tests
			return false
				|| chr == '!' || chr == '/' || chr == '?'
				|| (chr >= 'A' && chr <= 'Z')
				|| (chr >= 'a' && chr <= 'z');
		}
		public static void Tokenize(string htmlText, IWriter w)
		{
			var r = new CharReader(htmlText);
			htmlText = null;
			var sb = new StringBuilder();
			var lastTagIx = -1;
			string tagName = null;
			string attrName = null;
			var attrs = new List<Attr>();
			char attrValQuotes;
			r.Read();
		textCur: ;
			if (!r.IsEof && (r.Chr != '<' || !r.Peek().IsHtmlTagBeginning())) {
				sb.Append(r.Chr);
				r.Read();
				goto textCur;
			}
			if (sb.Length != 0) {
				var text = sb.ToString();
				sb.Clear();
				w.Text(text);
			}
			if (r.IsEof) {
				goto eofCur;
			}
			lastTagIx = r.Ix;
			r.Read();
		tagNameCur: ;
			if (!r.IsEof && !r.Chr.IsHtmlWhiteSpace() && r.Chr != '>' && (r.Chr != '/' || r.Peek() != '>')) {
				sb.Append(r.Chr);
				r.Read();
				goto tagNameCur;
			}
			tagName = sb.ToString();
			sb.Clear();
			if (tagName.StartsWith("!--", StringComparison.Ordinal)) {
				tagName = null;
				goto commentsCur;
			}
		tagBodyCur: ;
			if (r.Chr.IsHtmlWhiteSpace()) {
				r.Read();
				goto tagBodyCur;
			}
			if (attrName != null && (r.Chr != '=' || attrName == "/")) {
				attrs.Add(new Attr(attrName, null));
				attrName = null;
			}
			if (r.IsEof || r.Chr == '>') {
				w.Tag(tagName, attrs.AsReadOnly());
				attrs.Clear();
				var tagNameSaved = tagName;
				tagName = null;
				if (r.IsEof) {
					goto eofCur;
				}
				r.Read();
				if (!r.IsEof && (tagNameSaved.EqualsIgnoreCase("script") || tagNameSaved.EqualsIgnoreCase("style"))) {
					var inlayBeginIx = r.Ix;
					var inlayEndIx = r.Text.IndexOf(string.Format("</{0}>", tagNameSaved), inlayBeginIx, StringComparison.OrdinalIgnoreCase);
					inlayEndIx = inlayEndIx >= 0 ? inlayEndIx : r.Text.Length;
					r.SkipTo(inlayEndIx);
					w.Inlay(r.Text.Substring(inlayBeginIx, inlayEndIx - inlayBeginIx), tagNameSaved.EqualsIgnoreCase("script") ? HtmlInlayType.Script : HtmlInlayType.Style);
				}
				goto textCur;
			}
			if (attrName == null) {
				goto attrNameCur;
			}
			r.Read();
			goto attrPreValCur;
		attrNameCur: ;
			if (!r.IsEof && !r.Chr.IsHtmlWhiteSpace() && r.Chr != '>' && r.Chr != '=') {
				sb.Append(r.Chr);
				r.Read();
				if (r.Chr != '/' || sb.Length != 1) {
					goto attrNameCur;
				}
			}
			attrName = sb.ToString();
			sb.Clear();
			goto tagBodyCur;
		attrPreValCur: ;
			if (r.Chr.IsHtmlWhiteSpace()) {
				r.Read();
				goto attrPreValCur;
			}
			if (r.Chr == '"' || r.Chr == '\'') {
				attrValQuotes = r.Chr;
				r.Read();
			}
			else {
				attrValQuotes = '\0';
			}
		attrValCur: ;
			if (!r.IsEof && (attrValQuotes != '\0' ? r.Chr != attrValQuotes : !r.Chr.IsHtmlWhiteSpace() && r.Chr != '>')) {
				sb.Append(r.Chr);
				r.Read();
				goto attrValCur;
			}
			if (!r.IsEof && attrValQuotes != '\0') {
				r.Read();
			}
			var attrVal = sb.ToString();
			sb.Clear();
			attrs.Add(new Attr(attrName, attrVal));
			attrName = null;
			goto tagBodyCur;
		commentsCur: ;
			var commentsBeginIx = lastTagIx;
			var commentsEndIx = r.Text.IndexOf("-->", commentsBeginIx + 4, StringComparison.Ordinal);
			commentsEndIx = commentsEndIx >= 0 ? commentsEndIx + 3 : r.Text.Length;
			if (!r.IsEof) {
				r.SkipTo(commentsEndIx);
			}
			w.Inlay(r.Text.Substring(commentsBeginIx, commentsEndIx - commentsBeginIx), HtmlInlayType.Comments);
			goto textCur;
		eofCur: ;
			r = null;
			w.Eof();
		}
	}
}
