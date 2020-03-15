using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.ApiClients;
using Covid19id.Models;
using Covid19id.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Covid19id.ApiCaches {
	public class KawalCovid19idCache : IKawalCovid19id {
		private readonly IMemoryCache _memoryCache;
		private readonly KawalCovid19idClient _client;

		public KawalCovid19idCache(
			IMemoryCache memoryCache,
			KawalCovid19idClient client
		) {
			_memoryCache = memoryCache;
			_client = client;
		}

		public async Task<ImmutableList<DailyStatistics>> GetDailyStatisticsAsync(CancellationToken cancellationToken) {
			return await _memoryCache.GetOrCreateAsync("KawalCovid19idDailyStatistics", async entry => {
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
				return await _client.GetDailyStatisticsAsync(cancellationToken).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}
	}
}
