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
using Microsoft.Extensions.Caching.Memory;

namespace Covid19id.ApiClients {
	public class JHUCSSEApiClient : IJHUCSSEApi {
		private const string CACHE_FOLDER_NAME = "csse_covid_19_daily_reports";
		private const int PARSER_VERSION = 1;

		private static readonly DateTime JAN_22ND_UTC = new DateTime(2020, 1, 22, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime JAN_23RD_UTC = new DateTime(2020, 1, 23, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime JAN_27TH_UTC = new DateTime(2020, 1, 27, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// US Admin1 written in {Admin2}, {State code} format
		/// </summary>
		private static readonly DateTime FEB_1ST_UTC = new DateTime(2020, 2, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Canada Admin1 written in {Admin2}, {State code} format
		/// </summary>
		private static readonly DateTime FEB_4TH_UTC = new DateTime(2020, 2, 4, 0, 0, 0, DateTimeKind.Utc);

		private static readonly DateTime FEB_8TH_UTC = new DateTime(2020, 2, 8, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime FEB_21ST_UTC = new DateTime(2020, 2, 21, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime FEB_23RD_UTC = new DateTime(2020, 2, 23, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime FEB_25TH_UTC = new DateTime(2020, 2, 25, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime FEB_28TH_UTC = new DateTime(2020, 2, 28, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// CSV v2 format
		/// </summary>
		private static readonly DateTime MARCH_1ST_UTC = new DateTime(2020, 3, 1, 0, 0, 0, DateTimeKind.Utc);

		private static readonly DateTime MARCH_2ND_UTC = new DateTime(2020, 3, 2, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_3RD_UTC = new DateTime(2020, 3, 3, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_4TH_UTC = new DateTime(2020, 3, 4, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_6TH_UTC = new DateTime(2020, 3, 6, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_7TH_UTC = new DateTime(2020, 3, 7, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_8TH_UTC = new DateTime(2020, 3, 8, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_9TH_UTC = new DateTime(2020, 3, 9, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Some country names were briefly changed on March 10th
		/// </summary>
		private static readonly DateTime MARCH_10TH_UTC = new DateTime(2020, 3, 10, 0, 0, 0, DateTimeKind.Utc);

		private static readonly DateTime MARCH_11TH_UTC = new DateTime(2020, 3, 11, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_12TH_UTC = new DateTime(2020, 3, 12, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_13TH_UTC = new DateTime(2020, 3, 13, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_14TH_UTC = new DateTime(2020, 3, 14, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_16TH_UTC = new DateTime(2020, 3, 16, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_18TH_UTC = new DateTime(2020, 3, 18, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_19TH_UTC = new DateTime(2020, 3, 19, 0, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime MARCH_21ST_UTC = new DateTime(2020, 3, 21, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// CSV v3 format
		/// </summary>
		private static readonly DateTime MARCH_22ND_UTC = new DateTime(2020, 3, 22, 0, 0, 0, DateTimeKind.Utc);

		private readonly HttpClient _httpClient;
		private readonly IMemoryCache _memoryCache;

		public JHUCSSEApiClient(
			HttpClient httpClient,
			IMemoryCache memoryCache
		) {
			_httpClient = httpClient;
			_memoryCache = memoryCache;
		}

		private async Task<JHUCSSEDailyReport> GetMarch9thDailyReportAsync(CancellationToken cancellationToken) {
			return await _memoryCache.GetOrCreateAsync("JHUCSSEMarch9thDailyReport", async entry => {
				JHUCSSEDailyReport dailyReport = await GetDailyReportAsync(MARCH_9TH_UTC, cancellationToken).ConfigureAwait(false);
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
				return dailyReport;
			}).ConfigureAwait(false);
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
					string? admin2 = null;
					string country = values[1];
					DateTime lastUpdate = DateTime.Parse(values[2], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
					int confirmed = string.IsNullOrWhiteSpace(values[3]) ? 0 : int.Parse(values[3], CultureInfo.InvariantCulture);
					int deaths = string.IsNullOrWhiteSpace(values[4]) ? 0 : int.Parse(values[4], CultureInfo.InvariantCulture);
					int recovered = string.IsNullOrWhiteSpace(values[5]) ? 0 : int.Parse(values[5], CultureInfo.InvariantCulture);

					#region Data normalization and cleanup

					// Australia was empty and removed later on 24th
					if (utcDate == JAN_23RD_UTC
						&& country == "Australia") continue;

					// Brazil was empty and removed later on 24th
					if (utcDate == JAN_23RD_UTC
						&& country == "Brazil") continue;

					// Colombia was empty and removed later on 24th
					if (utcDate == JAN_23RD_UTC
						&& country == "Colombia") continue;

					// Tibet was empty and removed later on 24th
					if ((utcDate == JAN_22ND_UTC || utcDate == JAN_23RD_UTC)
						&& country == "Mainland China"
						&& admin1 == "Tibet") continue;

					// Malaysia was empty and removed later on 24th
					if (utcDate == JAN_23RD_UTC
						&& country == "Malaysia") continue;

					// Mexico was empty and removed later on 24th
					if (utcDate == JAN_23RD_UTC
						&& country == "Mexico") continue;

					// Philippines was empty and removed later on 24th
					if (utcDate == JAN_23RD_UTC
						&& country == "Philippines") continue;

					if (country == "US"
						&& admin1 == "Chicago") {
						//admin2 = "Chicago";
						admin1 = "Illinois";
					}

					// 1 Confirmed case later removed
					if (utcDate == JAN_27TH_UTC
						&& country == "Ivory Coast") continue;

					// Germany later tracked nationally
					if (country == "Germany"
						&& admin1 == "Bavaria") {
						admin1 = null;
					}

					if (utcDate >= FEB_1ST_UTC
						&& country == "US") {
						string[] admins = admin1!.Split(',');
						if (admins.Length == 2) {
							if (admins[1] == " NE (From Diamond Princess)"
								|| admins[1] == " CA (From Diamond Princess)"
								|| admins[1] == " TX (From Diamond Princess)") {
								admin2 = admins[0].Trim();
								admin1 = GeographyServices.GetUSStateName(admins[1][1..3]);
							} else {
								admin2 = admins[0].Trim();
								admin1 = GeographyServices.GetUSStateName(admins[1].Trim());
							}
						}
						if (admin1 == "Unassigned Location (From Diamond Princess)") {
							admin1 = "Diamond Princess";
						}
					}

					if (utcDate >= FEB_4TH_UTC
						&& country == "Canada") {
						string[] admins = admin1!.Split(',');
						if (admins.Length == 2) {
							//admin2 = admins[0].Trim();
							admin2 = null; // Admin2 later unused in Canada
							admin1 = GeographyServices.GetCanadaStateName(admins[1].Trim());
						}

						// Admin2 later unused in Canada, multiple entries from same Admin1 will be merged
						if (reports.SingleOrDefault(report => report.Country == "Canada"
							&& report.Admin1 == admin1
							&& report.Admin2 == null) is JHUCSSEReport existingReport) {
							existingReport.IncrementStats(
								confirmed: confirmed,
								deaths: deaths,
								recovered: recovered,
								active: null
							);
							continue;
						}
					}

					if (country == "Australia") {
						if (admin1 == "From Diamond Princess") {
							admin1 = "Diamond Princess";
						}
					}

					if (country == "Others") {
						if (admin1 == "Diamond Princess cruise ship") {
							admin1 = "Diamond Princess";
						}
					}

					// Cruise Ship later renamed to Diamond Princess cruise ship
					if (utcDate <= FEB_8TH_UTC
						&& country == "Others"
						&& admin1 == "Cruise Ship") {
						admin1 = "Diamond Princess";
					}

					// Admin1 entered as None instead of empty
					if (utcDate == FEB_21ST_UTC
						&& country == "Lebanon"
						&& admin1 == "None") {
						admin1 = null;
					}

					// Admin1 entered as None instead of empty
					if (utcDate == FEB_23RD_UTC
						&& country == "Iraq"
						&& admin1 == "None") {
						admin1 = null;
					}

					// Admin1 entered as None instead of empty
					if (utcDate == FEB_25TH_UTC
						&& country == "Austria"
						&& admin1 == "None") {
						admin1 = null;
					}

					// Admin1 entered as From Diamond Princess instead of empty
					if (utcDate <= FEB_25TH_UTC
						&& country == "Israel"
						&& admin1 == "From Diamond Princess") {
						admin1 = null;
					}

					// 1 Confirmed case later removed
					if (utcDate == FEB_28TH_UTC
						&& country == "Azerbaijan") continue;

					// 1 Confirmed case later removed
					if (utcDate == FEB_28TH_UTC
						&& country == "North Ireland") continue;

					// Hong Kong later moved into China dataset
					if (country == "Hong Kong"
						&& admin1 == "Hong Kong") {
						country = "China";
					}

					// Macau later moved into China dataset
					if (country == "Macau"
						&& admin1 == "Macau") {
						country = "China";
					}

					// Taiwan was renamed multiple times
					if (country == "Taiwan"
						&& admin1 == "Taiwan") {
						country = "Taiwan";
						admin1 = null;
					}

					// Mainland China later renamed to China
					if (country == "Mainland China") {
						country = "China";
					}

					if (country == "UK") {
						country = "United Kingdom";
						admin1 = "United Kingdom";
					}

					#endregion

					JHUCSSEReport report = new JHUCSSEReport(
						utcDate: utcDate,
						key: admin1 is null
							? country
							: admin2 is null
								? $"{admin1}, {country}"
								: $"{admin2}, {admin1}, {country}",
						country: country,
						admin1: admin1,
						admin2: admin2,
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
				await FillWithZeroesAsync(utcDate, reports, cancellationToken).ConfigureAwait(false);
				return new JHUCSSEDailyReport(
					utcDate: utcDate,
					reportByKey: reports.ToImmutableDictionary(report => report.Key)
				);
			} else if (utcDate < MARCH_22ND_UTC) {
				List<JHUCSSEReport> reports = new List<JHUCSSEReport>();
				foreach (string line in csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1)) {
					List<string> values = CsvParser.Split(line);
					string? admin1 = values[0].DefaultIfWhiteSpace();
					string? admin2 = null;
					string country = values[1];
					DateTime lastUpdate = DateTime.Parse(values[2], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
					int confirmed = int.Parse(values[3], CultureInfo.InvariantCulture);
					int deaths = int.Parse(values[4], CultureInfo.InvariantCulture);
					int recovered = int.Parse(values[5], CultureInfo.InvariantCulture);
					double latitude = double.Parse(values[6], CultureInfo.InvariantCulture);
					double longitude = double.Parse(values[7], CultureInfo.InvariantCulture);

					#region Data normalization and cleanup

					try {
						if (country == "US") {
							// Duplicate entries
							if ((utcDate == MARCH_18TH_UTC || utcDate == MARCH_19TH_UTC)
								&& admin1 == "United States Virgin Islands") continue;
							string[] admins = admin1!.Split(',');
							if (admins.Length == 2) {
								if (admins[1] == " NE (From Diamond Princess)"
									|| admins[1] == " CA (From Diamond Princess)"
									|| admins[1] == " TX (From Diamond Princess)") {
									admin2 = admins[0].Trim();
									admin1 = GeographyServices.GetUSStateName(admins[1][1..3]);
								} else if (admins[1] == " U.S."
									&& admins[0] == "Virgin Islands") {
									admin1 = "Virgin Islands";
								} else if (admins[0] == "Unassigned Location"
									|| admins[0] == "Unknown Location") {
									admin2 = null;
									admin1 = GeographyServices.GetUSStateName(admins[1].Trim());
								} else {
									admin2 = admins[0].Trim();
									admin1 = GeographyServices.GetUSStateName(admins[1].Trim());
								}
							}
							if (admin1 == "Grand Princess Cruise Ship") {
								admin1 = "Grand Princess";
							}
							if (admin1 == "Unassigned Location (From Diamond Princess)") {
								admin1 = "Diamond Princess";
							}
							if (admin1 == "United States Virgin Islands") {
								admin1 = "Virgin Islands";
							}
						}

						if (country == "Canada") {
							string[] admins = admin1!.Split(',');
							if (admins.Length == 2) {
								//admin2 = admins[0].Trim();
								admin2 = null; // Admin2 later unused in Canada
								admin1 = GeographyServices.GetCanadaStateName(admins[1].Trim());
							}

							// Admin2 later unused in Canada, multiple entries from same Admin1 will be merged
							if (reports.SingleOrDefault(report => report.Country == "Canada"
								&& report.Admin1 == admin1
								&& report.Admin2 == null) is JHUCSSEReport existingReport) {
								existingReport.IncrementStats(
									confirmed: confirmed,
									deaths: deaths,
									recovered: recovered,
									active: null
								);
								continue;
							}
						}

						if (country == "Australia") {
							if (admin1 == "From Diamond Princess") {
								admin1 = "Diamond Princess";
							}
						}

						if (country == "Others") {
							if (admin1 == "Diamond Princess cruise ship") {
								admin1 = "Diamond Princess";
							}
						}

						if (country == "Cruise Ship") {
							if (admin1 == "Diamond Princess") {
								country = "Others";
							}
						}

						// Duplicate entries on March 8th
						if (utcDate == MARCH_8TH_UTC
							&& country == "Republic of Ireland") {
							continue;
						}

						// Hong Kong later moved into China dataset
						if ((country == "Hong Kong" || country == "Hong Kong SAR")
							&& admin1 == "Hong Kong") {
							country = "China";
						}

						// Macau later moved into China dataset
						if ((country == "Macau" || country == "Macao SAR")
							&& admin1 == "Macau") {
							country = "China";
						}

						// Taiwan was renamed multiple times
						if ((country == "Taiwan" || country == "Taipei and environs")
							&& admin1 == "Taiwan") {
							country = "Taiwan";
							admin1 = null;
						} else if (country == "Taiwan*") {
							country = "Taiwan";
						}

						// Czech Republic later renamed to Czechia
						if (country == "Czech Republic") {
							country = "Czechia";
						}

						// Faroe Islands was not identified as Admin1 of Denmark
						if (country == "Faroe Islands") {
							country = "Denmark";
							admin1 = "Faroe Islands";
						}

						// Duplicate entries
						if ((utcDate >= MARCH_19TH_UTC && utcDate <= MARCH_21ST_UTC)
							&& country == "Greenland") continue;

						// Greenland was not identified as Admin1 of Denmark
						if (country == "Greenland") {
							country = "Denmark";
							admin1 = "Greenland";
						}

						if (country == "Denmark"
							&& admin1 == null) {
							admin1 = "Denmark";
						}

						// Gibraltar was not identified as Admin1 of United Kingdom
						if (country == "Gibraltar") {
							country = "United Kingdom";
							admin1 = "Gibraltar";
						}

						// Caymand Islands was not identified as Admin1 of United Kingdom
						if (country == "Cayman Islands") {
							country = "United Kingdom";
							admin1 = "Cayman Islands";
						}

						// Shorten as Palestine
						if (country == "occupied Palestinian territory") {
							country = "Palestine";
						}

						// Saint Barthelemy was not identified as Admin1 of France
						if (country == "Saint Barthelemy") {
							country = "France";
							admin1 = "Saint Barthelemy";
						}

						// Duplicate entries
						if ((utcDate >= MARCH_16TH_UTC && utcDate <= MARCH_21ST_UTC)
							&& country == "France"
							&& admin1 == "Guadeloupe") continue;

						// Guadeloupe was not identified as Admin1 of France
						if (country == "Guadeloupe") {
							country = "France";
							admin1 = "Guadeloupe";
						}

						if (country == "France"
							&& admin1 == null) {
							admin1 = "France";
						}

						// Typo: Fench Guiana
						if (country == "France"
							&& admin1 == "Fench Guiana") {
							admin1 = "French Guiana";
						}

						// Duplicate entries
						if (utcDate == MARCH_14TH_UTC
							&& country == "French Guiana") continue;

						// Duplicate entries
						if ((utcDate >= MARCH_16TH_UTC && utcDate <= MARCH_21ST_UTC)
							&& country == "France"
							&& admin1 == "French Guiana") continue;

						// French Guiana was not identified as Admin1 of France
						if (country == "French Guiana") {
							country = "France";
							admin1 = "French Guiana";
						}

						// Normalize South Korea
						if (country == "Korea, South") {
							country = "South Korea";
						}

						// St. Martin was not identified as Admin1 of France
						if (country == "St. Martin") {
							country = "France";
							admin1 = "St Martin";
						}

						if (country == "UK") {
							country = "United Kingdom";
							admin1 = "United Kingdom";
						}

						if (country == "United Kingdom"
							&& (admin1 == null || admin1 == "UK")) {
							admin1 = "United Kingdom";
						}

						// Curacao was not identified as Admin1 of Netherlands
						if (country == "Curacao") {
							country = "Netherlands";
							admin1 = "Curacao";
						}

						if (country == "Netherlands"
							&& admin1 == null) {
							admin1 = "Netherlands";
						}

						// Duplicate entries
						if ((utcDate == MARCH_18TH_UTC || utcDate == MARCH_19TH_UTC)
							&& country == "Netherlands"
							&& admin1 == "Aruba") continue;

						// Aruba was not identified as Admin1 of Netherlands
						if (country == "Aruba") {
							country = "Netherlands";
							admin1 = "Aruba";
						}

						if (country == "Bahamas, The") {
							country = "Bahamas";
						}

						if (country == "East Timor") {
							country = "Timor-Leste";
						}

						if (country == "Gambia, The") {
							country = "Gambia";
						}

						if (country == "Guam") {
							// Duplicate entries
							if (utcDate >= MARCH_16TH_UTC) continue;
							country = "US";
							admin1 = "Guam";
						}

						// Duplicate entries for Gansu on March 11th and 12th
						if ((utcDate == MARCH_11TH_UTC || utcDate == MARCH_12TH_UTC)
							&& country == "Mainland China"
							&& admin1 == "Gansu") continue;

						// Duplicate entries for Hebei on March 11th and 12th
						if ((utcDate == MARCH_11TH_UTC || utcDate == MARCH_12TH_UTC)
							&& country == "Mainland China"
							&& admin1 == "Hebei") continue;

						// Some country names were briefly changed on March 10th
						if (utcDate == MARCH_10TH_UTC) {
							if (country == "Iran (Islamic Republic of)") country = "Iran";
							else if (country == "Republic of Moldova") country = "Moldova";
							else if (country == "occupied Palestinian territory") country = "Palestine";
							else if (country == "Russian Federation") country = "Russia";
							else if (country == "Republic of Korea") country = "South Korea";
							else if (country == "Saint Martin") {
								country = "France";
								admin1 = "St Martin";
							} else if (country == "Viet Nam") country = "Vietnam";
							else if (country == "Channel Islands") {
								country = "United Kingdom";
								admin1 = "Channel Islands";
							}
						}

						// Mainland China later renamed to China
						if (country == "Mainland China") {
							country = "China";
						}
					} catch (FormatException) {
						throw;
					}

					#endregion

					JHUCSSEReport report = new JHUCSSEReport(
						utcDate: utcDate,
						key: admin1 is null
							? country
							: admin2 is null
								? $"{admin1}, {country}"
								: $"{admin2}, {admin1}, {country}",
						country: country,
						admin1: admin1,
						admin2: admin2,
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
				await FillWithZeroesAsync(utcDate, reports, cancellationToken).ConfigureAwait(false);
				try {
					return new JHUCSSEDailyReport(
						utcDate: utcDate,
						reportByKey: reports.ToImmutableDictionary(report => report.Key)
					);
				} catch (Exception exc) {
					throw;
				}
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

						#region Data normalization and cleanup

						// Denmark proper
						if (country == "Denmark" && admin1 == null) {
							admin1 = "Denmark";
						}

						// Metropolitan France
						if (country == "France" && admin1 == null) {
							admin1 = "France";
						}

						// European Netherlands
						if (country == "Netherlands" && admin1 == null) {
							admin1 = "Netherlands";
						}

						// UK
						if (country == "United Kingdom" && admin1 == null) {
							admin1 = "United Kingdom";
						}

						// Duplicate District of Columbia on March 22, 2020
						if (utcDate == MARCH_22ND_UTC
							&& country == "US"
							&& admin1 == "District of Columbia"
							&& admin2 == "District of Columbia"
							&& fips == "11001"
							&& confirmed == 0
							&& deaths == 0
							&& recovered == 0
							&& active == 0) continue;

						// Taiwan was renamed multiple times
						if (country == "Taiwan*") {
							country = "Taiwan";
						}

						#endregion

						JHUCSSEReport report = new JHUCSSEReport(
						utcDate: utcDate,
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
						throw;
					}
				}
				await FillWithZeroesAsync(utcDate, reports, cancellationToken).ConfigureAwait(false);
				try {
					return new JHUCSSEDailyReport(
						utcDate: utcDate,
						reportByKey: reports.ToImmutableDictionary(report => report.Key)
					);
				} catch (Exception exc) {
					throw;
				}
			}
		}

		private async Task FillWithZeroesAsync(DateTime utcDate, List<JHUCSSEReport> reports, CancellationToken cancellationToken) {
			// Camp Ashland quarantined Diamond Princess passengers
			FillWithZeroes(utcDate, FEB_21ST_UTC, reports, "US", "Nebraska", "Ashland", null, null, null);

			// Chicago was removed on March 2nd
			FillWithZeroes(utcDate, MARCH_2ND_UTC, reports, "US", "Illinois", "Chicago", null, null, null);

			// Seattle was removed on March 2nd
			FillWithZeroes(utcDate, MARCH_2ND_UTC, reports, "US", "Washington", "Seattle", null, null, null);

			// Portland was removed on March 3rd
			FillWithZeroes(utcDate, MARCH_3RD_UTC, reports, "US", "Oregon", "Portland", null, null, null);

			// Orange was removed on March 4th
			FillWithZeroes(utcDate, MARCH_4TH_UTC, reports, "US", "California", "Orange", "06059", 33.70147516, -117.76459979999998);

			// Boston was removed on March 6th
			FillWithZeroes(utcDate, MARCH_6TH_UTC, reports, "US", "Massachusetts", "Boston", null, null, null);

			// New York City was removed on March 6th
			FillWithZeroes(utcDate, MARCH_6TH_UTC, reports, "US", "New York", "New York City", "36061", 40.76727260000001, -73.97152637);

			// Queens County, NY was removed on March 6th
			FillWithZeroes(utcDate, MARCH_6TH_UTC, reports, "US", "New York", "Queens County", null, null, null);

			// Tempe, AZ was removed on March 7th
			FillWithZeroes(utcDate, MARCH_7TH_UTC, reports, "US", "Arizona", "Tempe", null, null, null);

			// Berkeley, CA was removed on March 7th
			FillWithZeroes(utcDate, MARCH_7TH_UTC, reports, "US", "California", "Berkeley", null, null, null);

			// Santa Clara, CA was removed on March 7th
			FillWithZeroes(utcDate, MARCH_7TH_UTC, reports, "US", "California", "Santa Clara", "06085", 37.23104908, -121.6970462);

			// Norwell County, MA was removed on March 7th
			FillWithZeroes(utcDate, MARCH_7TH_UTC, reports, "US", "Massachusetts", "Norwell County", null, null, null);

			// Providence, RI was removed on March 7th
			FillWithZeroes(utcDate, MARCH_7TH_UTC, reports, "US", "Rhode Island", "Providence", "44007", 41.87064746, -71.57753536);

			// Santa Cruz County, CA was removed on March 8th
			FillWithZeroes(utcDate, MARCH_8TH_UTC, reports, "US", "California", "Santa Cruz County", null, null, null);

			// Floyd County, GA was removed on March 8th
			FillWithZeroes(utcDate, MARCH_8TH_UTC, reports, "US", "Georgia", "Floyd County", null, null, null);

			// Norfolk County, MA was removed on March 8th
			FillWithZeroes(utcDate, MARCH_8TH_UTC, reports, "US", "Massachusetts", "Norfolk County", null, null, null);

			// A lot of US Admin2s were removed on March 10th
			if (utcDate >= MARCH_10TH_UTC)  {
				JHUCSSEDailyReport march9thDailyReport = await GetMarch9thDailyReportAsync(cancellationToken).ConfigureAwait(false);
				foreach (string admin1 in march9thDailyReport.GetAdmin1Names("US")) {
					foreach (string admin2 in march9thDailyReport.GetAdmin2Names("US", admin1)) {
						JHUCSSEReport report = march9thDailyReport[$"{admin2}, {admin1}, US"];
						FillWithZeroes(utcDate, MARCH_10TH_UTC, reports, "US", admin1, admin2, null, report.Latitude, report.Longitude);
					}
				}
			}

			FillWithZeroes(utcDate, MARCH_10TH_UTC, reports, "Vatican City", null, null, null, 41.9029, 12.4534);

			FillWithZeroes(utcDate, MARCH_13TH_UTC, reports, "Palestine", null, null, null, 31.9522, 35.2332);

			FillWithZeroes(utcDate, MARCH_14TH_UTC, reports, "Canada", "Grand Princess", null, null, 37.6489, -122.6655);

			FillWithZeroes(utcDate, MARCH_14TH_UTC, reports, "US", "Alaska", null, null, 61.370716, -152.404419);

			FillWithZeroes(utcDate, MARCH_22ND_UTC, reports, "Australia", "Diamond Princess", null, null, 35.4437, 139.6380);

			FillWithZeroes(utcDate, MARCH_22ND_UTC, reports, "Cape Verde", null, null, null, 15.1111, -23.6167);
		}

		private void FillWithZeroes(DateTime utcDate, DateTime minUtcDate, List<JHUCSSEReport> reports, string country, string? admin1, string? admin2, string? fips, double? latitude, double? longitude) {
			if (utcDate >= minUtcDate
				&& !reports.Any(report => report.Country == country && report.Admin1 == admin1 && report.Admin2 == admin2)) {
				reports.Add(new JHUCSSEReport(
					utcDate: utcDate,
					key: $"{admin2}, {admin1}, {country}",
					country: country,
					admin1: admin1,
					admin2: admin2,
					fips: fips,
					lastUpdate: utcDate,
					latitude: latitude,
					longitude: longitude,
					confirmed: 0,
					deaths: 0,
					recovered: 0,
					active: null,
					reportVersion: JHUCSSEReportVersion.AddedAdmin2FIPSActive,
					parserVersion: PARSER_VERSION
				));
			}
		}
	}
}
