using AssetRipper.Core.Classes.AnimationClip.GenericBinding;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;


namespace AssetRipper.Core.Classes.AnimationClip
{
	public sealed class AnimationClipBindingConstant : IAssetReadable, IYAMLExportable, IDependent
	{
		public AnimationClipBindingConstant() { }
		public AnimationClipBindingConstant(bool _)
		{
			GenericBindings = Array.Empty<GenericBinding.GenericBinding>();
			PPtrCurveMapping = Array.Empty<PPtr<Object.Object>>();
		}

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreater(2017);

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

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
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
