using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.Utils;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Material
{
	public sealed class UnityPropertySheet : IAssetReadable, IYAMLExportable, IDependent
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(2017, 3))
			{
				return 3;
			}
			// min version is 2
			return 2;
		}

		/// <summary>
		/// 2021 and greater
		/// </summary>
		public static bool HasInts(UnityVersion version) => version.IsGreaterEqual(2021);

		public string FindPropertyNameByCRC28(uint crc)
		{
			foreach (FastPropertyName property in TexEnvs.Keys)
			{
				string hdrName = property.Value + HDRPostfixName;
				if (CrcUtils.Verify28DigestUTF8(hdrName, crc))
				{
					return hdrName;
				}
				string stName = property.Value + STPostfixName;
				if (CrcUtils.Verify28DigestUTF8(stName, crc))
				{
					return stName;
				}
				string texelName = property.Value + TexelSizePostfixName;
				if (CrcUtils.Verify28DigestUTF8(texelName, crc))
				{
					return texelName;
				}
			}
			foreach (FastPropertyName property in Floats.Keys)
			{
				if (property.IsCRC28Match(crc))
				{
					return property.Value;
				}
			}
			foreach (FastPropertyName property in Colors.Keys)
			{
				if (property.IsCRC28Match(crc))
				{
					return property.Value;
				}
			}
			return null;
		}

		public void Read(AssetReader reader)
		{
			m_texEnvs = new Dictionary<FastPropertyName, UnityTexEnv>();
			m_floats = new Dictionary<FastPropertyName, float>();
			m_colors = new Dictionary<FastPropertyName, ColorRGBAf>();

			m_texEnvs.Read(reader);
			if (HasInts(reader.Version))
			{
				m_ints = new Dictionary<FastPropertyName, int>();
				m_ints.Read(reader);
			}
			m_floats.Read(reader);
			m_colors.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(TexEnvsName, m_texEnvs.ExportYAML(container));
			if (HasInts(container.ExportVersion))
			{
				node.Add(IntsName, m_ints.ExportYAML(container));
			}
			node.Add(FloatsName, m_floats.ExportYAML(container));
			node.Add(ColorsName, m_colors.ExportYAML(container));
			return node;
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(TexEnvs.Values, TexEnvsName))
			{
				yield return asset;
			}
		}

		public IReadOnlyDictionary<FastPropertyName, UnityTexEnv> TexEnvs => m_texEnvs;
		public IReadOnlyDictionary<FastPropertyName, float> Floats => m_floats;
		public IReadOnlyDictionary<FastPropertyName, ColorRGBAf> Colors => m_colors;

		public const string TexEnvsName = "m_TexEnvs";
		public const string IntsName = "m_Ints";
		public const string FloatsName = "m_Floats";
		public const string ColorsName = "m_Colors";

		private const string HDRPostfixName = "_HDR";
		private const string STPostfixName = "_ST";
		private const string TexelSizePostfixName = "_TexelSize";

		private Dictionary<FastPropertyName, UnityTexEnv> m_texEnvs;
		private Dictionary<FastPropertyName, int> m_ints;
		private Dictionary<FastPropertyName, float> m_floats;
		private Dictionary<FastPropertyName, ColorRGBAf> m_colors;
	}
}
