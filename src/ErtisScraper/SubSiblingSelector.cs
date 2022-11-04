using System.Collections.Generic;
using System.Linq;
using ErtisScraper.Extensions;
using HtmlAgilityPack;

namespace ErtisScraper
{
	internal class SubSiblingSelector : HtmlSelectorBase
	{
		#region Properties

		public override bool AllowTraverse => false;

		public override string Token => "+";

		#endregion
		
		#region Methods
		
		protected internal override IEnumerable<HtmlNode> FilterCore(IEnumerable<HtmlNode> nodes)
		{
			foreach (var pivotNode in nodes)
			{
				var index = pivotNode.GetSelfIndex();
				var node = pivotNode.ParentNode.ChildNodes
					.Where(i => i.NodeType == HtmlNodeType.Element)
					.Skip(index + 1)
					.FirstOrDefault();
				
				if (node != null)
				{
					yield return node;
				}
			}
		}

		#endregion
	}
}