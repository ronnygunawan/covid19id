using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;

namespace Covid19id.Models {
	[JsonObject(MemberSerialization.OptIn)]
	public class JHUCSSEDailyReportCollection {
		[JsonProperty("dailyReportByUtcDate")]
		private ImmutableDictionary<DateTime, JHUCSSEDailyReport> DailyReportByUtcDate { get; }

		[JsonConstructor]
		public JHUCSSEDailyReportCollection(
			ImmutableDictionary<DateTime, JHUCSSEDailyReport> dailyReportByUtcDate
		) {
			DailyReportByUtcDate = dailyReportByUtcDate;
		}

		public IEnumerable<DateTime> Keys => DailyReportByUtcDate.Keys;
		public JHUCSSEDailyReport this[DateTime utcDate] => DailyReportByUtcDate[utcDate];

		public JHUCSSEHistoricalReport? GetHistoricalReportByKey(string key) {
			ImmutableList<(DateTime UtcDate, JHUCSSEReport Report)> timeSeries = DailyReportByUtcDate
				.Where(kvp => kvp.Value.ContainsKey(key))
				.Select(kvp => (kvp.Key, kvp.Value[key]))
				.ToImmutableList();

			if (timeSeries.Count == 0) return null;

			return new JHUCSSEHistoricalReport(
				key: key,
				country: timeSeries[0].Report.Country,
				admin1: timeSeries[0].Report.Admin1,
				admin2: timeSeries[0].Report.Admin2,
				fips: timeSeries[0].Report.FIPS,
				latitude: timeSeries[0].Report.Latitude,
				longitude: timeSeries[0].Report.Longitude,
				timeSeries: timeSeries
					.OrderBy(datum => datum.UtcDate)
					.Select(datum => new JHUCSSEDailyStatistics(
						utcDate: datum.UtcDate,
						confirmed: datum.Report.Confirmed,
						deaths: datum.Report.Deaths,
						recovered: datum.Report.Recovered,
						active: datum.Report.Active
					))
					.ToImmutableList()
			);
		}

		public JHUCSSEHistoricalReport? GetHistoricalReportByAdmin1(string country, string admin1) {
			ImmutableList<(DateTime UtcDate, ImmutableList<JHUCSSEReport> Reports)> timeSeries = DailyReportByUtcDate
				.Select(kvp => (UtcDate: kvp.Key, Reports: kvp.Value.GetReportsByAdmin1(country, admin1)))
				.Where(datum => !datum.Reports.IsEmpty)
				.ToImmutableList();

			if (timeSeries.Count == 0) return null;

			return new JHUCSSEHistoricalReport(
				key: null,
				country: country,
				admin1: admin1,
				admin2: null,
				fips: null,
				latitude: null,
				longitude: null,
				timeSeries: timeSeries
					.OrderBy(datum => datum.UtcDate)
					.Select(datum => new JHUCSSEDailyStatistics(
						utcDate: datum.UtcDate,
						confirmed: datum.Reports.Sum(report => report.Confirmed),
						deaths: datum.Reports.Sum(report => report.Deaths),
						recovered: datum.Reports.Sum(report => report.Recovered),
						active: datum.Reports.Any(report => report.Active.HasValue)
							? datum.Reports.Sum(report => report.Active ?? 0)
							: (int?)null
					))
					.ToImmutableList()
			);
		}

		public JHUCSSEHistoricalReport? GetHistoricalReportByCountry(string country) {
			ImmutableList<(DateTime UtcDate, ImmutableList<JHUCSSEReport> Reports)> timeSeries = DailyReportByUtcDate
				.Select(kvp => (UtcDate: kvp.Key, Reports: kvp.Value.GetReportsByCountry(country)))
				.Where(datum => !datum.Reports.IsEmpty)
				.ToImmutableList();

			if (timeSeries.Count == 0) return null;

			return new JHUCSSEHistoricalReport(
				key: null,
				country: country,
				admin1: null,
				admin2: null,
				fips: null,
				latitude: null,
				longitude: null,
				timeSeries: timeSeries
					.OrderBy(datum => datum.UtcDate)
					.Select(datum => new JHUCSSEDailyStatistics(
						utcDate: datum.UtcDate,
						confirmed: datum.Reports.Sum(report => report.Confirmed),
						deaths: datum.Reports.Sum(report => report.Deaths),
						recovered: datum.Reports.Sum(report => report.Recovered),
						active: datum.Reports.Any(report => report.Active.HasValue)
							? datum.Reports.Sum(report => report.Active ?? 0)
							: (int?)null
					))
					.ToImmutableList()
			);
		}

		public JHUCSSEHistoricalReport GetWorldHistoricalReport() {
			ImmutableList<(DateTime UtcDate, ImmutableList<JHUCSSEReport> Reports)> timeSeries = DailyReportByUtcDate
				.Select(kvp => (UtcDate: kvp.Key, Reports: kvp.Value.GetAllReports()))
				.ToImmutableList();

			return new JHUCSSEHistoricalReport(
				key: null,
				country: null,
				admin1: null,
				admin2: null,
				fips: null,
				latitude: null,
				longitude: null,
				timeSeries: timeSeries
					.OrderBy(datum => datum.UtcDate)
					.Select(datum => new JHUCSSEDailyStatistics(
						utcDate: datum.UtcDate,
						confirmed: datum.Reports.Sum(report => report.Confirmed),
						deaths: datum.Reports.Sum(report => report.Deaths),
						recovered: datum.Reports.Sum(report => report.Recovered),
						active: datum.Reports.All(report => report.Active.HasValue)
							? datum.Reports.Sum(report => report.Active!.Value)
							: (int?)null
					))
					.ToImmutableList()
			);
		}
	}
}
