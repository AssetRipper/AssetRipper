using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.GameObjects
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
			if(IsReadClassID(reader.Version))
			{
				ClassID = (ClassIDType)reader.ReadInt32();
			}
			Component.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("component", Component.ExportYAML(container));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			Component comp = Component.FindAsset(file);
			if(comp == null)
			{
				if(isLog)
				{
					ClassIDType compType = file.GetClassID(Component.PathID);
					Logger.Log(LogType.Debug, LogCategory.Export, $"GameObject's component {Component}[{compType}] isn't implemeneted yet");
				}
			}
			else
			{
				yield return comp;
			}
		}
		
		public ClassIDType ClassID { get; private set; }

		public PPtr<Component> Component;
	}
}
