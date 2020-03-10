using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace Covid19id.Services {
	public class KawalCovid19SheetServices {
		private static readonly string[] SCOPES = { SheetsService.Scope.SpreadsheetsReadonly };
		private const string SPREADSHEET_ID = "1ma1T9hWbec1pXlwZ89WakRk-OfVUQZsOCFl4FwZxzVw";
		private const string DAILY_STATISTICS_RANGE = "'Statistik Harian'!A2:O";

		private readonly ClientSecrets _clientSecrets;

		public KawalCovid19SheetServices(
			ClientSecrets clientSecrets
		) {
			_clientSecrets = clientSecrets;
		}

		public async Task<ImmutableList<DailyStatistics>> GetDailyStatisticsAsync(CancellationToken cancellationToken) {
			UserCredential userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
				clientSecrets: _clientSecrets,
				scopes: SCOPES,
				user: "user",
				taskCancellationToken: cancellationToken).ConfigureAwait(false);

			using SheetsService sheetsService = new SheetsService(new BaseClientService.Initializer {
				HttpClientInitializer = userCredential,
				ApplicationName = "Covid19id"
			});

			SpreadsheetsResource.ValuesResource.GetRequest getRequest = sheetsService.Spreadsheets.Values.Get(
				spreadsheetId: SPREADSHEET_ID,
				range: DAILY_STATISTICS_RANGE);

			ValueRange response = await getRequest.ExecuteAsync(cancellationToken).ConfigureAwait(false);

			foreach (IList<object> values in response.Values) {
				foreach (object value in values) {
					Console.WriteLine(value);
				}
			}

			return ImmutableList<DailyStatistics>.Empty;
		}
	}
}
