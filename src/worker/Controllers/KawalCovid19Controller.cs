using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.Models;
using Covid19id.Apis;
using Microsoft.AspNetCore.Mvc;

namespace Covid19id.Controllers {
	[ApiController]
	[Route("kawalcovid19")]
	public class KawalCovid19Controller : ControllerBase {
		private readonly IKawalCovid19idApi _kawalCovid19idApi;

		public KawalCovid19Controller(
			IKawalCovid19idApi kawalCovid19idApi
		) {
			_kawalCovid19idApi = kawalCovid19idApi;
		}

		[HttpGet("statistik-harian")]
		public async Task<ImmutableList<KawalCovid19idDailyStatistics>> GetDailyStatisticsAsync(CancellationToken cancellationToken) {
			return await _kawalCovid19idApi.GetDailyStatisticsAsync(cancellationToken).ConfigureAwait(false);
		}

		[HttpGet("statistik-provinsi")]
		public async Task<ImmutableList<KawalCovid19idProvinceStatistics>> GetProvinceStatisticsAsync(CancellationToken cancellationToken) {
			return await _kawalCovid19idApi.GetProvinceStatisticsAsync(cancellationToken).ConfigureAwait(false);
		}
	}
}
