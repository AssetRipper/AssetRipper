using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.LightmapSettings
{
	public sealed class EnlightenRendererInformation : IAsset, IDependent
	{
		/// <summary>
		/// Not Release
		/// </summary>
		public static bool HasGeometryHash(TransferInstructionFlags flags) => !flags.IsRelease();

		public void Read(AssetReader reader)
		{
			Renderer.Read(reader);
			DynamicLightmapSTInSystem.Read(reader);
			SystemId = reader.ReadInt32();
			InstanceHash.Read(reader);
			if (HasGeometryHash(reader.Flags))
			{
				GeometryHash.Read(reader);
			}
		}

		public void Write(AssetWriter writer)
		{
			Renderer.Write(writer);
			DynamicLightmapSTInSystem.Write(writer);
			writer.Write(SystemId);
			InstanceHash.Write(writer);
			if (HasGeometryHash(writer.Flags))
			{
				GeometryHash.Write(writer);
			}
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
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

		public int SystemId { get; set; }

		public const string RendererName = "renderer";
		public const string DynamicLightmapSTInSystemName = "dynamicLightmapSTInSystem";
		public const string SystemIdName = "systemId";
		public const string InstanceHashName = "instanceHash";
		public const string GeometryHashName = "geometryHash";

		public PPtr<Object.Object> Renderer = new();
		public Vector4f DynamicLightmapSTInSystem = new();
		public Hash128 InstanceHash = new();
		public Hash128 GeometryHash = new();
	}
}
