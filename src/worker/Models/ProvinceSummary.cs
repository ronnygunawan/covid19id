namespace Covid19id.Models {
	public class ProvinceSummary {
		public string Name { get; }
		public int Confirmed { get; }
		public int Deceased { get; }
		public int Recovered { get; }

		public ProvinceSummary(
			string name,
			int confirmed,
			int deceased,
			int recovered
		) {
			Name = name;
			Confirmed = confirmed;
			Deceased = deceased;
			Recovered = recovered;
		}
	}
}
