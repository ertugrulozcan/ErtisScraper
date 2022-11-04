using Newtonsoft.Json;

namespace ErtisScraper
{
	public class CrawlerOptions
	{
		#region Properties

		[JsonProperty("waitfor")]
		public WaitForOptions WaitFor { get; set; }
		
		[JsonProperty("viewport")]
		public ViewportOptions Viewport { get; set; }

		#endregion
	}
}