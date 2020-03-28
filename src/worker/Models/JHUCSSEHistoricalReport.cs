using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Covid19id.Models {
	[JsonObject(MemberSerialization.OptIn)]
	public class JHUCSSEHistoricalReport {
		[JsonProperty("key", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public string? Key { get; }

		[JsonProperty("country", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public string? Country { get; }

		[JsonProperty("admin1", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public string? Admin1 { get; }

		[JsonProperty("admin2", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public string? Admin2 { get; }

		[JsonProperty("fips", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public string? FIPS { get; }

		[JsonProperty("latitude")]
		public double? Latitude { get; }

		[JsonProperty("longitude")]
		public double? Longitude { get; }

		[JsonProperty("timeSeries")]
		public ImmutableList<JHUCSSEDailyStatistics> TimeSeries { get; }

		[JsonConstructor]
		public JHUCSSEHistoricalReport(
			string? key,
			string? country,
			string? admin1,
			string? admin2,
			string? fips,
			double? latitude,
			double? longitude,
			ImmutableList<JHUCSSEDailyStatistics> timeSeries
		) {
			Key = key;
			Country = country;
			Admin1 = admin1;
			Admin2 = admin2;
			FIPS = fips;
			Latitude = latitude;
			Longitude = longitude;
			TimeSeries = timeSeries;
		}
	}
}
