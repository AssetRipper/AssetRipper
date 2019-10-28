using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct EnlightenRendererInformation : IAssetReadable, IYAMLExportable, IDependent
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
			if (IsReadGeometryHash(reader.Flags))
			{
				GeometryHash.Read(reader);
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Renderer, RendererName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(RendererName, Renderer.ExportYAML(container));
			node.Add(DynamicLightmapSTInSystemName, DynamicLightmapSTInSystem.ExportYAML(container));
			node.Add(SystemIdName, SystemId);
			node.Add(InstanceHashName, InstanceHash.ExportYAML(container));
			node.Add(GeometryHashName, GeometryHash.ExportYAML(container));
			return node;
		}

		public int SystemId { get; private set; }

		public const string RendererName = "renderer";
		public const string DynamicLightmapSTInSystemName = "dynamicLightmapSTInSystem";
		public const string SystemIdName = "systemId";
		public const string InstanceHashName = "instanceHash";
		public const string GeometryHashName = "geometryHash";

		public PPtr<Object> Renderer;
		public Vector4f DynamicLightmapSTInSystem;
		public Hash128 InstanceHash;
		public Hash128 GeometryHash;
	}
}
