using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.Apis;
using Covid19id.Extensions;
using Covid19id.Models;
using Covid19id.Services;

namespace Covid19id.ApiClients {
	public class JHUCSSEApiClient : IJHUCSSEApi {
		private const string CACHE_FOLDER_NAME = "csse_covid_19_daily_reports";
		private const int PARSER_VERSION = 1;
		private static readonly DateTime MARCH_1ST_UTC = new DateTime(2020, 3, 1, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_22ND_UTC = new DateTime(2020, 3, 22, 0, 0, 0, DateTimeKind.Utc);
		private readonly HttpClient _httpClient;

		public JHUCSSEApiClient(
			HttpClient httpClient
		) {
			_httpClient = httpClient;
		}

		public async Task<JHUCSSEDailyReport> GetDailyReportAsync(DateTime utcDate, CancellationToken cancellationToken) {
			Directory.CreateDirectory(CACHE_FOLDER_NAME);
			string csv;
			string fileName = $"{CACHE_FOLDER_NAME}/{utcDate:MM-dd-yyyy}.csv";
			if (File.Exists(fileName)) {
				csv = await File.ReadAllTextAsync(fileName, cancellationToken).ConfigureAwait(false);
			} else {
				Uri uri = new Uri($"https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_daily_reports/{utcDate:MM-dd-yyyy}.csv");
				using HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
				if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound) throw new KeyNotFoundException($"{utcDate:MM-dd-yyyy}.csv not yet available.");
				httpResponseMessage.EnsureSuccessStatusCode();
				csv = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
#pragma warning disable RG0009 // Not using overload with CancellationToken.
				await File.WriteAllTextAsync(fileName, csv).ConfigureAwait(false);
#pragma warning restore RG0009 // Not using overload with CancellationToken.
				cancellationToken.ThrowIfCancellationRequested();
			}

			if (utcDate < MARCH_1ST_UTC) {
				List<JHUCSSEReport> reports = new List<JHUCSSEReport>();
				foreach(string line in csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1)) {
					List<string> values = CsvParser.Split(line);
					string? admin1 = values[0].DefaultIfWhiteSpace();
					string country = values[1];
					DateTime lastUpdate = DateTime.Parse(values[2], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
					int confirmed = string.IsNullOrWhiteSpace(values[3]) ? 0 : int.Parse(values[3], CultureInfo.InvariantCulture);
					int deaths = string.IsNullOrWhiteSpace(values[4]) ? 0 : int.Parse(values[4], CultureInfo.InvariantCulture);
					int recovered = string.IsNullOrWhiteSpace(values[5]) ? 0 : int.Parse(values[5], CultureInfo.InvariantCulture);

					JHUCSSEReport report = new JHUCSSEReport(
						key: admin1 is null
							? country
							: $"{admin1}, {country}",
						country: country,
						admin1: admin1,
						admin2: null,
						fips: null,
						lastUpdate: lastUpdate,
						latitude: null,
						longitude: null,
						confirmed: confirmed,
						deaths: deaths,
						recovered: recovered,
						active: null,
						reportVersion: JHUCSSEReportVersion.Original,
						parserVersion: PARSER_VERSION
					);

					reports.Add(report);
				}
				return new JHUCSSEDailyReport(
					utcDate: utcDate,
					reportByKey: reports.ToImmutableDictionary(report => report.Key)
				);
			} else if (utcDate < MARCH_22ND_UTC) {
				List<JHUCSSEReport> reports = new List<JHUCSSEReport>();
				foreach (string line in csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1)) {
					List<string> values = CsvParser.Split(line);
					string? admin1 = values[0].DefaultIfWhiteSpace();
					string country = values[1];
					DateTime lastUpdate = DateTime.Parse(values[2], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
					int confirmed = int.Parse(values[3], CultureInfo.InvariantCulture);
					int deaths = int.Parse(values[4], CultureInfo.InvariantCulture);
					int recovered = int.Parse(values[5], CultureInfo.InvariantCulture);
					double latitude = double.Parse(values[6], CultureInfo.InvariantCulture);
					double longitude = double.Parse(values[7], CultureInfo.InvariantCulture);

					JHUCSSEReport report = new JHUCSSEReport(
						key: admin1 is null
							? country
							: $"{admin1}, {country}",
						country: country,
						admin1: admin1,
						admin2: null,
						fips: null,
						lastUpdate: lastUpdate,
						latitude: latitude,
						longitude: longitude,
						confirmed: confirmed,
						deaths: deaths,
						recovered: recovered,
						active: null,
						reportVersion: JHUCSSEReportVersion.AddedLatLong,
						parserVersion: PARSER_VERSION
					);

					reports.Add(report);
				}
				return new JHUCSSEDailyReport(
					utcDate: utcDate,
					reportByKey: reports.ToImmutableDictionary(report => report.Key)
				);
			} else {
				List<JHUCSSEReport> reports = new List<JHUCSSEReport>();
				foreach (string line in csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1)) {
					try {
						List<string> values = CsvParser.Split(line);
						string? fips = values[0].DefaultIfWhiteSpace();
						string? admin2 = values[1].DefaultIfWhiteSpace();
						string? admin1 = values[2].DefaultIfWhiteSpace();
						string country = values[3];
						DateTime lastUpdate = DateTime.Parse(values[4], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
						double? latitude = string.IsNullOrWhiteSpace(values[5]) ? (double?)null : double.Parse(values[5], CultureInfo.InvariantCulture);
						double? longitude = string.IsNullOrWhiteSpace(values[6]) ? (double?)null : double.Parse(values[6], CultureInfo.InvariantCulture);
						int confirmed = int.Parse(values[7], CultureInfo.InvariantCulture);
						int deaths = int.Parse(values[8], CultureInfo.InvariantCulture);
						int recovered = int.Parse(values[9], CultureInfo.InvariantCulture);
						int active = int.Parse(values[10], CultureInfo.InvariantCulture);
						string key = values[11];

						JHUCSSEReport report = new JHUCSSEReport(
							key: key,
							country: country,
							admin1: admin1,
							admin2: admin2,
							fips: fips,
							lastUpdate: lastUpdate,
							latitude: latitude,
							longitude: longitude,
							confirmed: confirmed,
							deaths: deaths,
							recovered: recovered,
							active: active,
							reportVersion: JHUCSSEReportVersion.AddedAdmin2FIPSActive,
							parserVersion: PARSER_VERSION
						);

						reports.Add(report);
					} catch (Exception exc) {
						if (Debugger.IsAttached) {
							Debugger.Break();
						} else {
							Console.WriteLine(exc);
						}
					}
				}
				return new JHUCSSEDailyReport(
					utcDate: utcDate,
					reportByKey: reports.ToImmutableDictionary(report => report.Key)
				);
			}
		}
	}
}
