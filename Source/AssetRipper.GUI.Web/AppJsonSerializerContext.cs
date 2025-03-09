using AssetRipper.GUI.Web.Pages;
using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Web;

[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(byte[]))]
[JsonSerializable(typeof(Commands.PathFormData))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
