// ReSharper disable ReplaceSubstringWithRangeIndexer
namespace ErtisScraper.Extensions
{
	public static class StringExtensions
	{
		#region Methods

		public static string TrimStart(this string target, string trimString)
		{
			if (string.IsNullOrEmpty(trimString))
			{
				return target;
			}

			string result = target;
			while (result.StartsWith(trimString))
			{
				result = result.Substring(trimString.Length);
			}

			return result;
		}
		
		public static string TrimEnd(this string target, string trimString)
		{
			if (string.IsNullOrEmpty(trimString))
			{
				return target;
			}

			string result = target;
			while (result.EndsWith(trimString))
			{
				result = result.Substring(0, result.Length - trimString.Length);
			}

			return result;
		}

		public static string TrimAllWhitespaces(this string text)
		{
			return text.Replace(" ", string.Empty)
				.Replace("\n", string.Empty)
				.Replace("\n\r", string.Empty)
				.Trim();
		}

		#endregion
	}
}