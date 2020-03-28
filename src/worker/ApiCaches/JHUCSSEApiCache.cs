using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.ApiClients;
using Covid19id.Models;
using Covid19id.Apis;

namespace Covid19id.ApiCaches {
	public class JHUCSSEApiCache : IJHUCSSEApi {
		private readonly JHUCSSEApiClient _client;
		private static readonly ConcurrentDictionary<DateTime, JHUCSSEDailyReport> DAILY_REPORT_BY_UTC_DATE = new ConcurrentDictionary<DateTime, JHUCSSEDailyReport>();

		public JHUCSSEApiCache(
			JHUCSSEApiClient client
		) {
			_client = client;
		}

		public async Task<JHUCSSEDailyReport> GetDailyReportAsync(DateTime utcDate, CancellationToken cancellationToken) {
			if (DAILY_REPORT_BY_UTC_DATE.TryGetValue(utcDate, out JHUCSSEDailyReport? dailyReport)) {
				return dailyReport;
			} else {
				dailyReport = await _client.GetDailyReportAsync(utcDate, cancellationToken).ConfigureAwait(false);
				DAILY_REPORT_BY_UTC_DATE.TryAdd(utcDate, dailyReport);
				return dailyReport;
			}
		}
	}
}
