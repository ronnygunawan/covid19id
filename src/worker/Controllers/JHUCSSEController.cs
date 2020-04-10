using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.Models;
using Covid19id.Services;
using Microsoft.AspNetCore.Mvc;

namespace Covid19id.Controllers {
	[ApiController]
	[Route("jhucsse")]
	public class JHUCSSEController {
		private readonly JHUCSSEServices _jhucsseServices;

		public JHUCSSEController(
			JHUCSSEServices jhucsseServices
		) {
			_jhucsseServices = jhucsseServices;
		}

		[HttpGet("historical")]
		public async Task<JHUCSSEHistoricalReport> GetHistoricalReportAsync(string? country, string? admin1, string? admin2, CancellationToken cancellationToken) {
			if (country is null) {
				return await _jhucsseServices.GetWorldHistoricalReportAsync(cancellationToken).ConfigureAwait(false);
			} else if (admin1 is null) {
				return await _jhucsseServices.GetCountryHistoricalReportAsync(country, cancellationToken).ConfigureAwait(false);
			} else if (admin2 is null) {
				return await _jhucsseServices.GetAdmin1HistoricalReportAsync(country, admin1, cancellationToken).ConfigureAwait(false);
			} else {
				return await _jhucsseServices.GetAdmin2HistoricalReportAsync(country, admin1, admin2, cancellationToken).ConfigureAwait(false);
			}
		}

		[HttpGet("countries")]
		public async Task<ImmutableList<Country>> GetAllCountriesAsync(CancellationToken cancellationToken) {
			return await _jhucsseServices.GetAllCountriesAsync(cancellationToken).ConfigureAwait(false);
		}
	}
}
