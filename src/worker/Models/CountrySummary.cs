using System.Collections.Immutable;

namespace Covid19id.Models {
	public class CountrySummary {
		public string Name { get; }
		public int Confirmed { get; }
		public int Deceased { get; }
		public int Recovered { get; }
		public ImmutableList<ProvinceSummary> Provinces { get; }

		public CountrySummary(
			string name,
			int confirmed,
			int deceased,
			int recovered,
			ImmutableList<ProvinceSummary> provinces
		) {
			Name = name;
			Confirmed = confirmed;
			Deceased = deceased;
			Recovered = recovered;
			Provinces = provinces;
		}
	}
}
