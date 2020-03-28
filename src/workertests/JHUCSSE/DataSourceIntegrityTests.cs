using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Covid19id.Models;
using Covid19id.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.JHUCSSE {
	[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
	public class DataSourceIntegrityTests : IClassFixture<Fixture> {
		private readonly Fixture _fixture;

		public DataSourceIntegrityTests(
			Fixture fixture
		) {
			_fixture = fixture;
		}

		[Fact]
		public async Task NothingHasBeenRenamedOrRemoved() {
			JHUCSSEServices jhucsseServices = _fixture.ServiceProvider.GetRequiredService<JHUCSSEServices>();
			JHUCSSEDailyReportCollection dailyReportCollection = await jhucsseServices.GetAllDailyReportsAsync(CancellationToken.None);
			ImmutableList<JHUCSSEDailyReport> allDailyReports = dailyReportCollection.Keys
				.OrderBy(utcDate => utcDate)
				.Select(utcDate => dailyReportCollection[utcDate])
				.ToImmutableList();
			for (int i = 1; i < allDailyReports.Count; i++) {
				JHUCSSEDailyReport dailyReport = allDailyReports[i];
				JHUCSSEDailyReport prevReport = allDailyReports[i - 1];
				ImmutableList<Country> countries = dailyReport.GetCountries();
				ImmutableList<Country> prevCountries = dailyReport.GetCountries();
				foreach(Country country in prevCountries) {
					countries.Should().ContainSingle(c => c.Name == country.Name, "{0} in {1} should not be renamed or removed.", country.Name, prevReport.UtcDate.ToString("MM-dd-yyyy"));
				}
			}
		}
	}
}
