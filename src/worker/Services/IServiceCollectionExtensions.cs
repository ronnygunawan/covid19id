using Microsoft.Extensions.DependencyInjection;

namespace Covid19id.Services {
	public static class IServiceCollectionExtensions {
		public static void AddServices(this IServiceCollection services) {
			services.AddTransient<JHUCSSEServices>();
		}
	}
}
