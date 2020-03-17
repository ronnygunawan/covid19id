using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.Models;
using Covid19id.Services;
using Microsoft.AspNetCore.Mvc;

namespace Covid19id.Controllers {
	[ApiController]
	[Route("kawalcovid19")]
	public class KawalCovid19Controller : ControllerBase {
		private readonly IKawalCovid19id _kawalCovid19id;

		public KawalCovid19Controller(
			IKawalCovid19id kawalCovid19id
		) {
			_kawalCovid19id = kawalCovid19id;
		}

		[HttpGet("statistik-harian")]
		public async Task<ImmutableList<DailyStatistics>> GetDailyStatisticsAsync(CancellationToken cancellationToken) {
			return await _kawalCovid19id.GetDailyStatisticsAsync(cancellationToken).ConfigureAwait(false);
		}

		[HttpGet("statistik-provinsi")]
		public async Task<ImmutableList<ProvinceStatistics>> GetProvinceStatisticsAsync(CancellationToken cancellationToken) {
			return await _kawalCovid19id.GetProvinceStatisticsAsync(cancellationToken).ConfigureAwait(false);
		}
	}
}
