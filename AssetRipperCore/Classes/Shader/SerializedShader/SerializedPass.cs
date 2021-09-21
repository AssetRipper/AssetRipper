using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using System.Collections.Generic;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public struct SerializedPass : IAssetReadable
	{
		/// <summary>
		/// 2020.2 and greater
		/// </summary>
		public static bool HasHash(UnityVersion version) => version.IsGreaterEqual(2020, 2);
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasProgRayTracing(UnityVersion version) => version.IsGreaterEqual(2019, 3);

		public void Read(AssetReader reader)
		{
			if (HasHash(reader.Version))
			{
				EditorDataHash = reader.ReadAssetArray<Hash128>();
				reader.AlignStream();
				Platforms = reader.ReadUInt8Array();
				reader.AlignStream();
				LocalKeywordMask = reader.ReadUInt16Array();
				reader.AlignStream();
				GlobalKeywordMask = reader.ReadUInt16Array();
				reader.AlignStream();
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
			reader.AlignStream();

			UseName = reader.ReadString();
			Name = reader.ReadString();
			TextureName = reader.ReadString();
			Tags.Read(reader);
		}

		public Hash128[] EditorDataHash { get; set; }
		public byte[] Platforms { get; set; }
		public ushort[] LocalKeywordMask { get; set; }
		public ushort[] GlobalKeywordMask { get; set; }
		public IReadOnlyDictionary<string, int> NameIndices => m_nameIndices;
		public SerializedPassType Type { get; set; }
		public uint ProgramMask { get; set; }
		public bool HasInstancingVariant { get; set; }
		public string UseName { get; set; }
		public string Name { get; set; }
		public string TextureName { get; set; }

		public SerializedShaderState State;
		public SerializedProgram ProgVertex;
		public SerializedProgram ProgFragment;
		public SerializedProgram ProgGeometry;
		public SerializedProgram ProgHull;
		public SerializedProgram ProgDomain;
		public SerializedProgram ProgRayTracing;
		public SerializedTagMap Tags;

		private Dictionary<string, int> m_nameIndices;
	}
}
