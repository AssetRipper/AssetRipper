using SevenZip;
using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.Materials
{
	public struct UnityPropertySheet : IAssetReadable, IYAMLExportable, IDependent
	{
		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2017, 3))
			{
				return 3;
			}
			// min version is 2
			return 2;
		}

		public string FindPropertyNameByCRC28(uint crc)
		{
			foreach (FastPropertyName property in TexEnvs.Keys)
			{
				string hdrName = property.Value + HDRPostfixName;
				if (CRC.Verify28DigestUTF8(hdrName, crc))
				{
					return hdrName;
				}
				string stName = property.Value + STPostfixName;
				if (CRC.Verify28DigestUTF8(stName, crc))
				{
					return stName;
				}
				string texelName = property.Value + TexelSizePostfixName;
				if (CRC.Verify28DigestUTF8(texelName, crc))
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
			m_floats.Read(reader);
			m_colors.Read(reader);
		}
		
		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(TexEnvsName, m_texEnvs.ExportYAML(container));
			node.Add(FloatsName, m_floats.ExportYAML(container));
			node.Add(ColorsName, m_colors.ExportYAML(container));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(UnityTexEnv env in m_texEnvs.Values)
			{
				foreach(Object asset in env.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
		}

		public IReadOnlyDictionary<FastPropertyName, UnityTexEnv> TexEnvs => m_texEnvs;
		public IReadOnlyDictionary<FastPropertyName, float> Floats => m_floats;
		public IReadOnlyDictionary<FastPropertyName, ColorRGBAf> Colors => m_colors;

		public const string TexEnvsName = "m_TexEnvs";
		public const string FloatsName = "m_Floats";
		public const string ColorsName = "m_Colors";

		private const string HDRPostfixName = "_HDR";
		private const string STPostfixName = "_ST";
		private const string TexelSizePostfixName = "_TexelSize";

		private Dictionary<FastPropertyName, UnityTexEnv> m_texEnvs;
		private Dictionary<FastPropertyName, float> m_floats;
		private Dictionary<FastPropertyName, ColorRGBAf> m_colors;
	}
}
