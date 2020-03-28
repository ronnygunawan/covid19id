using System;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.Models;

namespace Covid19id.Apis {
	public interface IJHUCSSEApi {
		Task<JHUCSSEDailyReport> GetDailyReportAsync(DateTime utcDate, CancellationToken cancellationToken);
	}
}
