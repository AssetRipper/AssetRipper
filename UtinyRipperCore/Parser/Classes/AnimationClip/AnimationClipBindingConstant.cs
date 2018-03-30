using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct AnimationClipBindingConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreater(2017);
		}

		public void Read(AssetStream stream)
		{
			m_genericBindings = stream.ReadArray<GenericBinding>();
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			m_pptrCurveMapping = stream.ReadArray<PPtr<Object>>();
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("genericBindings", (GenericBindings == null) ? YAMLSequenceNode.Empty : GenericBindings.ExportYAML(exporter));
			node.Add("pptrCurveMapping", (PptrCurveMapping == null) ? YAMLSequenceNode.Empty : PptrCurveMapping.ExportYAML(exporter));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (PPtr<Object> ptr in m_pptrCurveMapping)
			{
				Object @object = ptr.FindObject(file);
				if(@object == null)
				{
					if(isLog)
					{
						Logger.Log(LogType.Warning, LogCategory.Export, $"AnimationClipBindingConstant's pptrCurveMapping {ptr.ToLogString(file)} wasn't found ");
					}
				}
				else
				{
					yield return @object;
				}
			}
		}

		public GenericBinding FindBinding(int index)
		{
			int curves = 0;
			for (int i = 0; i < GenericBindings.Count; i++)
			{
				GenericBinding gb = GenericBindings[i];
				if(gb.Attribute == 2) // Quaternion
				{
					curves += 4;
				}
				else if(gb.Attribute <= 4) // Vector3
				{
					curves += 3;
				}
				else // float
				{
					curves++;
				}
				if (curves > index)
				{
					return gb;
				}
			}

			return default;
		}

		public IReadOnlyList<GenericBinding> GenericBindings => m_genericBindings;
		public IReadOnlyList<PPtr<Object>> PptrCurveMapping => m_pptrCurveMapping;
		
		private GenericBinding[] m_genericBindings;
		private PPtr<Object>[] m_pptrCurveMapping;
	}
}
