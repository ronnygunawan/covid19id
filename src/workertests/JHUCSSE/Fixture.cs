using System;
using Covid19id.ApiCaches;
using Covid19id.ApiClients;
using Covid19id.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.JHUCSSE {
	public class Fixture {
		public IServiceProvider ServiceProvider { get; }

		public Fixture() {
			ServiceCollection services = new ServiceCollection();
			services.AddMemoryCache();
			services.AddApiClients();
			services.AddApiCaches();
			services.AddServices();
			ServiceProvider = services.BuildServiceProvider();
		}
	}
}
