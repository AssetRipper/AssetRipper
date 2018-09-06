using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct AnimationClipBindingConstant : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreater(2017);
		}

		public void Read(AssetReader reader)
		{
			m_genericBindings = reader.ReadArray<GenericBinding>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			m_pptrCurveMapping = reader.ReadArray<PPtr<Object>>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("genericBindings", (GenericBindings == null) ? YAMLSequenceNode.Empty : GenericBindings.ExportYAML(container));
			node.Add("pptrCurveMapping", (PptrCurveMapping == null) ? YAMLSequenceNode.Empty : PptrCurveMapping.ExportYAML(container));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (PPtr<Object> ptr in m_pptrCurveMapping)
			{
				yield return ptr.FetchDependency(file, isLog, () => nameof(AnimationClipBindingConstant), "pptrCurveMapping");
			}
		}

		public GenericBinding FindBinding(int index)
		{
			int curves = 0;
			for (int i = 0; i < GenericBindings.Count; i++)
			{
				GenericBinding gb = GenericBindings[i];
				if(gb.ClassID == ClassIDType.Transform)
				{
					curves += gb.BindingType.GetDimension();
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
			throw new ArgumentException($"Binding with index {index} wasn't found", nameof(index));
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

		public IReadOnlyList<GenericBinding> GenericBindings => m_genericBindings;
		public IReadOnlyList<PPtr<Object>> PptrCurveMapping => m_pptrCurveMapping;
		
		private GenericBinding[] m_genericBindings;
		private PPtr<Object>[] m_pptrCurveMapping;
	}
}
