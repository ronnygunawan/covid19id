using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Covid19id.Models {
	[JsonObject(MemberSerialization.OptIn)]
	public class JHUCSSEReport {
		[JsonProperty("key")]
		public string Key { get; }

		[JsonProperty("country")]
		public string Country { get; }

		[JsonProperty("admin1", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public string? Admin1 { get; }

		[JsonProperty("admin2", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public string? Admin2 { get; }

		[JsonProperty("fips", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public string? FIPS { get; }

		[JsonProperty("lastUpdate"), JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTime LastUpdate { get; }

		[JsonProperty("latitude", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public double? Latitude { get; }

		[JsonProperty("longitude", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public double? Longitude { get; }

		[JsonProperty("confirmed")]
		public int Confirmed { get; }

		[JsonProperty("deaths")]
		public int Deaths { get; }

		[JsonProperty("recovered")]
		public int Recovered { get; }

		[JsonProperty("active", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public int? Active { get; }

		[JsonProperty("reportVersion")]
		public JHUCSSEReportVersion ReportVersion { get; }

		[JsonProperty("parserVersion")]
		public int ParserVersion { get; }

		[JsonConstructor]
		public JHUCSSEReport(
			string key,
			string country,
			string? admin1,
			string? admin2,
			string? fips,
			DateTime lastUpdate,
			double? latitude,
			double? longitude,
			int confirmed,
			int deaths,
			int recovered,
			int? active,
			JHUCSSEReportVersion reportVersion,
			int parserVersion
		) {
			Key = key;
			Country = country;
			Admin1 = admin1;
			Admin2 = admin2;
			FIPS = fips;
			LastUpdate = lastUpdate;
			Latitude = latitude;
			Longitude = longitude;
			Confirmed = confirmed;
			Deaths = deaths;
			Recovered = recovered;
			Active = active;
			ReportVersion = reportVersion;
			ParserVersion = parserVersion;
		}
	}
}
