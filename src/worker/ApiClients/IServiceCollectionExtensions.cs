using Microsoft.Extensions.DependencyInjection;

namespace Covid19id.ApiClients {
	public static class IServiceCollectionExtensions {
		public static void AddApiClients(this IServiceCollection services) {
			services.AddTransient<KawalCovid19idClient>();
		}
	}
}
