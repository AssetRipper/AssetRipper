using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedSubShader : IAssetReadable, IYamlExportable
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("m_Passes", Passes.ExportYaml(container));
			node.Add("m_Tags", Tags.ExportYaml(container));
			node.Add("m_LOD", LOD);

			// Editor Only
			if (HasSerializedPackageRequirements(container.ExportVersion, container.ExportFlags))
			{
				node.Add("m_PackageRequirements", new SerializedPackageRequirements().ExportYaml(container));
			}
			return node;
		}

		public SerializedPass[] Passes { get; set; }
		public int LOD { get; set; }

		public SerializedTagMap Tags = new();
	}
}
