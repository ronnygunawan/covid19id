namespace Covid19id.Models {
	public enum JHUCSSEReportVersion {
		/// <summary>
		/// Jan 22, 2020 - Admin1, Country, LastUpdate, Confirmed, Deaths, Recovered
		/// </summary>
		Original = 0,

		/// <summary>
		/// March 1, 2020 - Admin1, Country, LastUpdate, Confirmed, Deaths, Recovered, Latitude, Longitude
		/// </summary>
		AddedLatLong = 1,

		/// <summary>
		/// March 22, 2020 - FIPS, Admin2, Admin1, Country, LastUpdate, Latitude, Longitude, Confirmed, Deaths, Recovered, Active, Key
		/// </summary>
		AddedAdmin2FIPSActive = 2,
	}
}
