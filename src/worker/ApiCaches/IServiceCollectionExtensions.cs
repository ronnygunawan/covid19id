using Covid19id.Apis;
using Microsoft.Extensions.DependencyInjection;

namespace Covid19id.ApiCaches {
	public static class IServiceCollectionExtensions {
		public static void AddApiCaches(this IServiceCollection services) {
			services.AddTransient<IKawalCovid19idApi, KawalCovid19idApiCache>();
			services.AddTransient<IJHUCSSEApi, JHUCSSEApiCache>();
		}
	}
}
