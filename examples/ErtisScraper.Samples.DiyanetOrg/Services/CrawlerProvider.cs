using ErtisScraper.Abstractions;
using ErtisScraper.Samples.DiyanetOrg.Services.Interfaces;

namespace ErtisScraper.Samples.DiyanetOrg.Services
{
	public class CrawlerProvider : ICrawlerProvider
	{
		#region Services

		private readonly CrawlerTarget _target;
		private readonly Crawler _crawler;

		#endregion

		#region Constructors

		public CrawlerProvider(ITargetProvider targetProvider)
		{
			this._target = targetProvider.GetTarget("diyanet");
			this._crawler = CrawlerFactory.CreateAsync(this._target).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		#endregion
		
		#region Methods
		
		public Crawler GetCrawler()
		{
			return this._crawler;
		}
		
		public CrawlerTarget GetTarget()
		{
			return this._target;
		}
		
		#endregion
	}
}