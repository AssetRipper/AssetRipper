using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeShaderCompilationContext :  IYAMLExportable
	{
		public static bool HasIsEditor(UnityVersion version) => (version.IsGreaterEqual(2020, 2, 4, UnityVersionType.Final, 1) && version.IsLess(2021, 1, 0, UnityVersionType.Alpha, 2)) ||
		                                                        version.IsGreaterEqual(2021, 1, 0, UnityVersionType.Alpha, 7);

		public static bool HasDxcAPI(UnityVersion version) => version.IsGreaterEqual(2021, 1, 0, UnityVersionType.Alpha, 2);
		public static bool HasApiMask(UnityVersion version) => version.IsLess(2021, 2, 0, UnityVersionType.Alpha, 8);
		public static bool HasPlatformGroup(UnityVersion version) => version.IsGreaterEqual(2021, 2, 0, UnityVersionType.Final, 1) && version.IsLess(2022, 1, 0, UnityVersionType.Alpha, 7);
		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();

			YAMLMappingNode buildTargetNode = new YAMLMappingNode();
			buildTargetNode.Add("platform", default(int));
			buildTargetNode.Add("subTarget",  default(int));
			buildTargetNode.Add("extendedPlatform",  default(int));
			if (HasIsEditor(container.Version))
			{
				buildTargetNode.Add("isEditor",  default(int));
			}
			node.Add("buildTarget", buildTargetNode);

			if (HasPlatformGroup(container.Version))
			{
				node.Add("platformGroup", default(int));
			}
			node.Add("sourceFileName", string.Empty);
			node.Add("source", string.Empty);
			node.Add("sourceFile", string.Empty);
			node.Add("kernels", Array.Empty<string>().ExportYAML());
			node.Add("kernelMacros", Array.Empty<string[]>().ExportYAML());
			node.Add("compilationFlags", default(int));
			if (HasApiMask(container.Version))
			{
				node.Add("apiMask", default(uint));
			}
			node.Add("supportedAPIs", default(uint));
			if (HasDxcAPI(container.Version))
			{
				buildTargetNode.Add("useDxcAPIs",  default(uint));
				buildTargetNode.Add("neverUseDxcAPIs",  default(uint));
			}
			node.Add("includeHash0", default(uint));
			node.Add("includeHash1", default(uint));
			node.Add("includeHash2", default(uint));
			node.Add("includeHash3", default(uint));
			node.Add("includeFiles", Array.Empty<string>().ExportYAML());

			return node;
		}
	}
}
