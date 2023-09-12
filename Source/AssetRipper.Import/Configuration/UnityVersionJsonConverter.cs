using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.Import.Configuration
{
    public class UnityVersionJsonConverter : JsonConverter<UnityVersion>
    {
        public override UnityVersion Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            string? input = reader.GetString();
            return input != null ? UnityVersion.Parse(input) : new UnityVersion();
        }

        public override void Write(
            Utf8JsonWriter writer,
            UnityVersion unityVersion,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(unityVersion.ToString());
    }
}
