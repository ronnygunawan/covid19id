using System.Collections.Immutable;

namespace Covid19id.Models {
	public class HistoricalData {
		public string Country { get; }
		public string? Province { get; }
		public double Latitude { get; }
		public double Longitude { get; }
		public ImmutableList<HistoricalDatum> TimeSeries { get; }

		public HistoricalData(
			string country,
			string? province,
			double latitude,
			double longitude,
			ImmutableList<HistoricalDatum> timeSeries
		) {
			Country = country;
			Province = province;
			Latitude = latitude;
			Longitude = longitude;
			TimeSeries = timeSeries;
		}
	}
}
