using Covid19id.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Covid19id.ApiCaches {
	public static class IServiceCollectionExtensions {
		public static void AddApiCaches(this IServiceCollection services) {
			services.AddTransient<IKawalCovid19id, KawalCovid19idCache>();
		}
	}
}
