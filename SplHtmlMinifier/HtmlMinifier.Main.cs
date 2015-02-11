using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SplHtmlMinifier
{
	public static partial class HtmlMinifier
	{
		static bool HasClassOrIdAttr(this IList<Attr> attrs)
		{
			return attrs.Any(attr => attr.IsValid && (attr.Name.EqualsIgnoreCase("class") || attr.Name.EqualsIgnoreCase("id")));
		}
		class HtmlMinifierWorker : HtmlTokenizer.IWriter
		{
			readonly Encoding encoding;
			HtmlMinifierWorker(Encoding encoding)
			{
				this.encoding = encoding;
			}
			string Result { get; set; }
			#region Writer
			enum WsType
			{
				Ortho = 0,
				Needs,
				Trimmed,
				Gives
			}
			StringBuilder wrSb = new StringBuilder();
			WsType wrRightmostWst;
			void WrWrite(string text, WsType leftWst, WsType rightWst)
			{
				if (leftWst == WsType.Ortho && rightWst == WsType.Ortho) {
					wrSb.Append(text);
					return;
				}
				if (false
					|| (wrRightmostWst == WsType.Trimmed && (leftWst == WsType.Needs || leftWst == WsType.Trimmed))
					|| (leftWst == WsType.Trimmed && (wrRightmostWst == WsType.Needs || wrRightmostWst == WsType.Trimmed))
					) {
					wrSb.Append(' ');
				}
				if (true
					&& wrRightmostWst != WsType.Ortho && leftWst != WsType.Ortho
					&& (wrRightmostWst != WsType.Needs || leftWst != WsType.Needs)
					) {
					wrRightmostWst = WsType.Ortho;
				}
				wrSb.Append(text);
				if ((int)rightWst > (int)wrRightmostWst) {
					wrRightmostWst = rightWst;
				}
			}
			void WrEof()
			{
				Result = wrSb.ToString();
				wrSb = null;
			}
			#endregion
			#region Tags stack
			int tsPreDepth;
			int tsScriptDepth;
			int tsStyleDepth;
			int tsTextareaDepth;
			void TsNote(Tag tag)
			{
				if (!tag.IsValid) {
					return;
				}
				if (tag.NameExcSlash.EqualsIgnoreCase("pre")) {
					tsPreDepth = Math.Max(0, tsPreDepth + (!tag.IsClosing ? 1 : -1));
				}
				else if (tag.NameExcSlash.EqualsIgnoreCase("script")) {
					tsScriptDepth = Math.Max(0, tsScriptDepth + (!tag.IsClosing ? 1 : -1));
				}
				else if (tag.NameExcSlash.EqualsIgnoreCase("style")) {
					tsStyleDepth = Math.Max(0, tsStyleDepth + (!tag.IsClosing ? 1 : -1));
				}
				else if (tag.NameExcSlash.EqualsIgnoreCase("textarea")) {
					tsTextareaDepth = Math.Max(0, tsTextareaDepth + (!tag.IsClosing ? 1 : -1));
				}
				Trace.Assert(tsScriptDepth + tsStyleDepth < 2);
			}
			bool TsIsInScriptOrStyle()
			{
				return tsScriptDepth != 0 || tsStyleDepth != 0;
			}
			bool TsIsNoTrimText()
			{
				return tsPreDepth != 0 || tsTextareaDepth != 0;
			}
			#endregion
			#region Reader
			Tag rdDeferredTag;
			void RdFlushDeferredTag()
			{
				if (rdDeferredTag != null) {
					RdWriteTag(rdDeferredTag);
					rdDeferredTag = null;
				}
			}
			void HtmlTokenizer.IWriter.Text(string textRaw)
			{
				Trace.Assert(!TsIsInScriptOrStyle());
				bool leftWsesTrimmed;
				bool rightWsesTrimmed;
				var textHtml = EncodeText(DecodeText(textRaw), encoding, !TsIsNoTrimText(), out leftWsesTrimmed, out rightWsesTrimmed);
				if (textHtml != "") {
					RdFlushDeferredTag();
				}
				var leftWst = leftWsesTrimmed ? WsType.Trimmed : textHtml == "" ? WsType.Ortho : !textHtml[0].IsHtmlWhiteSpace() ? WsType.Needs : WsType.Gives;
				var rightWst = rightWsesTrimmed ? WsType.Trimmed : textHtml == "" ? WsType.Ortho : !textHtml[textHtml.Length - 1].IsHtmlWhiteSpace() ? WsType.Needs : WsType.Gives;
				WrWrite(textHtml, leftWst, rightWst);
			}
			void RdWriteTag(Tag tag)
			{
				WsType leftWst;
				WsType rightWst;
				if (tag.IsValid && (!tag.IsClosing || !tag.Flags.NoClosing())) {
					leftWst = WsType.Ortho;
					rightWst = WsType.Ortho;
					if (!tag.Flags.IsOrthoWs()) {
						var isClosing = tag.IsClosing || tag.Flags.NoClosing();
						var isOpening = !tag.IsClosing;
						if (false
							|| (tag.Flags.GivesInnerWs() && isClosing)
							|| (tag.Flags.GivesOuterWs() && isOpening)
							|| tag.Flags.GivesWs()
							) {
							leftWst = WsType.Gives;
						}
						if (false
							|| (tag.Flags.GivesInnerWs() && isOpening)
							|| (tag.Flags.GivesOuterWs() && isClosing)
							|| tag.Flags.GivesWs()
							) {
							rightWst = WsType.Gives;
						}
						var needsWs = tag.Flags.NeedsWs() || (tag.Flags.MayNeedWs() && tag.Attrs.HasClassOrIdAttr());
						if (false
							|| (tag.Flags.NeedsInnerWs() && isClosing)
							|| (tag.Flags.NeedsOuterWs() && isOpening)
							|| needsWs
							) {
							leftWst = WsType.Needs;
						}
						if (false
							|| (tag.Flags.NeedsInnerWs() && isOpening)
							|| (tag.Flags.NeedsOuterWs() && isClosing)
							|| needsWs
							) {
							rightWst = WsType.Needs;
						}
					}
				}
				else {
					leftWst = WsType.Needs;
					rightWst = WsType.Needs;
				}
				WrWrite(EncodeTag(tag, encoding), leftWst, rightWst);
			}
			void HtmlTokenizer.IWriter.Tag(string nameRaw, IEnumerable<HtmlTokenizer.Attr> attrsRaw)
			{
				Trace.Assert(!TsIsInScriptOrStyle() || nameRaw.EqualsIgnoreCase("/script") || nameRaw.EqualsIgnoreCase("/style"));
				var tag = DecodeTag(nameRaw, attrsRaw, true);
				TsNote(tag);
				if (rdDeferredTag != null && tag.IsValid && !tag.IsClosing && tag.NameExcSlash.EqualsIgnoreCase(rdDeferredTag.NameExcSlash)) {
					rdDeferredTag = null;
				}
				RdFlushDeferredTag();
				if (tag.IsValid && tag.Flags.OptClosing() && tag.IsClosing && tag.IsEmpty) {
					rdDeferredTag = tag;
					return;
				}
				RdWriteTag(tag);
			}
			void HtmlTokenizer.IWriter.Inlay(string textRaw, HtmlInlayType type)
			{
				RdFlushDeferredTag();
				WrWrite(EncodeInlay(DecodeInlay(textRaw, type), encoding), WsType.Ortho, WsType.Ortho);
			}
			void HtmlTokenizer.IWriter.Eof()
			{
				RdFlushDeferredTag();
				WrEof();
			}
			#endregion
			internal static string Run(string htmlText, Encoding encoding)
			{
				var worker = new HtmlMinifierWorker(encoding);
				HtmlTokenizer.Tokenize(htmlText, worker);
				return worker.Result;
			}
		}
		public static string Minify(string htmlText, Encoding encoding)
		{
			return HtmlMinifierWorker.Run(htmlText, encoding);
		}
	}
}
