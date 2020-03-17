using System;

namespace Covid19id.Models {
	public class HistoricalDatum {
		public DateTime Date { get; }
		public int Confirmed { get; }
		public int Deceased { get; }
		public int Recovered { get; }

		public HistoricalDatum(
			DateTime date,
			int confirmed,
			int deceased,
			int recovered
		) {
			Date = date;
			Confirmed = confirmed;
			Deceased = deceased;
			Recovered = recovered;
		}
	}
}
