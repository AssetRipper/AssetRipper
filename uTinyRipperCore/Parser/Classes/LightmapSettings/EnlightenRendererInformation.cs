using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct EnlightenRendererInformation : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadGeometryHash(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}

		public void Read(AssetReader reader)
		{
			Renderer.Read(reader);
			DynamicLightmapSTInSystem.Read(reader);
			SystemId = reader.ReadInt32();
			InstanceHash.Read(reader);
			if(IsReadGeometryHash(reader.Flags))
			{
				GeometryHash.Read(reader);
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Renderer.FetchDependency(file, isLog, () => nameof(EnlightenRendererInformation), "renderer");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("renderer", Renderer.ExportYAML(container));
			node.Add("dynamicLightmapSTInSystem", DynamicLightmapSTInSystem.ExportYAML(container));
			node.Add("systemId", SystemId);
			node.Add("instanceHash", InstanceHash.ExportYAML(container));
			node.Add("geometryHash", GeometryHash.ExportYAML(container));
			return node;
		}

		public int SystemId { get; private set; }

		public PPtr<Object> Renderer;
		public Vector4f DynamicLightmapSTInSystem;
		public Hash128 InstanceHash;
		public Hash128 GeometryHash;
	}
}
