using System;
using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct AnimationClipBindingConstant : IAssetReadable, IYAMLExportable, IDependent
	{
		public AnimationClipBindingConstant(bool _)
		{
			m_genericBindings = Array.Empty<GenericBinding>();
			m_pptrCurveMapping = Array.Empty<PPtr<Object>>();
		}

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreater(2017);
		}

		public GenericBinding FindBinding(int index)
		{
			int curves = 0;
			for (int i = 0; i < GenericBindings.Count; i++)
			{
				GenericBinding gb = GenericBindings[i];
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

		public bool IsAvatarMatch(Avatar avatar)
		{
			foreach (GenericBinding binding in GenericBindings)
			{
				if (!avatar.TOS.ContainsKey(binding.Path))
				{
					return false;
				}
			}
			return true;
		}

		public void Read(AssetReader reader)
		{
			m_genericBindings = reader.ReadAssetArray<GenericBinding>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			m_pptrCurveMapping = reader.ReadAssetArray<PPtr<Object>>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		public IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			return context.FetchDependencies(PptrCurveMapping, PptrCurveMappingName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(GenericBindingsName, GenericBindings.ExportYAML(container));
			node.Add(PptrCurveMappingName, PptrCurveMapping.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<GenericBinding> GenericBindings => m_genericBindings;
		public IReadOnlyList<PPtr<Object>> PptrCurveMapping => m_pptrCurveMapping;

		public const string GenericBindingsName = "genericBindings";
		public const string PptrCurveMappingName = "pptrCurveMapping";

		private GenericBinding[] m_genericBindings;
		private PPtr<Object>[] m_pptrCurveMapping;
	}
}
