using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeShaderPlatformVariant : IAssetReadable, IYAMLExportable
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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("targetRenderer", TargetRenderer);
			node.Add("targetLevel", TargetLevel);
			node.Add("kernels", HasKernelParent(container.Version) ? KernelsParent.ExportYAML(container) : Kernels.ExportYAML(container));
			node.Add("constantBuffers", ConstantBuffers.ExportYAML(container));
			node.Add("resourcesResolved", ResourcesResolved);
			//Editor-only
			node.Add("compilerPlatform", default(int));
			if (HasKernelParent(container.Version))
			{
				YAMLMappingNode fixedBitsetNode = new YAMLMappingNode();
				node.Add("Array", Array.Empty<uint>().ExportYAML(false));
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
