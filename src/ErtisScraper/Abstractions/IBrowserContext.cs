using System;
using PuppeteerSharp;

namespace ErtisScraper.Abstractions
{
	public interface IBrowserContext : IDisposable, IAsyncDisposable
	{
		Browser Browser { get; }
	}
}