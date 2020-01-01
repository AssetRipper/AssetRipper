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
			// NODE: unknown version
			// Width and Height has been replaced by Resolution
			if (version.IsGreaterEqual(2019, 3, 0, VersionType.Beta))
			{
				return 5;
			}
			if (version.IsGreaterEqual(2019, 3))
			{
				return 4;
			}
			// Heightmap has been flipped?
			if (version.IsGreaterEqual(2018, 3))
			{
				return 3;
			}
			// PrecomputedError has been changed?
			if (version.IsGreaterEqual(5, 5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasHoles(Version version) => version.IsGreaterEqual(2019, 3);
		/// <summary>
		/// 2.1.0 to 2.6.0 exclusive
		/// </summary>
		public static bool HasShifts(Version version) => version.IsGreaterEqual(2, 1) && version.IsLess(2, 6);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasDefaultPhysicMaterial(Version version) => version.IsLess(5);
		/// <summary>
		/// Less than 2019.3
		/// </summary>
		public static bool HasWidth(Version version) => version.IsLess(2019, 3);
		/// <summary>
		/// 5.0.0 to 2019.3 exclusive
		/// </summary>
		public static bool HasThickness(Version version) => version.IsGreaterEqual(5) && version.IsLess(2019, 3);
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasResolution(Version version) => version.IsGreaterEqual(2019, 3);

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
			if (HasHoles(reader.Version))
			{
				Holes = reader.ReadByteArray();
				HolesLOD = reader.ReadByteArray();
				EnableHolesTextureCompression = reader.ReadBoolean();
			}
			if (HasAlign(reader.Version))
			{
				reader.AlignStream();
			}
			if (HasShifts(reader.Version))
			{
				Shifts = reader.ReadAssetArray<Shift>();
				reader.AlignStream();
			}

			PrecomputedError = reader.ReadSingleArray();
			MinMaxPatchHeights = reader.ReadSingleArray();
			if (HasDefaultPhysicMaterial(reader.Version))
			{
				DefaultPhysicMaterial.Read(reader);
			}

			if (HasWidth(reader.Version))
			{
				Width = reader.ReadInt32();
				Height = reader.ReadInt32();
			}
			if (HasThickness(reader.Version))
			{
				Thickness = reader.ReadSingle();
			}
			if (HasResolution(reader.Version))
			{
				Resolution = reader.ReadInt32();
			}

			Levels = reader.ReadInt32();
			Scale.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			Heights.Write(writer);
			if (HasHoles(writer.Version))
			{
				writer.Write(Holes);
				writer.Write(HolesLOD);
				writer.Write(EnableHolesTextureCompression);
			}
			if (HasAlign(writer.Version))
			{
				writer.AlignStream();
			}
			if (HasShifts(writer.Version))
			{
				Shifts.Write(writer);
				writer.AlignStream();
			}

			PrecomputedError.Write(writer);
			MinMaxPatchHeights.Write(writer);
			if (HasDefaultPhysicMaterial(writer.Version))
			{
				DefaultPhysicMaterial.Write(writer);
			}

			if (HasWidth(writer.Version))
			{
				writer.Write(Width);
				writer.Write(Height);
			}
			if (HasThickness(writer.Version))
			{
				writer.Write(Thickness);
			}
			if (HasResolution(writer.Version))
			{
				writer.Write(Resolution);
			}

			writer.Write(Levels);
			Scale.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(HeightsName, Heights.ExportYAML(true));
			if (HasHoles(container.ExportVersion))
			{
				node.Add(HolesName, Holes.ExportYAML());
				node.Add(HolesLODName, HolesLOD.ExportYAML());
				node.Add(EnableHolesTextureCompressionName, EnableHolesTextureCompression);
			}
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

			if (HasWidth(container.ExportVersion))
			{
				node.Add(WidthName, Width);
				node.Add(HeightName, Height);
			}
			if (HasThickness(container.ExportVersion))
			{
				node.Add(ThicknessName, Thickness);
			}
			if (HasResolution(container.ExportVersion))
			{
				node.Add(ResolutionName, Resolution);
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
		public byte[] Holes { get; set; }
		public byte[] HolesLOD { get; set; }
		public bool EnableHolesTextureCompression { get; set; }
		public Shift[] Shifts { get; set; }
		public float[] PrecomputedError { get; set; }
		public float[] MinMaxPatchHeights { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public float Thickness { get; set; }
		public int Resolution
		{
			get => Width;
			set => Width = value;
		}
		public int Levels { get; set; }

		public const string HeightsName = "m_Heights";
		public const string HolesName = "m_Holes";
		public const string HolesLODName = "m_HolesLOD";
		public const string EnableHolesTextureCompressionName = "m_EnableHolesTextureCompression";
		public const string ShiftsName = "m_Shifts";
		public const string PrecomputedErrorName = "m_PrecomputedError";
		public const string MinMaxPatchHeightsName = "m_MinMaxPatchHeights";
		public const string DefaultPhysicMaterialName = "m_DefaultPhysicMaterial";
		public const string WidthName = "m_Width";
		public const string HeightName = "m_Height";
		public const string ThicknessName = "m_Thickness";
		public const string ResolutionName = "m_Resolution";
		public const string LevelsName = "m_Levels";
		public const string ScaleName = "m_Scale";

		public PPtr<PhysicMaterial> DefaultPhysicMaterial;
		public Vector3f Scale;
	}
}
