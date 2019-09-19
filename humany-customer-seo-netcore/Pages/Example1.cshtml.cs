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

		public Example1Model(ILogger<Example1Model> logger)
		{
			_logger = logger;
		}

		public void OnGet()
		{
		}
	}
}
