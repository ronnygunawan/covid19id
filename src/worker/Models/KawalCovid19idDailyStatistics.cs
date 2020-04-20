namespace Covid19id.Models {
	public class KawalCovid19idDailyStatistics {
		public string Date { get; }
		public int NewCases { get; }
		public int Cases { get; }
		public int ActiveCases { get; }
		public int NewRecoveries { get; }
		public int Recovered { get; }
		public int NewDeaths { get; }
		public int Deceased { get; }
		public int? PDP { get; }
		public int? ODP { get; }
		public int Observed { get; }
		public int Confirmed { get; }
		public int Negatives { get; }
		public int Observing { get; }

		public KawalCovid19idDailyStatistics(
			string date,
			int newCases,
			int cases,
			int activeCases,
			int newRecoveries,
			int recovered,
			int newDeaths,
			int deceased,
			int? pdp,
			int? odp,
			int observed,
			int confirmed,
			int negatives,
			int observing
		) {
			Date = date;
			NewCases = newCases;
			Cases = cases;
			ActiveCases = activeCases;
			NewRecoveries = newRecoveries;
			Recovered = recovered;
			NewDeaths = newDeaths;
			Deceased = deceased;
			PDP = pdp;
			ODP = odp;
			Observed = observed;
			Confirmed = confirmed;
			Negatives = negatives;
			Observing = observing;
		}
	}
}
