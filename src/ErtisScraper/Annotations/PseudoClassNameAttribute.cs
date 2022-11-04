using System;

namespace ErtisScraper.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public class PseudoClassNameAttribute : Attribute
	{
		#region Properties

		public string FunctionName { get; }

		#endregion

		#region Constructors

		public PseudoClassNameAttribute(string name)
		{
			this.FunctionName = name;
		}

		#endregion
	}
}