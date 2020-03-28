using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.ApiClients;
using Covid19id.Models;
using Covid19id.Apis;
using Microsoft.Extensions.Caching.Memory;

namespace Covid19id.ApiCaches {
	public class KawalCovid19idApiCache : IKawalCovid19idApi {
		private readonly IMemoryCache _memoryCache;
		private readonly KawalCovid19idApiClient _client;

		public KawalCovid19idApiCache(
			IMemoryCache memoryCache,
			KawalCovid19idApiClient client
		) {
			_memoryCache = memoryCache;
			_client = client;
		}

		public async Task<ImmutableList<KawalCovid19idDailyStatistics>> GetDailyStatisticsAsync(CancellationToken cancellationToken) {
			return await _memoryCache.GetOrCreateAsync("KawalCovid19idDailyStatistics", async entry => {
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
				return await _client.GetDailyStatisticsAsync(cancellationToken).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		public async Task<ImmutableList<KawalCovid19idProvinceStatistics>> GetProvinceStatisticsAsync(CancellationToken cancellationToken) {
			return await _memoryCache.GetOrCreateAsync("KawalCovid19idProvinceStatustics", async entry => {
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
				return await _client.GetProvinceStatisticsAsync(cancellationToken).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}
	}
}
