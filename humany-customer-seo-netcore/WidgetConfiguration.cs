using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace humany_customer_seo_netcore
{
	public class WidgetConfiguration
	{
		public string GetHumanyTenantDomain(string topDomain) => $"{Tenant}.humany.{topDomain}";
		public string Tenant { get; set; } = string.Empty;
		public string WidgetUriName { get; set; } = string.Empty;
		public string Implementation { get; set; } = string.Empty;
		public string BasePath { get; set; } = string.Empty;

		public WidgetConfiguration()
		{
		}

		public WidgetConfiguration(string tenant, string widgetUriName, string implementation, string basePath = "/")
		{
			Tenant = tenant;
			WidgetUriName = widgetUriName;
			Implementation = implementation;
			BasePath = basePath;
		}

		public bool IsValid()
		{
			return !string.IsNullOrWhiteSpace(Tenant) && !string.IsNullOrWhiteSpace(WidgetUriName) && !string.IsNullOrWhiteSpace(Implementation);
		}
		public static string GetPathInWidget(string pageName, string requestPath)
		{
			var pathInWidgetMatch = new System.Text.RegularExpressions.Regex(@$"^/{pageName.Trim('/')}/?(.*)").Match(requestPath);
			return pathInWidgetMatch.Success ? pathInWidgetMatch.Groups[1].Value : "";
		}
	}
}
