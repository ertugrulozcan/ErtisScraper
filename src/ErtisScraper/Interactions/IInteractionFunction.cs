using System.Threading.Tasks;
using PuppeteerSharp;

namespace ErtisScraper.Interactions
{
	public interface IInteractionFunction
	{
		#region Properties
		
		string Name { get; }
		
		string Comment { get; }
		
		#endregion
		
		#region Methods

		Task ExecuteAsync(Page page);

		#endregion
	}
}