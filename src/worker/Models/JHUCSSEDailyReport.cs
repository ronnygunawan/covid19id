using System;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Covid19id.Models {
	[JsonObject(MemberSerialization.OptIn)]
	public class JHUCSSEDailyReport {
		[JsonProperty("utcDate"), JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTime UtcDate { get; }

		[JsonProperty("reportByKey")]
		private ImmutableDictionary<string, JHUCSSEReport> ReportByKey { get; }

		[JsonConstructor]
		public JHUCSSEDailyReport(
			DateTime utcDate,
			ImmutableDictionary<string, JHUCSSEReport> reportByKey
		) {
			UtcDate = utcDate;
			ReportByKey = reportByKey;
		}

		public bool ContainsKey(string key) => ReportByKey.ContainsKey(key);
		public JHUCSSEReport this[string key] => ReportByKey[key];

		public ImmutableList<JHUCSSEReport> GetReportsByAdmin1(string country, string admin1) => ReportByKey.Values
			.Where(report => report.Country == country && report.Admin1 == admin1)
			.ToImmutableList();

		public ImmutableList<JHUCSSEReport> GetReportsByCountry(string country) => ReportByKey.Values
			.Where(report => report.Country == country)
			.ToImmutableList();

		public ImmutableList<JHUCSSEReport> GetAllReports() => ReportByKey.Values
			.ToImmutableList();

		public ImmutableList<string> GetCountryNames() => ReportByKey.Values
			.GroupBy(report => report.Country)
			.Select(g => g.Key)
			.OrderBy(country => country)
			.ToImmutableList();

		public ImmutableList<string> GetAdmin1Names(string country) => ReportByKey.Values
			.Where(report => report.Country == country)
			.GroupBy(report => report.Admin1)
			.Select(g => g.Key)
			.OfType<string>()
			.OrderBy(admin1 => admin1)
			.ToImmutableList();

		public ImmutableList<string> GetAdmin2Names(string country, string admin1) => ReportByKey.Values
			.Where(report => report.Country == country && report.Admin1 == admin1)
			.GroupBy(report => report.Admin2)
			.Select(g => g.Key)
			.OfType<string>()
			.OrderBy(admin2 => admin2)
			.ToImmutableList();

		public ImmutableList<Country> GetCountries() =>
			(from report in ReportByKey.Values
			 group report by report.Country into c
			 orderby c.Key
			 select new Country(
				 name: c.Key,
				 admin1s: c.Any(report => report.Admin1 != null)
					? c.All(report => report.Admin1 != null)
						? (from report in c
						   group report by report.Admin1 into a1
						   orderby a1.Key
						   select new Admin1(
							   name: a1.Key,
							   admin2s: a1.Any(report => report.Admin2 != null)
								   ? (from report in a1
									  where report.Admin2 != null
									  orderby report.Admin2
									  select new Admin2(report.Admin2!)).ToImmutableList()
								   : null
							  )).ToImmutableList()
						: throw new InvalidOperationException($"Country {c.Key} contains entry with null Admin1 in {UtcDate:MM-dd-yyyy}.csv.")
					: null
			 )).ToImmutableList();
	}
}
