using System;
using System.Threading.Tasks;

namespace ErtisScraper.Abstractions
{
	public interface ICrawler : IDisposable, IAsyncDisposable
	{
		CrawlerTarget Target { get; }

		Task<ScrapingResult<T>> ScrapeAsync<T>(string url) where T : class;

		Task<ScrapingResult> ScrapeAsync(string url);
	}
}