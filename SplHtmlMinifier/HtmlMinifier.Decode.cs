using System;
using System.Collections.Generic;
using System.Linq;

namespace SplHtmlMinifier
{
	public static partial class HtmlMinifier
	{
		static Text DecodeText(string textRaw)
		{
			return new Text(HtmlCharCoding.Decode(textRaw));
		}
		static Attr DecodeAttr(HtmlTokenizer.Attr attrRaw, bool sanitize)
		{
			var name = attrRaw.Name;
			var isValid = HtmlAttrs.AttrNames.ContainsKey(name);
			if (isValid && sanitize) {
				name = name.ToLower();
			}
			var val = attrRaw.Val != null ? HtmlCharCoding.Decode(attrRaw.Val) : null;
			return new Attr(isValid, name, val);
		}
		static Tag DecodeTag(string nameRaw, IEnumerable<HtmlTokenizer.Attr> attrsRaw, bool sanitize)
		{
			var name = nameRaw;
			var isClosing = name.StartsWith("/", StringComparison.Ordinal);
			var nameExcSlash = !isClosing ? name : name.Substring(1);
			var isValid = HtmlTags.TagNameToFlags.ContainsKey(nameExcSlash);
			if (isValid && sanitize) {
				name = name.ToLower();
			}
			var flags = isValid ? HtmlTags.TagNameToFlags[nameExcSlash] : HtmlTagFlags.None;
			var attrs = attrsRaw.Select(attrRaw => DecodeAttr(attrRaw, isValid && sanitize)).ToList();
			if (true
				&& isValid && sanitize
				&& !isClosing && flags.NoClosing() && attrs.Count != 0 && attrs[attrs.Count - 1].IsSlash) {
				attrs.RemoveAt(attrs.Count - 1);
			}
			return new Tag(isValid, name, flags, attrs.AsReadOnly());
		}
		static Inlay DecodeInlay(string textRaw, HtmlInlayType type)
		{
			return new Inlay(textRaw, type);
		}
	}
}
