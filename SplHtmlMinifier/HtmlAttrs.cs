﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SplHtmlMinifier
{
	public static class HtmlAttrs
	{
		static readonly IReadOnlyDictionary<string, bool> attrNames =
			new ReadOnlyDictionary<string, bool>(new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
		{
			#region Names
			// From http://www.w3.org/TR/html401/index/attributes.html
			{ "abbr", true },
			{ "accept-charset", true },
			{ "accept", true },
			{ "accesskey", true },
			{ "action", true },
			{ "align", true },
			{ "alink", true },
			{ "alt", true },
			{ "archive", true },
			{ "axis", true },
			{ "background", true },
			{ "bgcolor", true },
			{ "border", true },
			{ "cellpadding", true },
			{ "cellspacing", true },
			{ "char", true },
			{ "charoff", true },
			{ "charset", true },
			{ "checked", true },
			{ "cite", true },
			{ "class", true },
			{ "classid", true },
			{ "clear", true },
			{ "code", true },
			{ "codebase", true },
			{ "codetype", true },
			{ "color", true },
			{ "cols", true },
			{ "colspan", true },
			{ "compact", true },
			{ "content", true },
			{ "coords", true },
			{ "data", true },
			{ "datetime", true },
			{ "declare", true },
			{ "defer", true },
			{ "dir", true },
			{ "disabled", true },
			{ "enctype", true },
			{ "face", true },
			{ "for", true },
			{ "frame", true },
			{ "frameborder", true },
			{ "headers", true },
			{ "height", true },
			{ "href", true },
			{ "hreflang", true },
			{ "hspace", true },
			{ "http-equiv", true },
			{ "id", true },
			{ "ismap", true },
			{ "label", true },
			{ "lang", true },
			{ "language", true },
			{ "link", true },
			{ "longdesc", true },
			{ "marginheight", true },
			{ "marginwidth", true },
			{ "maxlength", true },
			{ "media", true },
			{ "method", true },
			{ "multiple", true },
			{ "name", true },
			{ "nohref", true },
			{ "noresize", true },
			{ "noshade", true },
			{ "nowrap", true },
			{ "object", true },
			{ "onblur", true },
			{ "onchange", true },
			{ "onclick", true },
			{ "ondblclick", true },
			{ "onfocus", true },
			{ "onkeydown", true },
			{ "onkeypress", true },
			{ "onkeyup", true },
			{ "onload", true },
			{ "onmousedown", true },
			{ "onmousemove", true },
			{ "onmouseout", true },
			{ "onmouseover", true },
			{ "onmouseup", true },
			{ "onreset", true },
			{ "onselect", true },
			{ "onsubmit", true },
			{ "onunload", true },
			{ "profile", true },
			{ "prompt", true },
			{ "readonly", true },
			{ "rel", true },
			{ "rev", true },
			{ "rows", true },
			{ "rowspan", true },
			{ "rules", true },
			{ "scheme", true },
			{ "scope", true },
			{ "scrolling", true },
			{ "selected", true },
			{ "shape", true },
			{ "size", true },
			{ "span", true },
			{ "src", true },
			{ "standby", true },
			{ "start", true },
			{ "style", true },
			{ "summary", true },
			{ "tabindex", true },
			{ "target", true },
			{ "text", true },
			{ "title", true },
			{ "type", true },
			{ "usemap", true },
			{ "valign", true },
			{ "value", true },
			{ "valuetype", true },
			{ "version", true },
			{ "vlink", true },
			{ "vspace", true },
			{ "width", true },
			#endregion
		});
		public static IReadOnlyDictionary<string, bool> AttrNames { get { return attrNames; } }
	}
}
