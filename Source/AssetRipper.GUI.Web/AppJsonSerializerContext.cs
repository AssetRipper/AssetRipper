using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Web;

[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
