using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct AnimationClipBindingConstant : IAssetReadable, IYAMLExportable, IDependent
	{
		public AnimationClipBindingConstant(bool _)
		{
			m_genericBindings = new GenericBinding[0];
			m_pptrCurveMapping = new PPtr<Object>[0];
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

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (PPtr<Object> ptr in m_pptrCurveMapping)
			{
				yield return ptr.FetchDependency(file, isLog, () => nameof(AnimationClipBindingConstant), "pptrCurveMapping");
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("genericBindings", GenericBindings.ExportYAML(container));
			node.Add("pptrCurveMapping", PptrCurveMapping.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<GenericBinding> GenericBindings => m_genericBindings;
		public IReadOnlyList<PPtr<Object>> PptrCurveMapping => m_pptrCurveMapping;
		
		private GenericBinding[] m_genericBindings;
		private PPtr<Object>[] m_pptrCurveMapping;
	}
}
