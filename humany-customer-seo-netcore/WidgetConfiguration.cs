using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace humany_customer_seo_netcore
{
	public class WidgetConfiguration
	{
		public string HumanyTenantDomain { get => $"{Tenant}.humany.cc"; }
		public string Tenant { get; set; } = string.Empty;
		public string Interface { get; set; } = string.Empty;
		public string Implementation { get; set; } = string.Empty;
		public string BasePath { get; set; } = string.Empty;

		public WidgetConfiguration()
		{
		}

		public WidgetConfiguration(string tenant, string interface_, string implementation, string basePath = "/")
		{
			Tenant = tenant;
			Interface = interface_;
			Implementation = implementation;
			BasePath = basePath;
		}

		public bool IsValid()
		{
			return !string.IsNullOrWhiteSpace(Tenant) && !string.IsNullOrWhiteSpace(Interface) && !string.IsNullOrWhiteSpace(Implementation);
		}
		public static string GetPathInWidget(string pageName, string requestPath)
		{
			var pathInWidgetMatch = new System.Text.RegularExpressions.Regex(@$"^/{pageName.Trim('/')}/?(.*)").Match(requestPath);
			return pathInWidgetMatch.Success ? pathInWidgetMatch.Groups[1].Value : "";
		}
	}
}
