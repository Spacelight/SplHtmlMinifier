using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SplHtmlMinifier
{
	public static partial class HtmlMinifier
	{
		static bool HasClassOrIdAttr(this IList<Attr> attrs)
		{
			foreach (var attr in attrs) {
				if (attr.IsValid && (false
					|| attr.Name.EqualsIgnoreCase("class")
					|| attr.Name.EqualsIgnoreCase("id")
					)) {
					return true;
				}
			}
			return false;
		}
		class HtmlMinifierWkr : HtmlTokenizer.IWriter
		{
			Encoding encoding;
			HtmlMinifierWkr(Encoding encoding)
			{
				this.encoding = encoding;
			}
			public string Result { get; private set; }
			#region Writer
			enum WsType
			{
				Ortho = 0,
				Needs,
				Trimmed,
				Gives,
			}
			StringBuilder rsltSB = new StringBuilder();
			WsType rsltRightWst;
			void Write(string textHtml, WsType leftWst, WsType rightWst)
			{
				if (leftWst == WsType.Ortho && rightWst == WsType.Ortho) {
					rsltSB.Append(textHtml);
					return;
				}
				if (false
					|| (rsltRightWst == WsType.Trimmed && (leftWst == WsType.Needs || leftWst == WsType.Trimmed))
					|| (leftWst == WsType.Trimmed && (rsltRightWst == WsType.Needs || rsltRightWst == WsType.Trimmed))
					) {
					rsltSB.Append(' ');
				}
				if (rsltRightWst != WsType.Ortho && leftWst != WsType.Ortho && (rsltRightWst != WsType.Needs || leftWst != WsType.Needs)) {
					rsltRightWst = WsType.Ortho;
				}
				rsltSB.Append(textHtml);
				if ((int)rightWst > (int)rsltRightWst) {
					rsltRightWst = rightWst;
				}
			}
			public void Eof()
			{
				Result = rsltSB.ToString();
				rsltSB.Clear();
				rsltSB = null;
			}
			#endregion
			#region Tags stack
			int tagsPreDepth;
			int tagsScriptDepth;
			int tagsStyleDepth;
			int tagsTextareaDepth;
			void TagsNoteTag(Tag tag)
			{
				if (!tag.IsValid || tag.IsSelfClosing) {
					return;
				}
				if (false) { }
				else if (tag.NameExcSlash.EqualsIgnoreCase("pre")) {
					tagsPreDepth = Math.Max(tagsPreDepth + (!tag.IsClosing ? 1 : -1), 0);
				}
				else if (tag.NameExcSlash.EqualsIgnoreCase("script")) {
					tagsScriptDepth = Math.Max(tagsScriptDepth + (!tag.IsClosing ? 1 : -1), 0);
				}
				else if (tag.NameExcSlash.EqualsIgnoreCase("style")) {
					tagsStyleDepth = Math.Max(tagsStyleDepth + (!tag.IsClosing ? 1 : -1), 0);
				}
				else if (tag.NameExcSlash.EqualsIgnoreCase("textarea")) {
					tagsTextareaDepth = Math.Max(tagsTextareaDepth + (!tag.IsClosing ? 1 : -1), 0);
				}
			}
			bool TagsIsInScriptOrStyle()
			{
				return tagsScriptDepth != 0 || tagsStyleDepth != 0;
			}
			bool TagsIsTextNoTrim()
			{
				return tagsPreDepth != 0 || tagsTextareaDepth != 0;
			}
			#endregion
			#region Reader
			public void Text(string textRaw)
			{
				Trace.Assert(!TagsIsInScriptOrStyle());
				bool leftWsesTrimmed;
				bool rightWsesTrimmed;
				string textHtml = EncodeText(DecodeText(textRaw), encoding, !TagsIsTextNoTrim(), out leftWsesTrimmed, out rightWsesTrimmed);
				Write(textHtml,
					leftWsesTrimmed ? WsType.Trimmed : textHtml != "" && textHtml[0].IsHtmlWhiteSpace() ? WsType.Gives : textHtml != "" ? WsType.Needs : WsType.Ortho,
					rightWsesTrimmed ? WsType.Trimmed : textHtml != "" && textHtml[textHtml.Length - 1].IsHtmlWhiteSpace() ? WsType.Gives : textHtml != "" ? WsType.Needs : WsType.Ortho);
			}
			public void Tag(string nameRaw, IEnumerable<HtmlTokenizer.Attr> attrsRaw)
			{
				Trace.Assert(!TagsIsInScriptOrStyle() || nameRaw.EqualsIgnoreCase("/script") || nameRaw.EqualsIgnoreCase("/style"));
				Tag tag = DecodeTag(nameRaw, attrsRaw, true);
				WsType leftWst = WsType.Ortho;
				WsType rightWst = WsType.Ortho;
				if (tag.IsValid) {
					TagsNoteTag(tag);
					if (!tag.Flags.IsOrthoWs()) {
						bool isOpening = !tag.IsClosing;
						bool isClosing = tag.IsClosing || tag.IsSelfClosing;
						if (tag.Flags.GivesInnerWs() && !tag.IsSelfClosing) {
							if (isOpening) { rightWst = WsType.Gives; }
							if (isClosing) { leftWst = WsType.Gives; }
						}
						if (tag.Flags.GivesWs()) {
							leftWst = WsType.Gives;
							rightWst = WsType.Gives;
						}
						if (tag.Flags.NeedsOuterWs() || (tag.Flags.MayNeedOuterWs() && tag.Attrs.HasClassOrIdAttr())) {
							if (isOpening) { leftWst = WsType.Needs; }
							if (isClosing) { rightWst = WsType.Needs; }
						}
						if (tag.Flags.NeedsWs()) {
							leftWst = WsType.Needs;
							rightWst = WsType.Needs;
						}
					}
				}
				Write(EncodeTag(tag, encoding), leftWst, rightWst);
			}
			public void Inlay(string textRaw, HtmlInlayType type)
			{
				Write(EncodeInlay(DecodeInlay(textRaw, type), encoding), WsType.Ortho, WsType.Ortho);
			}
			#endregion
			internal static string Run(string htmlText, Encoding encoding)
			{
				var wkr = new HtmlMinifierWkr(encoding);
				HtmlTokenizer.Tokenize(htmlText, wkr);
				return wkr.Result;
			}
		}
		public static string Minify(string htmlText, Encoding encoding)
		{
			return HtmlMinifierWkr.Run(htmlText, encoding);
		}
		public static string Minify(string htmlText)
		{
			return Minify(htmlText, Encoding.UTF8);
		}
	}
}
