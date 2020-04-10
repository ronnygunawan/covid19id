using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.Apis;
using Covid19id.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Covid19id.Services {
	public class JHUCSSEServices {
		private static readonly DateTime START_DATE = new DateTime(2020, 1, 22, 0, 0, 0, DateTimeKind.Utc);
		private readonly IJHUCSSEApi _jhucsseApi;
		private readonly IMemoryCache _memoryCache;

		public JHUCSSEServices(
			IJHUCSSEApi jhucsseApi,
			IMemoryCache memoryCache
		) {
			_jhucsseApi = jhucsseApi;
			_memoryCache = memoryCache;
		}

		public async Task<JHUCSSEDailyReportCollection> GetAllDailyReportsAsync(CancellationToken cancellationToken) {
			return await _memoryCache.GetOrCreateAsync("JHUCSSEAllDailyReports", async entry => {
				int days = (int)DateTime.UtcNow.Date.Subtract(START_DATE).TotalDays + 1;

				Task<JHUCSSEDailyReport?>[] dailyReportTasks = Enumerable.Range(0, days)
					.Select(i => START_DATE.AddDays(i))
					.Select(async utcDate => {
						try {
							return await _jhucsseApi.GetDailyReportAsync(utcDate, cancellationToken).ConfigureAwait(false);
						} catch (KeyNotFoundException) {
							return null;
						}
					})
					.ToArray();

				JHUCSSEDailyReport?[] dailyReports = await Task.WhenAll(dailyReportTasks).ConfigureAwait(false);

				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
				return new JHUCSSEDailyReportCollection(
					dailyReportByUtcDate: dailyReports
						.OfType<JHUCSSEDailyReport>()
						.ToImmutableDictionary(
							keySelector: dailyReport => dailyReport.UtcDate
						)
				);
			}).ConfigureAwait(false);
		}

		public async Task<ImmutableList<Country>> GetAllCountriesAsync(CancellationToken cancellationToken) {
			JHUCSSEDailyReportCollection allDailyReports = await GetAllDailyReportsAsync(cancellationToken).ConfigureAwait(false);
			return allDailyReports[allDailyReports.Keys.Max()].GetCountries();
		}

		public async Task<JHUCSSEHistoricalReport> GetWorldHistoricalReportAsync(CancellationToken cancellationToken) {
			JHUCSSEDailyReportCollection allDailyReports = await GetAllDailyReportsAsync(cancellationToken).ConfigureAwait(false);
			return allDailyReports.GetWorldHistoricalReport();
		}

		public async Task<JHUCSSEHistoricalReport> GetCountryHistoricalReportAsync(string country, CancellationToken cancellationToken) {
			JHUCSSEDailyReportCollection allDailyReports = await GetAllDailyReportsAsync(cancellationToken).ConfigureAwait(false);
			return allDailyReports.GetHistoricalReportByCountry(country) ?? throw new KeyNotFoundException($"Historical report not found for {country}.");
		}

		public async Task<JHUCSSEHistoricalReport> GetAdmin1HistoricalReportAsync(string country, string admin1, CancellationToken cancellationToken) {
			JHUCSSEDailyReportCollection allDailyReports = await GetAllDailyReportsAsync(cancellationToken).ConfigureAwait(false);
			return allDailyReports.GetHistoricalReportByAdmin1(country, admin1) ?? throw new KeyNotFoundException($"Historical report not found for {admin1}, {country}.");
		}

		public async Task<JHUCSSEHistoricalReport> GetAdmin2HistoricalReportAsync(string country, string admin1, string admin2, CancellationToken cancellationToken) {
			JHUCSSEDailyReportCollection allDailyReports = await GetAllDailyReportsAsync(cancellationToken).ConfigureAwait(false);
			return allDailyReports.GetHistoricalReportByKey($"{admin2}, {admin1}, {country}") ?? throw new KeyNotFoundException($"Historical report not found for {admin2}, {admin1}, {country}.");
		}
	}
}
