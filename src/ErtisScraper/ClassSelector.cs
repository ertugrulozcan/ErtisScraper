using System;
using System.Collections.Generic;
using System.Linq;
using ErtisScraper.Extensions;
using HtmlAgilityPack;

namespace ErtisScraper
{
	internal class ClassSelector : HtmlSelectorBase
	{
		#region Properties

		public override string Token => ".";

		#endregion

		#region Methods

		protected internal override IEnumerable<HtmlNode> FilterCore(IEnumerable<HtmlNode> nodes)
		{
			foreach (var node in nodes)
			{
				if (node.GetClassList().Any(c => c.Equals(this.Selector, StringComparison.InvariantCultureIgnoreCase)))
				{
					yield return node;
				}
			}
		}

		#endregion
	}
}