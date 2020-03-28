using Newtonsoft.Json;

namespace Covid19id.Models {
	[JsonObject(MemberSerialization.OptIn)]
	public class Admin2 {
		[JsonProperty("name")]
		public string Name { get; }

		[JsonConstructor]
		public Admin2(
			string name
		) {
			Name = name;
		}
	}
}
