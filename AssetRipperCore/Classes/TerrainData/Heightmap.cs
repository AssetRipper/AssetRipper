using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.TerrainData
{
	public sealed class Heightmap : IAsset, IDependent
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			// NODE: unknown version
			// Width and Height has been replaced by Resolution
			if (version.IsGreaterEqual(2019, 3, 0, UnityVersionType.Beta))
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
		public static bool HasHoles(UnityVersion version) => version.IsGreaterEqual(2019, 3);
		/// <summary>
		/// 2.1.0 to 2.6.0 exclusive
		/// </summary>
		public static bool HasShifts(UnityVersion version) => version.IsGreaterEqual(2, 1) && version.IsLess(2, 6);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasDefaultPhysicMaterial(UnityVersion version) => version.IsLess(5);
		/// <summary>
		/// Less than 2019.3
		/// </summary>
		public static bool HasWidth(UnityVersion version) => version.IsLess(2019, 3);
		/// <summary>
		/// 5.0.0 to 2019.3 exclusive
		/// </summary>
		public static bool HasThickness(UnityVersion version) => version.IsGreaterEqual(5) && version.IsLess(2019, 3);
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasResolution(UnityVersion version) => version.IsGreaterEqual(2019, 3);

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool HasAlign(UnityVersion version) => version.IsGreaterEqual(2, 1);

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
				Width = Resolution;
				Height = Resolution;
			}

			Levels = reader.ReadInt32();
			m_Scale.Read(reader);
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
			m_Scale.Write(writer);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(HeightsName, Heights.ExportYaml(true));
			if (HasHoles(container.ExportVersion))
			{
				node.Add(HolesName, Holes.ExportYaml());
				node.Add(HolesLODName, HolesLOD.ExportYaml());
				node.Add(EnableHolesTextureCompressionName, EnableHolesTextureCompression);
			}
			if (HasShifts(container.ExportVersion))
			{
				node.Add(ShiftsName, Shifts.ExportYaml(container));
			}

			node.Add(PrecomputedErrorName, PrecomputedError.ExportYaml());
			node.Add(MinMaxPatchHeightsName, MinMaxPatchHeights.ExportYaml());
			if (HasDefaultPhysicMaterial(container.ExportVersion))
			{
				node.Add(DefaultPhysicMaterialName, DefaultPhysicMaterial.ExportYaml(container));
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
			node.Add(ScaleName, m_Scale.ExportYaml(container));
			return node;
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
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
		public Vector3f Scale => m_Scale;

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

		public PPtr<PhysicMaterial.PhysicMaterial> DefaultPhysicMaterial = new();
		public Vector3f m_Scale = new();
	}
}
