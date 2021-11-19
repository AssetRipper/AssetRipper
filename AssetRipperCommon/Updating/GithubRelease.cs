using System.Text.Json.Serialization;

namespace AssetRipper.Core.Updating
{
	public class GithubRelease
	{
		[JsonInclude, JsonPropertyName("tag_name")]
		public string TagName;
	}
}