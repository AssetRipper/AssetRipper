using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.GameObjects
{
	public struct ComponentPair : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool HasClassID(Version version) => version.IsLess(5, 5);

		public void Read(AssetReader reader)
		{
			if (HasClassID(reader.Version))
			{
				ClassID = (ClassIDType)reader.ReadInt32();
			}
			Component.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ComponentName, Component.ExportYAML(container));
			return node;
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Component, ComponentName);
		}

		public ClassIDType ClassID { get; set; }

		public const string ComponentName = "component";

		public PPtr<Component> Component;
	}
}
