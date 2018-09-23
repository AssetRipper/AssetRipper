using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.TerrainDatas
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

		public void Read(AssetReader reader)
		{
			m_heights = reader.ReadInt16Array();
			if (IsReadAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadShifts(reader.Version))
			{
				m_shifts = reader.ReadArray<Shift>();
				reader.AlignStream(AlignType.Align4);
			}

			m_precomputedError = reader.ReadSingleArray();
			m_minMaxPatchHeights = reader.ReadSingleArray();

			if (IsReadDefaultPhysicMaterial(reader.Version))
			{
				DefaultPhysicMaterial.Read(reader);
			}
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			if (IsReadThickness(reader.Version))
			{
				Thickness = reader.ReadSingle();
			}
			Levels = reader.ReadInt32();
			Scale.Read(reader);
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
