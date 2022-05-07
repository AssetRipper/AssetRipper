using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Structure.Assembly.Serializable
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

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Pointer, string.Empty);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			return Pointer.ExportYaml(container);
		}

		public PPtr<IUnityObjectBase> Pointer = new();
	}
}
