using System.Collections.Generic;
using AssetRipper.Classes;
using AssetRipper.Converters;
using AssetRipper.YAML;

using Object = AssetRipper.Classes.Object;

namespace AssetRipper.Game.Assembly
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
