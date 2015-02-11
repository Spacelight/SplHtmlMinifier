using System;
using System.Collections.Generic;

namespace SplHtmlMinifier
{
	public static partial class HtmlMinifier
	{
		public class Text
		{
			public string Val { get; private set; }
			public Text(string val)
			{
				Val = val;
			}
		}
		public class Attr
		{
			public bool IsValid { get; private set; }
			public string Name { get; private set; }
			public string Val { get; private set; }
			public Attr(bool isValid, string name, string val)
			{
				IsValid = isValid;
				Name = name;
				Val = val;
			}
			public bool IsSlash { get { return Name == "/" && Val == null; } }
		}
		public class Tag
		{
			public bool IsValid { get; private set; }
			public string Name { get; private set; }
			public HtmlTagFlags Flags { get; private set; }
			public IList<Attr> Attrs { get; private set; }
			public Tag(bool isValid, string name, HtmlTagFlags flags, IList<Attr> attrs)
			{
				IsValid = isValid;
				Name = name;
				Flags = flags;
				Attrs = attrs;
			}
			bool? isClosing;
			string nameExcSlash;
			bool? isSelfClosing;
			bool? isEmpty;
			public bool IsClosing
			{
				get
				{
					if (isClosing == null) {
						isClosing = Name.StartsWith("/", StringComparison.Ordinal);
					}
					return (bool)isClosing;
				}
			}
			public string NameExcSlash
			{
				get
				{
					if (nameExcSlash == null) {
						nameExcSlash = !Name.StartsWith("/", StringComparison.Ordinal) ? Name : Name.Substring(1);
					}
					return nameExcSlash;
				}
			}
			public bool IsSelfClosing
			{
				get
				{
					if (isSelfClosing == null) {
						isSelfClosing = !Name.StartsWith("/", StringComparison.Ordinal) && (Flags.NoClosing() || (Attrs.Count != 0 && Attrs[Attrs.Count - 1].IsSlash));
					}
					return (bool)isSelfClosing;
				}
			}
			public bool IsEmpty
			{
				get
				{
					if (isEmpty == null) {
						isEmpty = Attrs.Count == 0 || (Attrs.Count == 1 || Attrs[0].IsSlash);
					}
					return (bool)isEmpty;
				}
			}
		}
		public class Inlay
		{
			public string Val { get; private set; }
			public HtmlInlayType Type { get; private set; }
			public Inlay(string val, HtmlInlayType type)
			{
				Val = val;
				Type = type;
			}
		}
	}
}
