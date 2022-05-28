using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeShaderPlatformVariant : IAssetReadable, IYamlExportable
	{
		public static bool HasKernelParent(UnityVersion version) => version.IsGreaterEqual(2020, 1, 0, UnityVersionType.Alpha, 9);
		public static bool HasPlatformKeywords(UnityVersion version) => version.IsLess(2021, 2, 0, UnityVersionType.Alpha, 19);

		public void Read(AssetReader reader)
		{
			TargetRenderer = reader.ReadInt32();
			TargetLevel = reader.ReadInt32();
			if (HasKernelParent(reader.Version))
			{
				KernelsParent = reader.ReadAssetArray<ComputeShaderKernelParent>();
			}
			else
			{
				Kernels = reader.ReadAssetArray<ComputeShaderKernel>();
			}
			reader.AlignStream();
			ConstantBuffers = reader.ReadAssetArray<ComputeShaderCB>();
			reader.AlignStream();
			ResourcesResolved = reader.ReadBoolean();
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("targetRenderer", TargetRenderer);
			node.Add("targetLevel", TargetLevel);
			node.Add("kernels", HasKernelParent(container.Version) ? KernelsParent.ExportYaml(container) : Kernels.ExportYaml(container));
			node.Add("constantBuffers", ConstantBuffers.ExportYaml(container));
			node.Add("resourcesResolved", ResourcesResolved);
			//Editor-only
			node.Add("compilerPlatform", default(int));
			if (HasKernelParent(container.Version))
			{
				YamlMappingNode fixedBitsetNode = new YamlMappingNode();
				node.Add("Array", Array.Empty<uint>().ExportYaml(false));
				node.Add("platformKeywords", fixedBitsetNode);
			}

			node.Add("needsReflectionData", false);

			return node;
		}

		public int TargetRenderer { get; set; }
		public int TargetLevel { get; set; }
		public ComputeShaderKernel[] Kernels { get; set; }
		public ComputeShaderKernelParent[] KernelsParent { get; set; }
		public ComputeShaderCB[] ConstantBuffers { get; set; }
		public bool ResourcesResolved { get; set; }
	}
}
