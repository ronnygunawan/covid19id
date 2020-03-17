using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.Models;

namespace Covid19id.Services {
	public interface IJohnsHopkinsCSSE {
		Task<ImmutableList<CountrySummary>> GetCountriesAsync(CancellationToken cancellationToken);
		Task<ImmutableList<HistoricalData>> GetAllHistoricalDataAsync(CancellationToken cancellationToken);
		Task<HistoricalData> GetCountryHistoricalDataAsync(string country, CancellationToken cancellationToken);
		Task<HistoricalData> GetProvinceHistoricalDataAsync(string country, string province, CancellationToken cancellationToken);
	}
}
