﻿using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace humany_customer_seo_netcore
{
	public interface ISeoService
	{
		Task<SeoPage> Get(string tenant, string widgetUriName, string pathInWidget, string baseUrl);
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

		public Error Error { get; set; }
	}

	public class SeoService : ISeoService
	{
		private readonly IOptions<AppSettings> options;

		public SeoService(IOptions<AppSettings> options)
		{
			this.options = options;
		}
		public async Task<SeoPage> Get(string tenant, string widgetUriName, string pathInWidget, string baseUrl)
		{
			var seoHost = options.Value.SeoBaseUrl;
			using (var client = new HttpClient())
			{
				var ub = new UriBuilder($"{options.Value.SeoBaseUrl}/v1/{tenant}/{widgetUriName}/{pathInWidget}");
				var query = HttpUtility.ParseQueryString(ub.Query);
				query.Add("baseUrl", baseUrl);
				ub.Query = query.ToString();

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
