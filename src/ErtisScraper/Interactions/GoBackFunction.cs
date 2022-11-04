using System.Threading.Tasks;
using PuppeteerSharp;

namespace ErtisScraper.Interactions
{
	public class GoBackFunction : FunctionBase, IInteractionFunction
	{
		#region Properties

		public override string Name => "goback";

		protected override FunctionParameter[] DefaultParameters => null;

		#endregion

		#region Methods

		public async Task ExecuteAsync(Page page)
		{
			await page.GoBackAsync();
		}

		#endregion
	}
}