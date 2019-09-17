using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace humany_customer_seo_netcore.Pages
{
	public class Example1Model : PageModel
	{
		private readonly ILogger<Example1Model> _logger;

		private readonly ISeoService seoService;

		public WidgetConfiguration WidgetConfig = new WidgetConfiguration("seo-customer", "inline-v4", "v4");

		public PageResult? SeoPage { get; private set; }

		public Example1Model(ILogger<Example1Model> logger, ISeoService seoService)
		{
			_logger = logger;
			this.seoService = seoService;
		}

		public async Task OnGet()
		{
			SeoPage = await seoService.Get(WidgetConfig.Tenant, WidgetConfig.Interface, WidgetConfiguration.GetPathInWidget("Help", Request.Path));
		}
	}
}
