namespace Covid19id.Models {
	public class DailyStatistics {
		public int NewCases { get; }
		public int Cases { get; }
		public int ActiveCases { get; }
		public int NewRecoveries { get; }
		public int Recovered { get; }
		public int NewDeaths { get; }
		public int Died { get; }
		public int Observed { get; }
		public int Confirmed { get; }
		public int Negative { get; }
		public int Testing { get; }

		public DailyStatistics(
			int newCases,
			int cases,
			int activeCases,
			int newRecoveries,
			int recovered,
			int newDeaths,
			int died,
			int observed,
			int confirmed,
			int negative,
			int testing
		) {
			NewCases = newCases;
			Cases = cases;
			ActiveCases = activeCases;
			NewRecoveries = newRecoveries;
			Recovered = recovered;
			NewDeaths = newDeaths;
			Died = died;
			Observed = observed;
			Confirmed = confirmed;
			Negative = negative;
			Testing = testing;
		}
	}
}
