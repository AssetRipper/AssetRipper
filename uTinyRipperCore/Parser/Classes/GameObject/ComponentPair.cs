using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.GameObjects
{
	public struct ComponentPair : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool IsReadClassID(Version version)
		{
			return version.IsLess(5, 5);
		}

		public void Read(AssetReader reader)
		{
			if (IsReadClassID(reader.Version))
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

		public IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			Component comp = Component.FindAsset(context.File);
			if (comp == null)
			{
				if (context.IsLog)
				{
					ClassIDType assetType = context.File.GetAssetType(Component.PathID);
					Logger.Log(LogType.Debug, LogCategory.Export, $"GameObject's component {assetType}{Component} isn't implemeneted yet");
				}
			}
			else
			{
				yield return comp;
			}
		}

		public ClassIDType ClassID { get; private set; }

		public const string ComponentName = "component";

		public PPtr<Component> Component;
	}
}
