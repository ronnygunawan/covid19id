namespace Covid19id.Extensions {
	public static class StringExtensions {
		public static string? DefaultIfWhiteSpace(this string s) {
			if (string.IsNullOrWhiteSpace(s)) return null;
			else return s;
		}
	}
}
