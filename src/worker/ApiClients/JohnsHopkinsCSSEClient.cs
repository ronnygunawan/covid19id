using System;
using System.Collections.Immutable;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.Models;
using Covid19id.Services;

namespace Covid19id.ApiClients {
	public class JohnsHopkinsCSSEClient : IJohnsHopkinsCSSE {
		private readonly HttpClient _httpClient;

		private static readonly Uri REALTIMEURL = new Uri("https://services1.arcgis.com/0MSEUqKaxRlEPj5g/arcgis/rest/services/ncov_cases/FeatureServer/1/query?f=json&where=Confirmed%20%3E%200&returnGeometry=false&spatialRel=esriSpatialRelIntersects&outFields=*&orderByFields=Confirmed%20desc&outSR=102100&resultOffset=0&resultRecordCount=250&cacheHint=true");
		private static readonly Uri CONFIRMEDCASESURL = new Uri("https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_time_series/time_series_19-covid-Confirmed.csv");
		private static readonly Uri DEATHCASESURL = new Uri("https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_time_series/time_series_19-covid-Deaths.csv");
		private static readonly Uri RECOVEREDCASESURL = new Uri("https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_time_series/time_series_19-covid-Recovered.csv");

		public JohnsHopkinsCSSEClient(
			HttpClient httpClient
		) {
			_httpClient = httpClient;
		}

		public Task<ImmutableList<HistoricalData>> GetAllHistoricalDataAsync(CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}

		[Obsolete("Use JohnsHopkinsCSSECache")]
		public Task<ImmutableList<CountrySummary>> GetCountriesAsync(CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}

		[Obsolete("Use JohnsHopkinsCSSECache")]
		public Task<HistoricalData> GetCountryHistoricalDataAsync(string country, CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}

		[Obsolete("Use JohnsHopkinsCSSECache")]
		public Task<HistoricalData> GetProvinceHistoricalDataAsync(string country, string province, CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}
	}
}
