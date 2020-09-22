using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace humany_customer_seo_netcore
{
	public interface ISeoService
	{
		Task<SeoPage> Get(string tenant, string widgetUriName, string pathInWidget, string baseUrl, TimeSpan? timeout = null);
	}

	public class Error
	{
		public int Code { get; set; }
		public string Message { get; set; } = string.Empty;
	}

	public class SeoPage
	{
		public string Html { get; set; } = string.Empty;

		public Dictionary<string, string> Headers = new Dictionary<string, string>();

		// From DeliveryController.cs:
		public const string CanonicalUrlHeaderName = "HumanyCanonicalUrl";
		public const string ContentRenderedOnHeaderName = "HumanyContentRenderedOn";
		public const string ContentModifiedOnHeaderName = "HumanyContentModifiedOn";
		public const string CssHrefHeaderName = "HumanyCssHref";
		public const string RobotsAllowIndexingHeaderName = "HumanyRobotsAllowIndexing";
		public const string PageTitleHeaderName = "HumanyPageTitle";
		public const string PageDescriptionHeaderName = "HumanyPageDescription";
		public const string GuideModifiedOnHeaderName = "HumanyGuideModifiedOn";

		public string CssHref => Headers.GetValueOrDefault(CssHrefHeaderName, "") ?? "";
		public string PageTitle => Headers.GetValueOrDefault(PageTitleHeaderName, "") ?? "";
		public string CanonicalUrl => Headers.GetValueOrDefault(CanonicalUrlHeaderName, "") ?? "";
		public string ContentRenderedOn => Headers.GetValueOrDefault(ContentRenderedOnHeaderName, "") ?? "";
		public string ContentModifiedOn => Headers.GetValueOrDefault(ContentModifiedOnHeaderName, "") ?? "";
		public bool RobotsAllowIndexing => (Headers.GetValueOrDefault(RobotsAllowIndexingHeaderName, "") ?? "").ToLower() != "false";
		public string PageDescription => Headers.GetValueOrDefault(PageDescriptionHeaderName, "") ?? "";
		public string GuideModifiedOn => Headers.GetValueOrDefault(GuideModifiedOnHeaderName, "") ?? ""; public Error Error { get; set; }
	}

	public class SeoService : ISeoService
	{
		private readonly IOptions<AppSettings> options;

		public SeoService(IOptions<AppSettings> options)
		{
			this.options = options;
		}
		public async Task<SeoPage> Get(string tenant, string widgetUriName, string pathInWidget, string baseUrl, TimeSpan? timeout = null)
		{
			var seoHost = options.Value.SeoBaseUrl;
			using (var client = new HttpClient())
			{
				var ub = new UriBuilder($"{options.Value.SeoBaseUrl}/v2/{tenant}/{widgetUriName}/{pathInWidget}");
				var query = HttpUtility.ParseQueryString(ub.Query);
				query.Add("seoBaseUrl", baseUrl);
				ub.Query = query.ToString();

				if (timeout.HasValue) client.Timeout = timeout.Value;
				HttpResponseMessage response;
				try
				{
					response = await client.GetAsync(ub.Uri);
					if (!response.IsSuccessStatusCode)
					{
						return new SeoPage { Error = new Error { Code = (int)response.StatusCode, Message = response.ReasonPhrase } };
					}
				}
				catch (Exception ex)
				{
					//Not a HttpError, but return it like this for convenience at call site...
					return new SeoPage { Error = new Error { Code = 599, Message = ex.Message } };
				}

				var content = await response.Content.ReadAsStringAsync();
				var headers = response.Headers.Where(k => k.Key.StartsWith("Humany"))
					.ToDictionary(k => k.Key, k => System.Net.WebUtility.UrlDecode(k.Value.First()));

				var result = new SeoPage
				{
					Html = content,
					Headers = headers
				};

				return result;
			}
		}
	}
}
