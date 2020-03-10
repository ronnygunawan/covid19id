using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Covid19id.Services {
	public static class IServiceCollectionExtensions {
		public static void AddServices(this IServiceCollection services) {
			services.AddSingleton<ClientSecrets>(serviceProvider => {
				IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
				return new ClientSecrets {
					ClientId = configuration["GoogleSheets:ClientId"],
					ClientSecret = configuration["GoogleSheets:ClientSecret"]
				};
			});
			services.AddTransient<KawalCovid19SheetServices>();
		}
	}
}
