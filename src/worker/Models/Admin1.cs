using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Covid19id.Models {
	[JsonObject(MemberSerialization.OptIn)]
	public class Admin1 {
		[JsonProperty("name")]
		public string Name { get; }

		[JsonProperty("admin1s", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public ImmutableList<Admin2>? Admin2s { get; }

		[JsonConstructor]
		public Admin1(
			string name,
			ImmutableList<Admin2>? admin2s
		) {
			Name = name;
			Admin2s = admin2s;
		}
	}
}
