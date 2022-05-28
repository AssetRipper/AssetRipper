using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;

using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeShaderVariant : IAssetReadable, IYamlExportable
	{
		public static bool HasResourcesResolved(UnityVersion version) => version.IsGreaterEqual(5, 1, 0, UnityVersionType.Final, 3);

		public void Read(AssetReader reader)
		{
			TargetRenderer = reader.ReadInt32();
			TargetLevel = reader.ReadInt32();
			Kernels = reader.ReadAssetArray<ComputeShaderKernel>();
			reader.AlignStream();
			ConstantBuffers = reader.ReadAssetArray<ComputeShaderCB>();
			reader.AlignStream();
			if (HasResourcesResolved(reader.Version))
			{
				ResourcesResolved = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("targetRenderer", TargetRenderer);
			node.Add("targetLevel", TargetLevel);
			node.Add("kernels", Kernels.ExportYaml(container));
			node.Add("constantBuffers", ConstantBuffers.ExportYaml(container));
			if (HasResourcesResolved(container.Version))
			{
				node.Add("resourcesResolved", ResourcesResolved);
			}

			return node;
		}

		public int TargetRenderer { get; set; }
		public int TargetLevel { get; set; }
		public ComputeShaderKernel[] Kernels { get; set; }
		public ComputeShaderCB[] ConstantBuffers { get; set; }
		public bool ResourcesResolved { get; set; }
	}
}
