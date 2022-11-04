namespace ErtisScraper.Samples.DiyanetOrg.Services.Interfaces
{
	public interface ICrawlerProvider
	{
		Crawler GetCrawler();

		CrawlerTarget GetTarget();
	}
}