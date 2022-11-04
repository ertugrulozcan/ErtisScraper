using Newtonsoft.Json;

namespace ErtisScraper
{
	public struct FieldOptions
	{
		#region Properties

		[JsonProperty("format")]
		public FieldFormatting Format { get; set; }

		#endregion
	}
}