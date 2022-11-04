using System.Collections.Generic;

namespace ErtisScraper.Abstractions
{
	public interface ITargetProvider
	{
		IEnumerable<CrawlerTarget> GetTargets();
		
		CrawlerTarget GetTarget(string name);
	}
}