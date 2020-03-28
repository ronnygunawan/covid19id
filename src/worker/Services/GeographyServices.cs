using System.Collections.Generic;

namespace Covid19id.Services {
	public static class GeographyServices {
		public static string GetUSStateName(string code) {
			switch (code) {
				case "AL": return "Alabama";
				case "AK": return "Alaska";
				case "AZ": return "Arizona";
				case "AR": return "Arkansas";
				case "CA": return "California";
				case "CO": return "Colorado";
				case "CT": return "Connecticut";
				case "DE": return "Delaware";
				case "DC": return "District of Columbia";
				case "FL": return "Florida";
				case "GA": return "Georgia";
				case "HI": return "Hawaii";
				case "ID": return "Idaho";
				case "IL": return "Illinois";
				case "IN": return "Indiana";
				case "IA": return "Iowa";
				case "KS": return "Kansas";
				case "KY": return "Kentucky";
				case "LA": return "Louisiana";
				case "ME": return "Maine";
				case "MD": return "Maryland";
				case "MA": return "Massachusetts";
				case "MI": return "Michigan";
				case "MN": return "Minnesota";
				case "MS": return "Mississippi";
				case "MO": return "Missouri";
				case "MT": return "Montana";
				case "NE": return "Nebraska";
				case "NV": return "Nevada";
				case "NH": return "New Hampshire";
				case "NJ": return "New Jersey";
				case "NM": return "New Mexico";
				case "NY": return "New York";
				case "NC": return "North Carolina";
				case "ND": return "North Dakota";
				case "OH": return "Ohio";
				case "OK": return "Oklahoma";
				case "OR": return "Oregon";
				case "PA": return "Pennsylvania";
				case "RI": return "Rhode Island";
				case "SC": return "South Carolina";
				case "SD": return "South Dakota";
				case "TN": return "Tennessee";
				case "TX": return "Texas";
				case "UT": return "Utah";
				case "VT": return "Vermont";
				case "VA": return "Virginia";
				case "WA": return "Washington";
				case "WV": return "West Virginia";
				case "WI": return "Wisconsin";
				case "WY": return "Wyoming";
				default:
					throw new KeyNotFoundException($"State name not found for {code}.");
			}
		}

		public static string GetCanadaStateName(string code) {
			switch (code) {
				case "NL": return "Newfoundland and Labrador";
				case "PE": return "Prince Edward Island";
				case "NS": return "Nova Scotia";
				case "NB": return "New Brunswick";
				case "QC": return "Quebec";
				case "ON": return "Ontario";
				case "MB": return "Manitoba";
				case "SK": return "Saskatchewan";
				case "AB": return "Alberta";
				case "59": return "British Columbia";
				case "YT": return "Yukon";
				case "NT": return "Northwest Territories";
				case "NU": return "Nunavut";
				default:
					throw new KeyNotFoundException($"State name not found for {code}.");
			}
		}
	}
}
