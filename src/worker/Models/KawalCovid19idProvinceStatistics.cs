namespace Covid19id.Models {
	public class KawalCovid19idProvinceStatistics {
		public int? Id { get; }
		public string Province { get; }
		public int Cases { get; }
		public int Deceased { get; }
		public int Recovered { get; }

		public KawalCovid19idProvinceStatistics(
			int? id,
			string province,
			int cases,
			int deceased,
			int recovered
		) {
			Id = id;
			Province = province;
			Cases = cases;
			Deceased = deceased;
			Recovered = recovered;
		}
	}
}
