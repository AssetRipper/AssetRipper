using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System.Collections.Generic;


namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedPass : IAssetReadable, IYamlExportable
	{
		/// <summary>
		/// 2020.2 and greater
		/// </summary>
		public static bool HasHash(UnityVersion version) => version.IsGreaterEqual(2020, 2);

		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasProgRayTracing(UnityVersion version) => version.IsGreaterEqual(2019, 3);

		/// <summary>
		/// 2021.2.0a16 and greater
		/// </summary>
		public static bool HasKeywordStateMaskInsteadOfKeywordMasks(UnityVersion version) => version.IsGreaterEqual(2021, 2, 0, UnityVersionType.Alpha, 16);

		/// <summary>
		/// 2018.1.0b11 and greater
		/// </summary>
		public static bool HasProceduralInstancingVariantField(UnityVersion version) => version.IsGreaterEqual(2018, 1, 0, UnityVersionType.Alpha, 11);

		/// <summary>
		/// 2021.2.0a17 and greater and Not Release
		/// </summary>
		private static bool HasSerializedPackageRequirements(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(2021, 2, 0, UnityVersionType.Alpha, 17);


		public void Read(AssetReader reader)
		{
			if (HasHash(reader.Version))
			{
				EditorDataHash = reader.ReadAssetArray<Hash128>();
				reader.AlignStream();
				Platforms = reader.ReadUInt8Array();
				reader.AlignStream();
				if (!HasKeywordStateMaskInsteadOfKeywordMasks(reader.Version))
				{
					LocalKeywordMask = reader.ReadUInt16Array();
					reader.AlignStream();
					GlobalKeywordMask = reader.ReadUInt16Array();
					reader.AlignStream();
				}
			}

			m_nameIndices = new Dictionary<string, int>();
			m_nameIndices.Read(reader);

			Type = (SerializedPassType)reader.ReadInt32();
			State.Read(reader);
			ProgramMask = reader.ReadUInt32();
			ProgVertex.Read(reader);
			ProgFragment.Read(reader);
			ProgGeometry.Read(reader);
			ProgHull.Read(reader);
			ProgDomain.Read(reader);
			if (HasProgRayTracing(reader.Version))
			{
				ProgRayTracing.Read(reader);
			}

			HasInstancingVariant = reader.ReadBoolean();
			if (HasProceduralInstancingVariantField(reader.Version))
			{
				HasProceduralInstancingVariant = reader.ReadBoolean();
			}
			reader.AlignStream();

			UseName = reader.ReadString();
			Name = reader.ReadString();
			TextureName = reader.ReadString();
			Tags.Read(reader);

			if (HasKeywordStateMaskInsteadOfKeywordMasks(reader.Version))
			{
				SerializedKeywordStateMask = reader.ReadUInt16Array();
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			if (HasHash(container.ExportVersion))
			{
				node.Add("m_EditorDataHash", EditorDataHash.ExportYaml(container));
				node.Add("m_Platforms", Platforms.ExportYaml());
				if (!HasKeywordStateMaskInsteadOfKeywordMasks(container.ExportVersion))
				{
					node.Add("m_LocalKeywordMask", LocalKeywordMask.ExportYaml(false));
					node.Add("m_GlobalKeywordMask", GlobalKeywordMask.ExportYaml(false));
				}
			}

			node.Add("m_NameIndices", NameIndices.ExportYaml());
			node.Add("m_Type", (int)Type);
			node.Add("m_State", State.ExportYaml(container));
			node.Add("m_ProgramMask", ProgramMask);
			node.Add("progVertex", ProgVertex.ExportYaml(container));
			node.Add("progFragment", ProgFragment.ExportYaml(container));
			node.Add("progGeometry", ProgGeometry.ExportYaml(container));
			node.Add("progHull", ProgHull.ExportYaml(container));
			node.Add("progDomain", ProgDomain.ExportYaml(container));
			if (HasProgRayTracing(container.ExportVersion))
			{
				node.Add("progRayTracing", ProgRayTracing.ExportYaml(container));
			}

			node.Add("m_HasInstancingVariant", HasInstancingVariant);
			if (HasProceduralInstancingVariantField(container.ExportVersion))
			{
				node.Add("m_HasProceduralInstancingVariant", HasProceduralInstancingVariant);
			}

			node.Add("m_UseName", UseName);
			node.Add("m_Name", Name);
			node.Add("m_TextureName", TextureName);
			node.Add("m_Tags", Tags.ExportYaml(container));
			if (HasKeywordStateMaskInsteadOfKeywordMasks(container.ExportVersion))
			{
				node.Add("m_SerializedKeywordStateMask", SerializedKeywordStateMask.ExportYaml(false));
			}

			// Editor Only
			if (HasSerializedPackageRequirements(container.ExportVersion, container.ExportFlags))
			{
				node.Add("m_PackageRequirements", new SerializedPackageRequirements().ExportYaml(container));
			}

			return node;
		}

		public Hash128[] EditorDataHash { get; set; }
		public byte[] Platforms { get; set; }
		public ushort[] LocalKeywordMask { get; set; }
		public ushort[] GlobalKeywordMask { get; set; }
		public IReadOnlyDictionary<string, int> NameIndices => m_nameIndices;
		public SerializedPassType Type { get; set; }
		public uint ProgramMask { get; set; }
		public bool HasInstancingVariant { get; set; }
		public bool HasProceduralInstancingVariant { get; set; }
		public string UseName { get; set; }
		public string Name { get; set; }
		public string TextureName { get; set; }
		public ushort[] SerializedKeywordStateMask { get; set; }

		public SerializedShaderState State = new();
		public SerializedProgram ProgVertex = new();
		public SerializedProgram ProgFragment = new();
		public SerializedProgram ProgGeometry = new();
		public SerializedProgram ProgHull = new();
		public SerializedProgram ProgDomain = new();
		public SerializedProgram ProgRayTracing = new();
		public SerializedTagMap Tags = new();

		private Dictionary<string, int> m_nameIndices;
	}
}
