using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters.TerrainDatas;
using uTinyRipper.Converters;
using uTinyRipper;

namespace uTinyRipper.Classes.TerrainDatas
{
	public struct Heightmap : IAsset, IDependent
	{
		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2018, 3))
			{
				return 3;
			}
			if (version.IsGreaterEqual(5, 5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2.1.0 to 2.6.0 exclusive
		/// </summary>
		public static bool HasShifts(Version version) => version.IsGreaterEqual(2, 1) && version.IsLess(2, 6);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasDefaultPhysicMaterial(Version version) => version.IsLess(5);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasThickness(Version version) => version.IsGreaterEqual(5);

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool HasAlign(Version version) => version.IsGreaterEqual(2, 1);

		public Heightmap Convert(IExportContainer container)
		{
			return HeightmapConverter.Convert(container, ref this);
		}

		public void Read(AssetReader reader)
		{
			Heights = reader.ReadInt16Array();
			if (HasAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			if (HasShifts(reader.Version))
			{
				Shifts = reader.ReadAssetArray<Shift>();
				reader.AlignStream(AlignType.Align4);
			}

			PrecomputedError = reader.ReadSingleArray();
			MinMaxPatchHeights = reader.ReadSingleArray();
			if (HasDefaultPhysicMaterial(reader.Version))
			{
				DefaultPhysicMaterial.Read(reader);
			}

			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			if (HasThickness(reader.Version))
			{
				Thickness = reader.ReadSingle();
			}

			Levels = reader.ReadInt32();
			Scale.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			Heights.Write(writer);
			if (HasAlign(writer.Version))
			{
				writer.AlignStream(AlignType.Align4);
			}
			if (HasShifts(writer.Version))
			{
				Shifts.Write(writer);
				writer.AlignStream(AlignType.Align4);
			}

			PrecomputedError.Write(writer);
			MinMaxPatchHeights.Write(writer);
			if (HasDefaultPhysicMaterial(writer.Version))
			{
				DefaultPhysicMaterial.Write(writer);
			}

			writer.Write(Width);
			writer.Write(Height);
			if (HasThickness(writer.Version))
			{
				writer.Write(Thickness);
			}

			writer.Write(Levels);
			Scale.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(HeightsName, Heights.ExportYAML(true));
			if (HasShifts(container.ExportVersion))
			{
				node.Add(ShiftsName, Shifts.ExportYAML(container));
			}

			node.Add(PrecomputedErrorName, PrecomputedError.ExportYAML());
			node.Add(MinMaxPatchHeightsName, MinMaxPatchHeights.ExportYAML());
			if (HasDefaultPhysicMaterial(container.ExportVersion))
			{
				node.Add(DefaultPhysicMaterialName, DefaultPhysicMaterial.ExportYAML(container));
			}

			node.Add(WidthName, Width);
			node.Add(HeightName, Height);
			if (HasThickness(container.ExportVersion))
			{
				node.Add(ThicknessName, Thickness);
			}

			node.Add(LevelsName, Levels);
			node.Add(ScaleName, Scale.ExportYAML(container));
			return node;
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			if (HasDefaultPhysicMaterial(context.Version))
			{
				yield return context.FetchDependency(DefaultPhysicMaterial, DefaultPhysicMaterialName);
			}
		}

		public short[] Heights { get; set; }
		public Shift[] Shifts { get; set; }
		public float[] PrecomputedError { get; set; }
		public float[] MinMaxPatchHeights { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public float Thickness { get; set; }
		public int Levels { get; set; }

		public const string HeightsName = "m_Heights";
		public const string ShiftsName = "m_Shifts";
		public const string PrecomputedErrorName = "m_PrecomputedError";
		public const string MinMaxPatchHeightsName = "m_MinMaxPatchHeights";
		public const string DefaultPhysicMaterialName = "m_DefaultPhysicMaterial";
		public const string WidthName = "m_Width";
		public const string HeightName = "m_Height";
		public const string ThicknessName = "m_Thickness";
		public const string LevelsName = "m_Levels";
		public const string ScaleName = "m_Scale";

		public PPtr<PhysicMaterial> DefaultPhysicMaterial;
		public Vector3f Scale;
	}
}
