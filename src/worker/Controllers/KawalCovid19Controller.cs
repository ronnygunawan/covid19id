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
		private readonly KawalCovid19SheetServices _kawalCovid19SheetServices;

		public KawalCovid19Controller(
			KawalCovid19SheetServices kawalCovid19SheetServices
		) {
			_kawalCovid19SheetServices = kawalCovid19SheetServices;
		}

		[HttpGet("statistik-harian")]
		public async Task<ImmutableList<DailyStatistics>> GetDailyStatisticsAsync(CancellationToken cancellationToken) {
			return await _kawalCovid19SheetServices.GetDailyStatisticsAsync(cancellationToken).ConfigureAwait(false);
		}
	}
}
