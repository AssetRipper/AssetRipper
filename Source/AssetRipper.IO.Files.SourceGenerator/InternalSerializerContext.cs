using AssetRipper.IO.Files.SourceGenerator.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.IO.Files.SourceGenerator;

[JsonSerializable(typeof(TypeDeclaration))]
[JsonSerializable(typeof(TypeDefinition))]
internal partial class InternalSerializerContext : JsonSerializerContext
{
}
