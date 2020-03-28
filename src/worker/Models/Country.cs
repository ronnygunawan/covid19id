using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Covid19id.Models {
	[JsonObject(MemberSerialization.OptIn)]
	public class Country {
		[JsonProperty("name")]
		public string Name { get; }

		[JsonProperty("admin1s", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public ImmutableList<Admin1>? Admin1s { get; }

		[JsonConstructor]
		public Country(
			string name,
			ImmutableList<Admin1>? admin1s
		) {
			Name = name;
			Admin1s = admin1s;
		}
	}
}
