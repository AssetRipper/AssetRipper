using AssetRipper.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.AnimationClip.GenericBinding;
using AssetRipper.Classes.Misc;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using AssetRipper.YAML;
using System;
using System.Collections.Generic;
using Version = AssetRipper.Parser.Files.Version;

namespace AssetRipper.Classes.AnimationClip
{
	public struct AnimationClipBindingConstant : IAssetReadable, IYAMLExportable, IDependent
	{
		public AnimationClipBindingConstant(bool _)
		{
			GenericBindings = Array.Empty<GenericBinding.GenericBinding>();
			PPtrCurveMapping = Array.Empty<PPtr<Object.Object>>();
		}

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreater(2017);

		public GenericBinding.GenericBinding FindBinding(int index)
		{
			int curves = 0;
			for (int i = 0; i < GenericBindings.Length; i++)
			{
				GenericBinding.GenericBinding gb = GenericBindings[i];
				if (gb.ClassID == ClassIDType.Transform)
				{
					curves += gb.TransformType.GetDimension();
				}
				else
				{
					curves += 1;
				}

				if (curves > index)
				{
					return gb;
				}
			}
			throw new ArgumentException($"Binding with index {index} hasn't been found", nameof(index));
		}

		public void Read(AssetReader reader)
		{
			GenericBindings = reader.ReadAssetArray<GenericBinding.GenericBinding>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			PPtrCurveMapping = reader.ReadAssetArray<PPtr<Object.Object>>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}
		}

		public IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			return context.FetchDependencies(PPtrCurveMapping, PptrCurveMappingName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(GenericBindingsName, GenericBindings.ExportYAML(container));
			node.Add(PptrCurveMappingName, PPtrCurveMapping.ExportYAML(container));
			return node;
		}

		public GenericBinding.GenericBinding[] GenericBindings { get; set; }
		public PPtr<Object.Object>[] PPtrCurveMapping { get; set; }

		public const string GenericBindingsName = "genericBindings";
		public const string PptrCurveMappingName = "pptrCurveMapping";
	}
}
