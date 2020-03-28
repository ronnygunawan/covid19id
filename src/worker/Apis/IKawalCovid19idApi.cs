using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.Models;

namespace Covid19id.Apis {
	public interface IKawalCovid19idApi {
		Task<ImmutableList<KawalCovid19idDailyStatistics>> GetDailyStatisticsAsync(CancellationToken cancellationToken);
		Task<ImmutableList<KawalCovid19idProvinceStatistics>> GetProvinceStatisticsAsync(CancellationToken cancellationToken);
	}
}
