using AssetRipper.Converters.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.Parser.IO.Asset.Writer;
using AssetRipper.YAML;
using System.Collections.Generic;
using Object = AssetRipper.Parser.Classes.Object.Object;

namespace AssetRipper.Structure.GameCollection.Assembly.Serializable
{
	public sealed class SerializablePointer : IAsset, IDependent
	{
		public void Read(AssetReader reader)
		{
			Pointer.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			Pointer.Write(writer);
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Pointer, string.Empty);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			return Pointer.ExportYAML(container);
		}

		public PPtr<Object> Pointer;
	}
}
