using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.TerrainDatas
{
	public struct Heightmap : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 2.1.0 to 2.6.0 exclusive
		/// </summary>
		public static bool IsReadShifts(Version version)
		{
			return version.IsGreaterEqual(2, 1) && version.IsLess(2, 6);
		}
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadDefaultPhysicMaterial(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadThickness(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsReadAlign(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(5, 5))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetStream stream)
		{
			m_heights = stream.ReadInt16Array();
			if (IsReadAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
			if (IsReadShifts(stream.Version))
			{
				m_shifts = stream.ReadArray<Shift>();
				stream.AlignStream(AlignType.Align4);
			}

			m_precomputedError = stream.ReadSingleArray();
			m_minMaxPatchHeights = stream.ReadSingleArray();

			if (IsReadDefaultPhysicMaterial(stream.Version))
			{
				DefaultPhysicMaterial.Read(stream);
			}
			Width = stream.ReadInt32();
			Height = stream.ReadInt32();
			if (IsReadThickness(stream.Version))
			{
				Thickness = stream.ReadSingle();
			}
			Levels = stream.ReadInt32();
			Scale.Read(stream);
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			if (IsReadDefaultPhysicMaterial(file.Version))
			{
				yield return DefaultPhysicMaterial.FetchDependency(file, isLog, () => nameof(Heightmap), "m_DefaultPhysicMaterial");
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Heights", Heights.ExportYAML(true));
			node.Add("m_PrecomputedError", PrecomputedError.ExportYAML());
			node.Add("m_MinMaxPatchHeights", MinMaxPatchHeights.ExportYAML());
			node.Add("m_Width", Width);
			node.Add("m_Height", Height);
			node.Add("m_Thickness", Thickness);
			node.Add("m_Levels", Levels);
			node.Add("m_Scale", Scale.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<short> Heights => m_heights;
		public IReadOnlyList<Shift> Shifts => m_shifts;
		public IReadOnlyList<float> PrecomputedError => m_precomputedError;
		public IReadOnlyList<float> MinMaxPatchHeights => m_minMaxPatchHeights;
		public int Width { get; private set; }
		public int Height { get; private set; }
		public float Thickness { get; private set; }
		public int Levels { get; private set; }

		public PPtr<PhysicMaterial> DefaultPhysicMaterial;
		public Vector3f Scale;

		private short[] m_heights;
		private Shift[] m_shifts;
		private float[] m_precomputedError;
		private float[] m_minMaxPatchHeights;
	}
}
