using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeShader : NamedObject
	{
		public ComputeShader(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			return 1;
		}

		public static bool HasComputeShaderVariant(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final, 4);
		public static bool HasEditorOnlyVariant(UnityVersion version) => version.IsGreaterEqual(2017, 2, 0, UnityVersionType.Beta, 2) && version.IsLess(2021, 2, 0, UnityVersionType.Alpha, 19);
		public static bool EditorOnlyVariantNewName(UnityVersion version) => version.IsGreaterEqual(2018, 1, 0, UnityVersionType.Beta, 11);
		public static bool HasComputeShaderPlatformVariant(UnityVersion version) => version.IsGreaterEqual(2020, 1, 0, UnityVersionType.Alpha, 9);
		public static bool HasComputeShaderCompilationContext(UnityVersion version) => version.IsGreaterEqual(2020, 1, 0, UnityVersionType.Alpha, 17);
		public static bool HasPreprocessorOverride(UnityVersion version) => version.IsGreaterEqual(2020, 2, 0, UnityVersionType.Alpha, 7);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasComputeShaderPlatformVariant(reader.Version))
			{
				PlatformVariants = reader.ReadAssetArray<ComputeShaderPlatformVariant>();
			}
			else if (HasComputeShaderVariant(reader.Version))
			{
				Variants = reader.ReadAssetArray<ComputeShaderVariant>();
			}
			else
			{
				Kernels = reader.ReadAssetArray<ComputeShaderKernel>();
			}

			reader.AlignStream();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (HasComputeShaderPlatformVariant(container.Version))
			{
				node.Add("variants", PlatformVariants.ExportYAML(container));
			}
			else if (HasComputeShaderVariant(container.Version))
			{
				node.Add("variants", Variants.ExportYAML(container));
			}
			else
			{
				node.Add("kernels", Kernels.ExportYAML(container));
			}
			//Editor-Only
			if (HasComputeShaderCompilationContext(container.Version))
			{
				node.Add("m_CompilationContext", new ComputeShaderCompilationContext().ExportYAML(container));
			}
			node.Add("errors" , (new HashSet<ShaderError>()).ExportYAML(container));
			if (HasPreprocessorOverride(container.Version))
			{
				node.Add("m_PreprocessorOverride", default(int));
			}
			if (HasEditorOnlyVariant(container.Version))
			{
				node.Add(EditorOnlyVariantNewName(container.Version) ? "m_HasEditorOnlyVariant" : "m_hasEditorOnlyVariant", false);
			}


			return node;
		}

		/// <summary>
		/// Greater or equal to 5.0.0f4 and less than 2020.1.0a9
		/// </summary>
		public ComputeShaderVariant[] Variants { get; set; }

		/// <summary>
		/// Greater or equal to 2020.1.0a9
		/// </summary>
		public ComputeShaderPlatformVariant[] PlatformVariants { get; set; }

		/// <summary>
		/// Less than 5.0.0f4
		/// </summary>
		public ComputeShaderKernel[] Kernels { get; set; }
	}
}
