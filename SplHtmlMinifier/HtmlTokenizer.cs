using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace SplHtmlMinifier
{
	public enum HtmlInlayType
	{
		Comments,
		Doctype,
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
			if (tagName == "!DOCTYPE") {
				tagName = null;
				goto doctypeCur;
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
				if (r.Chr == '>') {
					r.Read();
				}
				goto tagDoneCur;
			}
			if (attrName == null) {
				goto attrNameCur;
			}
			r.Read();
			goto attrPreValCur;
		attrNameCur: ;
			if (!r.IsEof && !r.Chr.IsHtmlWhiteSpace() && r.Chr != '>' && (r.Chr != '/' || sb.Length == 0) && r.Chr != '=') {
				sb.Append(r.Chr);
				r.Read();
				goto attrNameCur;
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
		tagDoneCur: ;
			w.Tag(tagName, attrs.AsReadOnly());
			attrs.Clear();
			var tagNameSaved = tagName;
			tagName = null;
			if (r.IsEof || (true
				&& !tagNameSaved.EqualsIgnoreCase("script") && !tagNameSaved.EqualsIgnoreCase("style")
				&& !tagNameSaved.EqualsIgnoreCase("iframe") && !tagNameSaved.EqualsIgnoreCase("textarea") && !tagNameSaved.EqualsIgnoreCase("title")
				)) {
				goto textCur;
			}
			var fragmBeginIx = r.Ix;
			var m = new Regex(string.Format(@"</{0}\b", tagNameSaved), RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline)
				.Match(r.Text, fragmBeginIx);
			var fragmEndIx = m.Success ? m.Index : r.Text.Length;
			if (fragmEndIx == fragmBeginIx) {
				goto textCur;
			}
			r.SkipTo(fragmEndIx);
			if (tagNameSaved.EqualsIgnoreCase("script") || tagNameSaved.EqualsIgnoreCase("style")) {
				w.Inlay(r.Text.Substring(fragmBeginIx, fragmEndIx - fragmBeginIx), tagNameSaved.EqualsIgnoreCase("script") ? HtmlInlayType.Script : HtmlInlayType.Style);
			}
			else {
				w.Text(r.Text.Substring(fragmBeginIx, fragmEndIx - fragmBeginIx));
			}
			goto textCur;
		commentsCur: ;
			var commentsBeginIx = lastTagIx;
			var commentsEndIx = r.Text.IndexOf("-->", commentsBeginIx + 4, StringComparison.Ordinal);
			commentsEndIx = commentsEndIx >= 0 ? commentsEndIx + 3 : r.Text.Length;
			if (!r.IsEof) {
				r.SkipTo(commentsEndIx);
			}
			w.Inlay(r.Text.Substring(commentsBeginIx, commentsEndIx - commentsBeginIx), HtmlInlayType.Comments);
			goto textCur;
		doctypeCur: ;
			var doctypeBeginIx = lastTagIx;
			m = new Regex(@"<!DOCTYPE\s*(([^\s""'>]+|""[^""]*(""|$)|'[^']*('|$))\s*)*(>|$)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline)
				.Match(r.Text, doctypeBeginIx);
			Trace.Assert(m.Success);
			var doctypeEndIx = m.Index + m.Length;
			if (!r.IsEof) {
				r.SkipTo(doctypeEndIx);
			}
			w.Inlay(r.Text.Substring(doctypeBeginIx, doctypeEndIx - doctypeBeginIx), HtmlInlayType.Doctype);
			goto textCur;
		eofCur: ;
			r = null;
			w.Eof();
		}
	}
}
