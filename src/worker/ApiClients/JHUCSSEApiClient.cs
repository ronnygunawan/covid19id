using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.PortableExecutable;
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

		/// <summary>
		/// CSV v3 format
		/// </summary>
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

					// Cruise Ship later renamed to Diamond Princess cruise ship
					if (utcDate <= FEB_8TH_UTC
						&& country == "Others"
						&& admin1 == "Cruise Ship") {
						admin1 = "Diamond Princess cruise ship";
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

					#endregion

					JHUCSSEReport report = new JHUCSSEReport(
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
				FillWithZeroes(utcDate, reports);
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
							string[] admins = admin1!.Split(',');
							if (admins.Length == 2) {
								if (admins[1] == " NE (From Diamond Princess)"
									|| admins[1] == " CA (From Diamond Princess)"
									|| admins[1] == " TX (From Diamond Princess)") {
									admin2 = admins[0].Trim();
									admin1 = GeographyServices.GetUSStateName(admins[1][1..3]);
								} else if (admins[1] == " U.S."
									&& admins[0] == "Virgin Islands") {
									admin2 = admins[0].Trim();
									admin1 = "U.S. Virgin Islands";
								} else {
									admin2 = admins[0].Trim();
									admin1 = GeographyServices.GetUSStateName(admins[1].Trim());
								}
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
					} catch (FormatException) {
						throw;
					}

					#endregion

					JHUCSSEReport report = new JHUCSSEReport(
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
				FillWithZeroes(utcDate, reports);
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

						#endregion

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
						throw;
					}
				}
				FillWithZeroes(utcDate, reports);
				return new JHUCSSEDailyReport(
					utcDate: utcDate,
					reportByKey: reports.ToImmutableDictionary(report => report.Key)
				);
			}
		}

		private void FillWithZeroes(DateTime utcDate, List<JHUCSSEReport> reports) {
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
		}

		private void FillWithZeroes(DateTime utcDate, DateTime minUtcDate, List<JHUCSSEReport> reports, string country, string? admin1, string? admin2, string? fips, double? latitude, double? longitude) {
			if (utcDate >= minUtcDate
				&& !reports.Any(report => report.Country == country && report.Admin1 == admin1 && report.Admin2 == admin2)) {
				reports.Add(new JHUCSSEReport(
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
