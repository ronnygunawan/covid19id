using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Covid19id.ApiClients {
	public static class IServiceCollectionExtensions {
		public static void AddApiClients(this IServiceCollection services) {
			services.AddSingleton<HttpClient>();
			services.AddTransient<KawalCovid19idApiClient>();
			services.AddTransient<JHUCSSEApiClient>();
		}
	}
}
