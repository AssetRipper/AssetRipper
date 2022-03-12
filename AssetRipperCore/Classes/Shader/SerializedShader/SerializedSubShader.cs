using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedSubShader : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 2021.2.0a17 and greater and Not Release
		/// </summary>
		private static bool HasSerializedPackageRequirements(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(2021, 2, 0, UnityVersionType.Alpha, 17);

		public void Read(AssetReader reader)
		{
			Passes = reader.ReadAssetArray<SerializedPass>();
			Tags.Read(reader);
			LOD = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Passes", Passes.ExportYAML(container));
			node.Add("m_Tags", Tags.ExportYAML(container));
			node.Add("m_LOD", LOD);

			// Editor Only
			if (HasSerializedPackageRequirements(container.ExportVersion, container.ExportFlags))
			{
				node.Add("m_PackageRequirements", new SerializedPackageRequirements().ExportYAML(container));
			}
			return node;
		}

		public SerializedPass[] Passes { get; set; }
		public int LOD { get; set; }

		public SerializedTagMap Tags = new();
	}
}
