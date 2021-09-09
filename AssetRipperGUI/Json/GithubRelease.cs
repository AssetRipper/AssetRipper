using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Json
{
	public class GithubRelease
	{
		[JsonInclude, JsonPropertyName("tag_name")]
		public string TagName;
	}
}