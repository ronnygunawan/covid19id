using System;
using System.Collections.Immutable;
using System.Diagnostics;
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
	public class HistoricalDataIntegrityTests : IClassFixture<Fixture> {
		private readonly Fixture _fixture;

		public HistoricalDataIntegrityTests(
			Fixture fixture
		) {
			_fixture = fixture;
		}

		[Fact]
		public async Task NoRegressionInHistoricalData() {
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
				ImmutableList<Country> prevCountries = prevReport.GetCountries();

				foreach(Country country in prevCountries) {
					countries
						.Should().ContainSingle(c => c.Name == country.Name,
							because: "\n{0} in {1}.csv should not be renamed or removed.", country.Name, prevReport.UtcDate.ToString("MM-dd-yyyy"));

					if (country.Admin1s != null) {
						foreach (Admin1 admin1 in country.Admin1s) {
							countries
								.Single(c => c.Name == country.Name)
								.Admin1s
								.Should().NotBeNull(because: "\nAdmin1s of {0} in {1}.csv should not be removed.", country.Name, prevReport.UtcDate.ToString("MM-dd-yyyy"));

							countries
								.Single(c => c.Name == country.Name)
								.Admin1s
								.Should().ContainSingle(a1 => a1.Name == admin1.Name,
									because: "\n{0}, {1} in {2}.csv should not be renamed or removed.", admin1.Name, country.Name, prevReport.UtcDate.ToString("MM-dd-yyyy"));

							if (admin1.Admin2s != null) {
								foreach (Admin2 admin2 in admin1.Admin2s) {
									countries
										.Single(c => c.Name == country.Name)
										.Admin1s
										.Single(a1 => a1.Name == admin1.Name)
										.Admin2s
										.Should().NotBeNull(because: "\nAdmin2s of {0}, {1} in {2}.csv should not be removed.", admin1.Name, country.Name, prevReport.UtcDate.ToString("MM-dd-yyyy"));

									countries
										.Single(c => c.Name == country.Name)
										.Admin1s
										.Single(a1 => a1.Name == admin1.Name)
										.Admin2s
										.Should().ContainSingle(a2 => a2.Name == admin2.Name,
											because: "\n{0}, {1}, {2} in {3}.csv should not be renamed or removed.", admin2.Name, admin1.Name, country.Name, prevReport.UtcDate.ToString("MM-dd-yyyy"));
								}
							}
						}
					}
				}
			}
		}

		[Theory]
		[InlineData(1, 22, "China", "Hubei", 444, 17, 28, null)]
		[InlineData(3, 3, "US", "New York", 2, 0, 0, null)]
		public async Task KnownAdmin1DataVerified(int month, int day, string country, string admin1, int confirmed, int deaths, int recovered, int? active) {
			JHUCSSEServices jhucsseServices = _fixture.ServiceProvider.GetRequiredService<JHUCSSEServices>();
			JHUCSSEHistoricalReport admin1HistoricalReport = await jhucsseServices.GetAdmin1HistoricalReportAsync(country, admin1, CancellationToken.None);
			DateTime utcDate = new DateTime(2020, month, day, 0, 0, 0, DateTimeKind.Utc);
			JHUCSSEDailyStatistics datum = admin1HistoricalReport.TimeSeries.Single(datum => datum.UtcDate == utcDate);
			datum.Confirmed.Should().Be(confirmed);
			datum.Deaths.Should().Be(deaths);
			datum.Recovered.Should().Be(recovered);
			datum.Active.Should().Be(active);
		}

		[Fact]
		[SuppressMessage("Performance", "RG0001:Do not await inside a loop.", Justification = "<Pending>")]
		public async Task SumOfAllCountriesEqualsWorld() {
			JHUCSSEServices jhucsseServices = _fixture.ServiceProvider.GetRequiredService<JHUCSSEServices>();
			JHUCSSEHistoricalReport worldHistoricalReport = await jhucsseServices.GetWorldHistoricalReportAsync(CancellationToken.None);
			ImmutableList<Country> countries = await jhucsseServices.GetAllCountriesAsync(CancellationToken.None);

			foreach (DateTime utcDate in from datum in worldHistoricalReport.TimeSeries
										 orderby datum.UtcDate
										 select datum.UtcDate) {
				int totalConfirmed = 0;
				int totalDeaths = 0;
				int totalRecovered = 0;
				int? totalActive = null;

				Debug.WriteLine($"SumOfAllCountriesEqualsWorld: {utcDate}");

				foreach (Country country in countries) {
					JHUCSSEHistoricalReport countryHistoricalReport = await jhucsseServices.GetCountryHistoricalReportAsync(country.Name, CancellationToken.None);
					JHUCSSEDailyStatistics? countryDatum = countryHistoricalReport.TimeSeries.FirstOrDefault(datum => datum.UtcDate == utcDate);
					if (countryDatum != null) {
						totalConfirmed += countryDatum.Confirmed;
						totalDeaths += countryDatum.Deaths;
						totalRecovered += countryDatum.Recovered;
						if (countryDatum.Active.HasValue) {
							if (totalActive.HasValue) {
								totalActive += countryDatum.Active.Value;
							} else {
								totalActive = countryDatum.Active;
							}
						}
					}
				}

				JHUCSSEDailyStatistics worldDatum = worldHistoricalReport.TimeSeries.Single(datum => datum.UtcDate == utcDate);
				worldDatum.Confirmed.Should().Be(totalConfirmed,
					because: "Sum of all countries' confirmed cases should be equal to world's confirmed cases on {0}.", utcDate.ToString("MM-dd-yyyy"));
				worldDatum.Deaths.Should().Be(totalDeaths,
					because: "Sum of all countries' number of deaths should be equal to world's number of deaths on {0}.", utcDate.ToString("MM-dd-yyyy"));
				worldDatum.Recovered.Should().Be(totalRecovered,
					because: "Sum of all countries' number of recoveries should be equal to world's number of recoveries on {0}.", utcDate.ToString("MM-dd-yyyy"));
				worldDatum.Active.Should().Be(totalActive,
					because: "Sum of all countries' active cases should be equal to world's active cases on {0}.", utcDate.ToString("MM-dd-yyyy"));
			}
		}
	}
}
