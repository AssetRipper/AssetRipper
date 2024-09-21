using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Web;

[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(byte[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
