using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SplHtmlMinifier
{
	[Flags]
	public enum HtmlTagFlags
	{
		None = 0,
		GivesInnerWs = 1 << 0,
		GivesWs = 1 << 1,
		MayNeedOuterWs = 1 << 2,
		NeedsOuterWs = 1 << 3,
		NeedsWs = 1 << 4,
		NoClosing = 1 << 5,
		OptClosing = 1 << 6,
	}
	public static class HtmlTags
	{
		#region Flag testers
		public static bool GivesInnerWs(this HtmlTagFlags flags) { return flags.HasFlag(HtmlTagFlags.GivesInnerWs); }
		public static bool GivesWs(this HtmlTagFlags flags) { return flags.HasFlag(HtmlTagFlags.GivesWs); }
		public static bool MayNeedOuterWs(this HtmlTagFlags flags) { return flags.HasFlag(HtmlTagFlags.MayNeedOuterWs); }
		public static bool NeedsOuterWs(this HtmlTagFlags flags) { return flags.HasFlag(HtmlTagFlags.NeedsOuterWs); }
		public static bool NeedsWs(this HtmlTagFlags flags) { return flags.HasFlag(HtmlTagFlags.NeedsWs); }
		public static bool NoClosing(this HtmlTagFlags flags) { return flags.HasFlag(HtmlTagFlags.NoClosing); }
		public static bool OptClosing(this HtmlTagFlags flags) { return flags.HasFlag(HtmlTagFlags.OptClosing); }
		public static bool IsOrthoWs(this HtmlTagFlags flags)
		{
			return (flags & (HtmlTagFlags.GivesInnerWs | HtmlTagFlags.GivesWs | HtmlTagFlags.MayNeedOuterWs | HtmlTagFlags.NeedsOuterWs | HtmlTagFlags.NeedsWs)) == 0;
		}
		#endregion
		static readonly IReadOnlyDictionary<string, HtmlTagFlags> tagNameToFlags =
			new ReadOnlyDictionary<string, HtmlTagFlags>(new Dictionary<string, HtmlTagFlags>(StringComparer.OrdinalIgnoreCase)
		{
			#region Flags
			// From manual tests
			{ "a", HtmlTagFlags.NeedsWs },
			{ "abbr", HtmlTagFlags.None },
			{ "acronym", HtmlTagFlags.None },
			{ "address", HtmlTagFlags.GivesWs },
			{ "applet", HtmlTagFlags.NeedsOuterWs },
			{ "area", HtmlTagFlags.NoClosing },
			{ "b", HtmlTagFlags.None },
			{ "base", HtmlTagFlags.NoClosing },
			{ "basefont", HtmlTagFlags.NoClosing },
			{ "bdo", HtmlTagFlags.None },
			{ "big", HtmlTagFlags.None },
			{ "blockquote", HtmlTagFlags.GivesWs },
			{ "body", HtmlTagFlags.OptClosing },
			{ "br", HtmlTagFlags.GivesWs | HtmlTagFlags.NoClosing },
			{ "button", HtmlTagFlags.GivesInnerWs | HtmlTagFlags.NeedsOuterWs },
			{ "caption", HtmlTagFlags.GivesInnerWs },
			{ "center", HtmlTagFlags.GivesWs },
			{ "cite", HtmlTagFlags.None },
			{ "code", HtmlTagFlags.None },
			{ "col", HtmlTagFlags.NoClosing },
			{ "colgroup", HtmlTagFlags.OptClosing },
			{ "dd", HtmlTagFlags.GivesWs | HtmlTagFlags.OptClosing },
			{ "del", HtmlTagFlags.None },
			{ "dfn", HtmlTagFlags.None },
			{ "dir", HtmlTagFlags.GivesWs },
			{ "div", HtmlTagFlags.GivesWs },
			{ "dl", HtmlTagFlags.GivesWs },
			{ "dt", HtmlTagFlags.GivesWs | HtmlTagFlags.OptClosing },
			{ "em", HtmlTagFlags.None },
			{ "fieldset", HtmlTagFlags.GivesWs },
			{ "font", HtmlTagFlags.None },
			{ "form", HtmlTagFlags.GivesWs },
			{ "frame", HtmlTagFlags.NoClosing },
			{ "frameset", HtmlTagFlags.None },
			{ "h1", HtmlTagFlags.GivesWs },
			{ "h2", HtmlTagFlags.GivesWs },
			{ "h3", HtmlTagFlags.GivesWs },
			{ "h4", HtmlTagFlags.GivesWs },
			{ "h5", HtmlTagFlags.GivesWs },
			{ "h6", HtmlTagFlags.GivesWs },
			{ "head", HtmlTagFlags.None | HtmlTagFlags.OptClosing },
			{ "hr", HtmlTagFlags.GivesWs | HtmlTagFlags.NoClosing },
			{ "html", HtmlTagFlags.None | HtmlTagFlags.OptClosing },
			{ "i", HtmlTagFlags.None },
			{ "iframe", HtmlTagFlags.NeedsOuterWs },
			{ "img", HtmlTagFlags.NeedsOuterWs | HtmlTagFlags.NoClosing },
			{ "input", HtmlTagFlags.NeedsWs | HtmlTagFlags.NoClosing },
			{ "ins", HtmlTagFlags.None },
			{ "isindex", HtmlTagFlags.NoClosing },
			{ "kbd", HtmlTagFlags.None },
			{ "label", HtmlTagFlags.None },
			{ "legend", HtmlTagFlags.GivesWs },
			{ "li", HtmlTagFlags.GivesWs | HtmlTagFlags.OptClosing },
			{ "link", HtmlTagFlags.NoClosing },
			{ "map", HtmlTagFlags.None },
			{ "menu", HtmlTagFlags.GivesWs },
			{ "meta", HtmlTagFlags.NoClosing },
			{ "noframes", HtmlTagFlags.NeedsOuterWs },
			{ "noscript", HtmlTagFlags.NeedsOuterWs },
			{ "object", HtmlTagFlags.NeedsOuterWs },
			{ "ol", HtmlTagFlags.GivesWs },
			{ "optgroup", HtmlTagFlags.NeedsOuterWs },
			{ "option", HtmlTagFlags.OptClosing },
			{ "p", HtmlTagFlags.GivesWs | HtmlTagFlags.OptClosing },
			{ "param", HtmlTagFlags.NoClosing },
			{ "pre", HtmlTagFlags.GivesWs },
			{ "q", HtmlTagFlags.None },
			{ "s", HtmlTagFlags.None },
			{ "samp", HtmlTagFlags.None },
			{ "script", HtmlTagFlags.NeedsOuterWs },
			{ "select", HtmlTagFlags.NeedsOuterWs },
			{ "small", HtmlTagFlags.None },
			{ "span", HtmlTagFlags.MayNeedOuterWs },
			{ "strike", HtmlTagFlags.None },
			{ "strong", HtmlTagFlags.None },
			{ "style", HtmlTagFlags.NeedsOuterWs },
			{ "sub", HtmlTagFlags.None },
			{ "sup", HtmlTagFlags.None },
			{ "table", HtmlTagFlags.None },
			{ "tbody", HtmlTagFlags.OptClosing },
			{ "td", HtmlTagFlags.GivesInnerWs | HtmlTagFlags.OptClosing },
			{ "textarea", HtmlTagFlags.NeedsOuterWs },
			{ "tfoot", HtmlTagFlags.OptClosing },
			{ "th", HtmlTagFlags.GivesInnerWs | HtmlTagFlags.OptClosing },
			{ "thead", HtmlTagFlags.OptClosing },
			{ "title", HtmlTagFlags.NeedsOuterWs },
			{ "tr", HtmlTagFlags.OptClosing },
			{ "tt", HtmlTagFlags.None },
			{ "u", HtmlTagFlags.NeedsWs },
			{ "ul", HtmlTagFlags.GivesWs },
			{ "var", HtmlTagFlags.None },
			#endregion
		});
		public static IReadOnlyDictionary<string, HtmlTagFlags> TagNameToFlags { get { return tagNameToFlags; } }
	}
}
