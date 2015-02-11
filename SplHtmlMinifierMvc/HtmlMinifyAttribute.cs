using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace SplHtmlMinifier.Mvc
{
	public class HtmlMinifyAttribute : ActionFilterAttribute
	{
		class MinifyFilterStream : Stream
		{
			readonly HttpResponseBase response;
			readonly Stream innerFilter;
			readonly MemoryStream memoryStream = new MemoryStream();
			public MinifyFilterStream(HttpResponseBase response, Stream innerFilter)
			{
				this.response = response;
				this.innerFilter = innerFilter;
			}
			#region Not supported
			public override long Length { get { throw new NotSupportedException(); } }
			public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
			public override int Read(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
			public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
			public override void SetLength(long value) { throw new NotSupportedException(); }
			#endregion
			public override bool CanWrite { get { return true; } }
			public override bool CanSeek { get { return false; } }
			public override bool CanRead { get { return false; } }
			public override void Write(byte[] buffer, int offset, int count)
			{
				memoryStream.Write(buffer, offset, count);
			}
			public override void Flush()
			{
				if (memoryStream.Length == 0) {
					return;
				}
				try {
					if (response.ContentType.Equals("text/html", StringComparison.OrdinalIgnoreCase)) {
						var encoding = response.ContentEncoding;
						var htmlText = encoding.GetString(memoryStream.ToArray());
						memoryStream.SetLength(0);
						htmlText = HtmlMinifier.Minify(htmlText, encoding);
						var bytes = encoding.GetBytes(htmlText);
						innerFilter.Write(bytes, 0, bytes.Length);
					}
					else {
						memoryStream.Position = 0;
						memoryStream.CopyTo(innerFilter);
						memoryStream.SetLength(0);
					}
				}
				finally {
					memoryStream.SetLength(0);
				}
			}
			public override void Close()
			{
				memoryStream.Close();
			}
		}
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var response = filterContext.HttpContext.Response;
			if (response.ContentType.Equals("text/html", StringComparison.OrdinalIgnoreCase)) {
				response.Filter = new MinifyFilterStream(response, response.Filter);
			}
		}
	}
}
