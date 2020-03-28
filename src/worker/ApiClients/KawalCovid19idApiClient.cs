using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.Models;
using Covid19id.Apis;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;

namespace Covid19id.ApiClients {
	public class KawalCovid19idApiClient : IKawalCovid19idApi {
		private const string SPREADSHEET_ID = "1ma1T9hWbec1pXlwZ89WakRk-OfVUQZsOCFl4FwZxzVw";
		private const string DAILY_STATISTICS_RANGE = "'Statistik Harian'!A2:O";
		private const string PROVINCE_STATISTICS_RANGE = "'Kasus per Provinsi'!A3:E38";

		private readonly IConfiguration _configuration;

		public KawalCovid19idApiClient(
			IConfiguration configuration
		) {
			_configuration = configuration;
		}

		public async Task<ImmutableList<KawalCovid19idDailyStatistics>> GetDailyStatisticsAsync(CancellationToken cancellationToken) {
			using SheetsService sheetsService = new SheetsService(new BaseClientService.Initializer {
				ApplicationName = "Covid19id",
				ApiKey = _configuration["GoogleSheetsApiKey"]
			});

			SpreadsheetsResource.ValuesResource.GetRequest getRequest = sheetsService.Spreadsheets.Values.Get(
				spreadsheetId: SPREADSHEET_ID,
				range: DAILY_STATISTICS_RANGE);

			ValueRange response = await getRequest.ExecuteAsync(cancellationToken).ConfigureAwait(false);

			return response.Values
				.Where(row => row.Count >= 15)
				.Select(row => {
					if (row.ToArray() is var columns
						&& columns[0] is string A && TryParseDate(A, out DateTime? date)
						&& columns[1] is string B && int.TryParse(B, out int newCases)
						&& columns[4] is string E && int.TryParse(E, out int cases)
						&& columns[5] is string F && int.TryParse(F, out int activeCases)
						&& columns[6] is string G && int.TryParse(G, out int newRecoveries)
						&& columns[7] is string H && int.TryParse(H, out int recovered)
						&& columns[8] is string I && int.TryParse(I, out int newDeaths)
						&& columns[9] is string J && int.TryParse(J, out int deceased)
						&& columns[11] is string L && int.TryParse(L, out int observed)
						&& columns[12] is string M && int.TryParse(M, out int confirmed)
						&& columns[13] is string N && int.TryParse(N, out int negatives)
						&& columns[14] is string O && int.TryParse(O, out int observing)) {
						return new KawalCovid19idDailyStatistics(
							date: date.Value.ToString("M/d/yy", CultureInfo.InvariantCulture),
							newCases: newCases,
							cases: cases,
							activeCases: activeCases,
							newRecoveries: newRecoveries,
							recovered: recovered,
							newDeaths: newDeaths,
							deceased: deceased,
							observed: observed,
							confirmed: confirmed,
							negatives: negatives,
							observing: observing
						);
					} else {
						return null;
					}
				})
				.OfType<KawalCovid19idDailyStatistics>()
				.ToImmutableList();
		}

		public async Task<ImmutableList<KawalCovid19idProvinceStatistics>> GetProvinceStatisticsAsync(CancellationToken cancellationToken) {
			using SheetsService sheetsService = new SheetsService(new BaseClientService.Initializer {
				ApplicationName = "Covid19id",
				ApiKey = _configuration["GoogleSheetsApiKey"]
			});

			SpreadsheetsResource.ValuesResource.GetRequest getRequest = sheetsService.Spreadsheets.Values.Get(
				spreadsheetId: SPREADSHEET_ID,
				range: PROVINCE_STATISTICS_RANGE);

			ValueRange response = await getRequest.ExecuteAsync(cancellationToken).ConfigureAwait(false);

			return response.Values
				.Where(row => row.Count >= 2)
				.Select(row => {
					if (row.ToArray() is var columns
						&& columns[1] is string province) {
						return new KawalCovid19idProvinceStatistics(
							id: columns[0] is string A && int.TryParse(A, out int id) ? id : (int?)null,
							province: province,
							cases: columns.Length >= 3 && columns[2] is string C && int.TryParse(C, out int cases) ? cases : 0,
							deceased: columns.Length >= 4 && columns[3] is string D && int.TryParse(D, out int deceased) ? deceased : 0,
							recovered: columns.Length >= 5 && columns[4] is string E && int.TryParse(E, out int recovered) ? recovered : 0
						);
					} else {
						return null;
					}
				})
				.OfType<KawalCovid19idProvinceStatistics>()
				.ToImmutableList();
		}

		private bool TryParseMonth(string s, out int month) {
			switch (s) {
				case "Jan": month = 1; return true;
				case "Feb": month = 2; return true;
				case "Mar": month = 3; return true;
				case "Apr": month = 4; return true;
				case "May": month = 5; return true;
				case "Mei": month = 5; return true;
				case "Jun": month = 6; return true;
				case "Jul": month = 7; return true;
				case "Agu": month = 8; return true;
				case "Aug": month = 8; return true;
				case "Sep": month = 9; return true;
				case "Okt": month = 10; return true;
				case "Oct": month = 10; return true;
				case "Nov": month = 11; return true;
				case "Des": month = 12; return true;
				case "Dec": month = 12; return true;
				default: month = 0; return false;
			}
		}

		private bool TryParseDate(string s, [NotNullWhen(true)]out DateTime? date) {
			if (s.Split(' ', StringSplitOptions.RemoveEmptyEntries) is var words
				&& words.Length == 2
				&& words[0] is var d && int.TryParse(d, out int day)
				&& words[1] is var m && TryParseMonth(m, out int month)) {
				date = new DateTime(2020, month, day);
				return true;
			} else {
				date = null;
				return false;
			}
		}
	}
}
