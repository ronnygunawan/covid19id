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

		[HttpGet("world")]
		public async Task<JHUCSSEHistoricalReport> GetWorldHistoricalReportAsync(CancellationToken cancellationToken) {
			return await _jhucsseServices.GetWorldHistoricalReportAsync(cancellationToken).ConfigureAwait(false);
		}
	}
}
