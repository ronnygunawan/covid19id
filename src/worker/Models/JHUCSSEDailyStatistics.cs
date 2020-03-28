using System;
using Newtonsoft.Json;

namespace Covid19id.Models {
	[JsonObject(MemberSerialization.OptIn)]
	public class JHUCSSEDailyStatistics {
		[JsonProperty("utcDate")]
		public DateTime UtcDate { get; }

		[JsonProperty("confirmed")]
		public int Confirmed { get; }

		[JsonProperty("deaths")]
		public int Deaths { get; }

		[JsonProperty("recovered")]
		public int Recovered { get; }

		[JsonProperty("active")]
		public int? Active { get; }

		[JsonConstructor]
		public JHUCSSEDailyStatistics(
			DateTime utcDate,
			int confirmed,
			int deaths,
			int recovered,
			int? active
		) {
			UtcDate = utcDate;
			Confirmed = confirmed;
			Deaths = deaths;
			Recovered = recovered;
			Active = active;
		}
	}
}
