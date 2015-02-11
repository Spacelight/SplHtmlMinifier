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
		GivesOuterWs = 1 << 1,
		GivesWs = 1 << 2,
		NeedsInnerWs = 1 << 3,
		NeedsOuterWs = 1 << 4,
		NeedsWs = 1 << 5,
		MayNeedWs = 1 << 6,
		NoClosing = 1 << 7,
		OptClosing = 1 << 8
	}
	public static class HtmlTags
	{
		#region Flag testers
		public static bool GivesInnerWs(this HtmlTagFlags flags) { return (flags & HtmlTagFlags.GivesInnerWs) != 0; }
		public static bool GivesOuterWs(this HtmlTagFlags flags) { return (flags & HtmlTagFlags.GivesOuterWs) != 0; }
		public static bool GivesWs(this HtmlTagFlags flags) { return (flags & HtmlTagFlags.GivesWs) != 0; }
		public static bool NeedsInnerWs(this HtmlTagFlags flags) { return (flags & HtmlTagFlags.NeedsInnerWs) != 0; }
		public static bool NeedsOuterWs(this HtmlTagFlags flags) { return (flags & HtmlTagFlags.NeedsOuterWs) != 0; }
		public static bool NeedsWs(this HtmlTagFlags flags) { return (flags & HtmlTagFlags.NeedsWs) != 0; }
		public static bool MayNeedWs(this HtmlTagFlags flags) { return (flags & HtmlTagFlags.MayNeedWs) != 0; }
		public static bool NoClosing(this HtmlTagFlags flags) { return (flags & HtmlTagFlags.NoClosing) != 0; }
		public static bool OptClosing(this HtmlTagFlags flags) { return (flags & HtmlTagFlags.OptClosing) != 0; }
		#endregion
		public static bool IsOrthoWs(this HtmlTagFlags flags)
		{
			return (flags & (0
				| HtmlTagFlags.GivesInnerWs | HtmlTagFlags.GivesOuterWs | HtmlTagFlags.GivesWs
				| HtmlTagFlags.NeedsInnerWs | HtmlTagFlags.NeedsOuterWs | HtmlTagFlags.NeedsWs
				| HtmlTagFlags.MayNeedWs
				)) == 0;
		}
		static readonly IReadOnlyDictionary<string, HtmlTagFlags> tagNameToFlags =
			new ReadOnlyDictionary<string, HtmlTagFlags>(new Dictionary<string, HtmlTagFlags>(StringComparer.OrdinalIgnoreCase)
		{
			#region Flags
			// From manual tests
			{ "a", HtmlTagFlags.NeedsWs },
			{ "abbr", HtmlTagFlags.None },
			{ "acronym", HtmlTagFlags.None },
			{ "address", HtmlTagFlags.GivesWs },
			{ "applet", HtmlTagFlags.NeedsWs },
			{ "area", HtmlTagFlags.NoClosing },
			{ "b", HtmlTagFlags.None },
			{ "base", HtmlTagFlags.NoClosing },
			{ "basefont", HtmlTagFlags.NoClosing },
			{ "bdo", HtmlTagFlags.None },
			{ "big", HtmlTagFlags.None },
			{ "blockquote", HtmlTagFlags.GivesWs },
			{ "body", HtmlTagFlags.NeedsWs | HtmlTagFlags.OptClosing },
			{ "br", HtmlTagFlags.GivesWs | HtmlTagFlags.NoClosing },
			{ "button", HtmlTagFlags.GivesInnerWs | HtmlTagFlags.NeedsOuterWs },
			{ "caption", HtmlTagFlags.GivesWs },
			{ "center", HtmlTagFlags.GivesWs },
			{ "cite", HtmlTagFlags.None },
			{ "code", HtmlTagFlags.None },
			{ "col", HtmlTagFlags.NoClosing },
			{ "colgroup", HtmlTagFlags.OptClosing },
			{ "dd", HtmlTagFlags.GivesWs | HtmlTagFlags.OptClosing },
			{ "del", HtmlTagFlags.NeedsWs },
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
			{ "head", HtmlTagFlags.NeedsWs | HtmlTagFlags.OptClosing },
			{ "hr", HtmlTagFlags.GivesWs | HtmlTagFlags.NoClosing },
			{ "html", HtmlTagFlags.OptClosing },
			{ "i", HtmlTagFlags.None },
			{ "iframe", HtmlTagFlags.NeedsWs },
			{ "img", HtmlTagFlags.NeedsWs | HtmlTagFlags.NoClosing },
			{ "input", HtmlTagFlags.NeedsWs | HtmlTagFlags.NoClosing },
			{ "ins", HtmlTagFlags.NeedsWs },
			{ "isindex", HtmlTagFlags.NoClosing },
			{ "kbd", HtmlTagFlags.None },
			{ "label", HtmlTagFlags.None },
			{ "legend", HtmlTagFlags.GivesWs },
			{ "li", HtmlTagFlags.GivesWs | HtmlTagFlags.OptClosing },
			{ "link", HtmlTagFlags.NoClosing },
			{ "map", HtmlTagFlags.None },
			{ "menu", HtmlTagFlags.GivesWs },
			{ "meta", HtmlTagFlags.NoClosing },
			{ "noframes", HtmlTagFlags.NeedsWs },
			{ "noscript", HtmlTagFlags.NeedsWs },
			{ "object", HtmlTagFlags.NeedsWs },
			{ "ol", HtmlTagFlags.GivesWs },
			{ "optgroup", HtmlTagFlags.NeedsWs },
			{ "option", HtmlTagFlags.GivesWs | HtmlTagFlags.OptClosing },
			{ "p", HtmlTagFlags.GivesWs | HtmlTagFlags.OptClosing },
			{ "param", HtmlTagFlags.NoClosing },
			{ "pre", HtmlTagFlags.GivesOuterWs | HtmlTagFlags.NeedsInnerWs },
			{ "q", HtmlTagFlags.NeedsWs },
			{ "s", HtmlTagFlags.NeedsWs },
			{ "samp", HtmlTagFlags.None },
			{ "script", HtmlTagFlags.NeedsWs },
			{ "select", HtmlTagFlags.NeedsWs },
			{ "small", HtmlTagFlags.None },
			{ "span", HtmlTagFlags.MayNeedWs },
			{ "strike", HtmlTagFlags.NeedsWs },
			{ "strong", HtmlTagFlags.None },
			{ "style", HtmlTagFlags.NeedsWs },
			{ "sub", HtmlTagFlags.None },
			{ "sup", HtmlTagFlags.None },
			{ "table", HtmlTagFlags.None },
			{ "tbody", HtmlTagFlags.OptClosing },
			{ "td", HtmlTagFlags.GivesWs | HtmlTagFlags.OptClosing },
			{ "textarea", HtmlTagFlags.NeedsWs },
			{ "tfoot", HtmlTagFlags.OptClosing },
			{ "th", HtmlTagFlags.GivesWs | HtmlTagFlags.OptClosing },
			{ "thead", HtmlTagFlags.OptClosing },
			{ "title", HtmlTagFlags.NeedsWs },
			{ "tr", HtmlTagFlags.OptClosing },
			{ "tt", HtmlTagFlags.None },
			{ "u", HtmlTagFlags.NeedsWs },
			{ "ul", HtmlTagFlags.GivesWs },
			{ "var", HtmlTagFlags.None }
			#endregion
		});
		public static IReadOnlyDictionary<string, HtmlTagFlags> TagNameToFlags { get { return tagNameToFlags; } }
	}
}
