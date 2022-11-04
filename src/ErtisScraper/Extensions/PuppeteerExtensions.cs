using System.Threading.Tasks;
using PuppeteerSharp;

namespace ErtisScraper.Extensions
{
	public static class PuppeteerExtensions
	{
		#region Methods

		public static async Task<ElementHandle> QuerySelectorByXPath(this Page page, string xpath)
		{
			var script = $"document.evaluate('{xpath}', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;";
			var handle = await page.EvaluateExpressionHandleAsync(script).ConfigureAwait(false);
			if (handle is ElementHandle element)
			{
				return element;
			}

			await handle.DisposeAsync().ConfigureAwait(false);
			return null;
		}
		
		public static async Task<ElementHandle> QuerySelectorByXPath(this Frame frame, string xpath)
		{
			var script = $"document.evaluate('{xpath}', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;";
			var handle = await frame.EvaluateExpressionHandleAsync(script).ConfigureAwait(false);
			if (handle is ElementHandle element)
			{
				return element;
			}

			await handle.DisposeAsync().ConfigureAwait(false);
			return null;
		}

		#endregion
	}
}