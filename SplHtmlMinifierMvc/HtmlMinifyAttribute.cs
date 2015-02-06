using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace SplHtmlMinifier.Mvc
{
	public class HtmlMinifyAttribute : ActionFilterAttribute
	{
		class HtmlMinifyFilterStream : Stream
		{
			HttpResponseBase response;
			Stream innerFilter;
			MemoryStream memoryStream;
			public HtmlMinifyFilterStream(HttpResponseBase response, Stream innerFilter)
			{
				this.response = response;
				this.innerFilter = innerFilter;
				memoryStream = new MemoryStream();
			}
			#region Not implemented
			public override long Length { get { throw new NotImplementedException(); } }
			public override long Position { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
			public override int Read(byte[] buffer, int offset, int count) { throw new NotImplementedException(); }
			public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
			public override void SetLength(long value) { throw new NotImplementedException(); }
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
						string htmlText = response.ContentEncoding.GetString(memoryStream.ToArray());
						memoryStream.SetLength(0);
						htmlText = HtmlMinifier.Minify(htmlText, response.ContentEncoding);
						byte[] bytes = response.ContentEncoding.GetBytes(htmlText);
						innerFilter.Write(bytes, 0, bytes.Length);
					}
					else {
						memoryStream.Position = 0;
						memoryStream.CopyTo(innerFilter);
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
				response.Filter = new HtmlMinifyFilterStream(response, response.Filter);
			}
		}
	}
}
