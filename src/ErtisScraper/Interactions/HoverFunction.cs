using System;
using System.Linq;
using System.Threading.Tasks;
using ErtisScraper.Extensions;
using PuppeteerSharp;

namespace ErtisScraper.Interactions
{
	public class HoverFunction : FunctionBase, IInteractionFunction
	{
		#region Properties

		public override string Name => "hover";

		protected override FunctionParameter[] DefaultParameters
		{
			get
			{
				return new FunctionParameter[]
				{
					new FunctionParameter<string>
					{
						Name = "selector"
					}
				};
			}
		}

		#endregion

		#region Methods

		public async Task ExecuteAsync(Page page)
		{
			var selector = this.GetParameterValue<string>("selector");
			
			var frame = this.GetParameterValue<string>("frame");
			if (string.IsNullOrEmpty(frame))
			{
				if (selector.StartsWith(XPathSelector.XPathSelectorToken))
				{
					var element = await page.QuerySelectorByXPath(selector);
					if (element != null)
					{
						await element.HoverAsync();
					}
					else
					{
						throw new Exception($"Node not found with '{selector}' selector on hover function");
					}
				}
				else
				{
					await page.HoverAsync(selector);	
				}
			}
			else
			{
				var currentFrame = page.Frames.FirstOrDefault(x => x.Name == frame);
				if (currentFrame != null)
				{
					if (selector.StartsWith(XPathSelector.XPathSelectorToken))
					{
						var element = await currentFrame.QuerySelectorByXPath(selector);
						if (element != null)
						{
							await element.HoverAsync();
						}
						else
						{
							throw new Exception($"Node not found with '{selector}' selector on hover function");
						}
					}
					else
					{
						await currentFrame.HoverAsync(selector);	
					}
				}
				else
				{
					throw new Exception($"Frame not found with name '{frame}'");
				}
			}
		}

		#endregion
	}
}