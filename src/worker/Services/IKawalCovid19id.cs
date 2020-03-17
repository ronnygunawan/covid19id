using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.Models;

namespace Covid19id.Services {
	public interface IKawalCovid19id {
		Task<ImmutableList<DailyStatistics>> GetDailyStatisticsAsync(CancellationToken cancellationToken);
		Task<ImmutableList<ProvinceStatistics>> GetProvinceStatisticsAsync(CancellationToken cancellationToken);
	}
}
