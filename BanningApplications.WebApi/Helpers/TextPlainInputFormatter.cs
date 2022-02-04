using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace BanningApplications.WebApi.Helpers
{
	//see: https://peterdaugaardrasmussen.com/2020/02/29/asp-net-core-how-to-make-a-controller-endpoint-that-accepts-text-plain/
	public class TextPlainInputFormatter : InputFormatter
	{
		private const string ContentType = "text/plain";

		public TextPlainInputFormatter()
		{
			SupportedMediaTypes.Add(ContentType);
		}

		public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
		{
			var request = context.HttpContext.Request;
			using var reader = new StreamReader(request.Body);
			var content = await reader.ReadToEndAsync();
			return await InputFormatterResult.SuccessAsync(content);
		}

		public override bool CanRead(InputFormatterContext context)
		{
			var contentType = context.HttpContext.Request.ContentType;
			return contentType.StartsWith(ContentType);	//note: some include encoding string with the content-type so we go with StartsWith
		}
	}
}
