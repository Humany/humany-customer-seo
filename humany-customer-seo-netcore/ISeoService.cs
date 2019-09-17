using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace humany_customer_seo_netcore
{

	public class PagesResult
	{
		public List<PageResult> Pages { get; set; } = new List<PageResult>();
	}

	public class HttpError
	{
		public int Code { get; set; }
		public string Message { get; set; } = string.Empty;
	}

	public class PageResult
	{
		public string Uri { get; set; } = string.Empty; // original url

		public string CanonicalUrl { get; set; } = string.Empty;

		public List<string> InternalLinks { get; set; } = new List<string>();

		public HttpError? Error { get; set; }

		public string Html { get; set; } = string.Empty;

		public string Css { get; set; } = string.Empty;

		public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
	}

	public interface ISeoService
	{
		Task<PageResult?> Get(string tenant, string widget, string path);
	}

	public class SeoService : ISeoService
	{
		public async Task<PageResult?> Get(string tenant, string widget, string path)
		{
			var seoHost = "https://seo.humany.cc";
			using (var client = new HttpClient())
			{
				var seoUrl = $"{seoHost}/deliver/{tenant}/{widget}/{path}";
				var response = await client.GetAsync(seoUrl);
				if (!response.IsSuccessStatusCode)
				{
					return new PageResult { Error = new HttpError { Code = (int)response.StatusCode, Message = response.ReasonPhrase } };
				}

				var content = await response.Content.ReadAsStringAsync();
				var rx = new Regex(@"<!-- BEGIN (?<type>\w+) -->(?<content>.+?)<!-- END \w+ -->", RegexOptions.Singleline);
				var matches = rx.Matches(content).Select(m => new { Id = m.Groups["type"].Value, Value = m.Groups["content"].Value })
					.ToDictionary(o => o.Id, o => o.Value);
				if (!matches.ContainsKey("HTML"))
					return null;
				return new PageResult { Html = matches["HTML"], Css = matches["CSS"] };
			}
		}
	}
}
