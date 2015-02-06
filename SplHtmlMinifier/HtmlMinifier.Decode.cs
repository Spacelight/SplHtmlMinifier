using System.Collections.Generic;

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
			string name = attrRaw.Name;
			bool isValid = HtmlAttrs.AttrNames.ContainsKey(name);
			if (isValid && sanitize) {
				name = name.ToLower();
			}
			string val = attrRaw.Val != null ? HtmlCharCoding.Decode(attrRaw.Val) : null;
			return new Attr(isValid, name, val);
		}
		static Tag DecodeTag(string nameRaw, IEnumerable<HtmlTokenizer.Attr> attrsRaw, bool sanitize)
		{
			string name = nameRaw;
			bool isClosing = name.StartsWith("/");
			bool isValid = HtmlTags.TagNameToFlags.ContainsKey(!isClosing ? name : name.Substring(1));
			HtmlTagFlags flags = HtmlTagFlags.None;
			if (isValid) {
				flags = HtmlTags.TagNameToFlags[!isClosing ? name : name.Substring(1)];
				if (flags.NoClosing() && isClosing) {
					isValid = false;
				}
			}
			if (isValid && sanitize) {
				name = name.ToLower();
			}
			var attrs = new List<Attr>();
			foreach (var attrRaw in attrsRaw) {
				var attr = DecodeAttr(attrRaw, isValid && sanitize);
				attrs.Add(attr);
			}
			bool isSelfClosing = false;
			if (!isClosing) {
				isSelfClosing = flags.NoClosing();
				if (attrs.Count != 0 && attrs[attrs.Count - 1].Name == "/" && attrs[attrs.Count - 1].Val == null) {
					isSelfClosing = true;
				}
			}
			return new Tag(isValid, name, flags, attrs.AsReadOnly(), isSelfClosing);
		}
		static Inlay DecodeInlay(string textRaw, HtmlInlayType type)
		{
			return new Inlay(textRaw, type);
		}
	}
}
