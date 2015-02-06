using System.Collections.Generic;

namespace SplHtmlMinifier
{
	public static partial class HtmlMinifier
	{
		public class Text
		{
			readonly string val;
			public string Val { get { return val; } }
			public Text(string val)
			{
				this.val = val;
			}
		}
		public class Attr
		{
			readonly bool isValid;
			readonly string name;
			readonly string val;
			public bool IsValid { get { return isValid; } }
			public string Name { get { return name; } }
			public string Val { get { return val; } }
			public Attr(bool isValid, string name, string val)
			{
				this.isValid = isValid;
				this.name = name;
				this.val = val;
			}
		}
		public class Tag
		{
			readonly bool isValid;
			readonly string name;
			readonly HtmlTagFlags flags;
			readonly IList<Attr> attrs;
			readonly bool isSelfClosing;
			public bool IsValid { get { return isValid; } }
			public string Name { get { return name; } }
			public HtmlTagFlags Flags { get { return flags; } }
			public IList<Attr> Attrs { get { return attrs; } }
			public bool IsSelfClosing { get { return isSelfClosing; } }
			public Tag(bool isValid, string name, HtmlTagFlags flags, IList<Attr> attrs, bool isSelfClosing)
			{
				this.isValid = isValid;
				this.name = name;
				this.flags = flags;
				this.attrs = attrs;
				this.isSelfClosing = isSelfClosing;
			}
			bool? isClosing;
			string nameExcSlash;
			public bool IsClosing
			{
				get
				{
					if (isClosing == null) {
						isClosing = name.StartsWith("/");
					}
					return (bool)isClosing;
				}
			}
			public string NameExcSlash
			{
				get
				{
					if (nameExcSlash == null) {
						nameExcSlash = !name.StartsWith("/") ? name : name.Substring(1);
					}
					return nameExcSlash;
				}
			}
		}
		public class Inlay
		{
			readonly string val;
			readonly HtmlInlayType type;
			public string Val { get { return val; } }
			public HtmlInlayType Type { get { return type; } }
			public Inlay(string val, HtmlInlayType type)
			{
				this.val = val;
				this.type = type;
			}
		}
	}
}
