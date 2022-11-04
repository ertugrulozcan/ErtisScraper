using System.Threading.Tasks;
using ErtisScraper.Samples.DiyanetOrg.Models;
using ErtisScraper.Samples.DiyanetOrg.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ErtisScraper.Samples.DiyanetOrg.Controllers
{
	[ApiController]
	[Route("crawler")]
	public class CrawlerController : ControllerBase
	{
		#region Services

		private readonly ICrawlerProvider _crawlerProvider;

		#endregion

		#region Constructors

		public CrawlerController(ICrawlerProvider crawlerProvider)
		{
			this._crawlerProvider = crawlerProvider;
		}

		#endregion
		
		#region Methods
		
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var result = await this._crawlerProvider.GetCrawler().ScrapeAsync<MonthlyPrayerTimes>($"https://{this._crawlerProvider.GetTarget().Domain}");
			return this.Ok(result);
		}
		
		#endregion
	}
}