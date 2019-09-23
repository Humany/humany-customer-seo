using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace humany_customer_seo_netcore
{
	public interface ISeoService
	{
		Task<SeoPage> Get(string tenant, string widgetUriName, string pathAndQuery);
	}

	public class Error
	{
		public int Code { get; set; }
		public string Message { get; set; } = string.Empty;
	}

	public class SeoPage
	{
		public string Html { get; set; } = string.Empty;
		public string CssHref { get; set; } = string.Empty;
		public string PageTitle { get; set; } = string.Empty;
		public string CanonicalUrl { get; set; } = string.Empty;
		public string ContentRenderedOn { get; set; } = string.Empty;
		public string ContentModifiedOn { get; set; } = string.Empty;

		public Error? Error { get; set; }
	}

	public class SeoService : ISeoService
	{
		private readonly IOptions<AppSettings> options;

		public SeoService(IOptions<AppSettings> options)
		{
			this.options = options;
		}
		public async Task<SeoPage> Get(string tenant, string widgetUriName, string pathAndQuery)
		{
			var seoHost = options.Value.SeoBaseUrl;
			using (var client = new HttpClient())
			{
				var seoUrl = $"{seoHost}/v1/{tenant}/{widgetUriName}/{pathAndQuery}";
				HttpResponseMessage response;
				try
				{
					response = await client.GetAsync(seoUrl);
					if (!response.IsSuccessStatusCode)
					{
						return new SeoPage { Error = new Error { Code = (int)response.StatusCode, Message = response.ReasonPhrase } };
					}
				}
				catch (Exception ex)
				{
					//Not a HttpError, but return it like this for convenience at call site...
					return new SeoPage { Error = new Error { Code = ex.HResult, Message = ex.Message } };
				}

				var content = await response.Content.ReadAsStringAsync();
				var rx = new Regex(@"<!-- BEGIN (?<type>\w+) -->(?<content>.+?)<!-- END \w+ -->", RegexOptions.Singleline);
				var matches = rx.Matches(content).Select(m => new { Id = m.Groups["type"].Value, Value = m.Groups["content"].Value })
					.ToDictionary(o => o.Id, o => o.Value);
				if (!matches.ContainsKey("HTML"))
					return new SeoPage { Error = new Error { Code = 502, Message = "HTML segment missing" } };

				var headers = response.Headers.Where(k => k.Key.StartsWith("Humany"))
					.ToDictionary(k => k.Key, k => System.Net.WebUtility.UrlDecode(k.Value.First()));

				var result = new SeoPage
				{
					Html = matches["HTML"],
					CssHref = headers.GetValueOrDefault("HumanyCssHref", "") ?? "",
					CanonicalUrl = headers.GetValueOrDefault("HumanyCanonicalUrl", "") ?? "",
					ContentModifiedOn = headers.GetValueOrDefault("HumanyContentModifiedOn", "") ?? "",
					ContentRenderedOn = headers.GetValueOrDefault("HumanyContentRenderedOn", "") ?? "",
					PageTitle = headers.GetValueOrDefault("HumanyPageTitle", "") ?? "",
				};

				return result;
			}
		}
	}
}
