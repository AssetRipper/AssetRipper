using System;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// GradientNEW previously
	/// </summary>
	public struct Gradient : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public static bool IsColor32(Version version)
		{
			return version.IsLess(5, 6);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadMode(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}
			
			if (version.IsGreaterEqual(5, 6, 1))
			{
				return 2;
			}
			return 1;
		}

		private ColorRGBAf GetExportKey(Version version, int key)
		{
			switch (key)
			{
				case 0:
					return IsColor32(version) ? new ColorRGBAf(Key32_0) : Key0;
				case 1:
					return IsColor32(version) ? new ColorRGBAf(Key32_1) : Key1;
				case 2:
					return IsColor32(version) ? new ColorRGBAf(Key32_2) : Key2;
				case 3:
					return IsColor32(version) ? new ColorRGBAf(Key32_3) : Key3;
				case 4:
					return IsColor32(version) ? new ColorRGBAf(Key32_4) : Key4;
				case 5:
					return IsColor32(version) ? new ColorRGBAf(Key32_5) : Key5;
				case 6:
					return IsColor32(version) ? new ColorRGBAf(Key32_6) : Key6;
				case 7:
					return IsColor32(version) ? new ColorRGBAf(Key32_7) : Key7;
				default:
					throw new Exception($"Unsupported key {key}");
			}
		}

		public void Read(AssetStream stream)
		{
			if (IsColor32(stream.Version))
			{
				Key32_0.Read(stream);
				Key32_1.Read(stream);
				Key32_2.Read(stream);
				Key32_3.Read(stream);
				Key32_4.Read(stream);
				Key32_5.Read(stream);
				Key32_6.Read(stream);
				Key32_7.Read(stream);
			}
			else
			{
				Key0.Read(stream);
				Key1.Read(stream);
				Key2.Read(stream);
				Key3.Read(stream);
				Key4.Read(stream);
				Key5.Read(stream);
				Key6.Read(stream);
				Key7.Read(stream);
			}
			Ctime0 = stream.ReadUInt16();
			Ctime1 = stream.ReadUInt16();
			Ctime2 = stream.ReadUInt16();
			Ctime3 = stream.ReadUInt16();
			Ctime4 = stream.ReadUInt16();
			Ctime5 = stream.ReadUInt16();
			Ctime6 = stream.ReadUInt16();
			Ctime7 = stream.ReadUInt16();
			Atime0 = stream.ReadUInt16();
			Atime1 = stream.ReadUInt16();
			Atime2 = stream.ReadUInt16();
			Atime3 = stream.ReadUInt16();
			Atime4 = stream.ReadUInt16();
			Atime5 = stream.ReadUInt16();
			Atime6 = stream.ReadUInt16();
			Atime7 = stream.ReadUInt16();
			if (IsReadMode(stream.Version))
			{
				Mode = stream.ReadInt32();
			}
			NumColorKeys = stream.ReadByte();
			NumAlphaKeys = stream.ReadByte();
			stream.AlignStream(AlignType.Align4);
			
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("key0", GetExportKey(exporter.Version, 0).ExportYAML(exporter));
			node.Add("key1", GetExportKey(exporter.Version, 1).ExportYAML(exporter));
			node.Add("key2", GetExportKey(exporter.Version, 2).ExportYAML(exporter));
			node.Add("key3", GetExportKey(exporter.Version, 3).ExportYAML(exporter));
			node.Add("key4", GetExportKey(exporter.Version, 4).ExportYAML(exporter));
			node.Add("key5", GetExportKey(exporter.Version, 5).ExportYAML(exporter));
			node.Add("key6", GetExportKey(exporter.Version, 6).ExportYAML(exporter));
			node.Add("key7", GetExportKey(exporter.Version, 7).ExportYAML(exporter));
			node.Add("ctime0", Ctime0);
			node.Add("ctime1", Ctime1);
			node.Add("ctime2", Ctime2);
			node.Add("ctime3", Ctime3);
			node.Add("ctime4", Ctime4);
			node.Add("ctime5", Ctime5);
			node.Add("ctime6", Ctime6);
			node.Add("ctime7", Ctime7);
			node.Add("atime0", Atime0);
			node.Add("atime1", Atime1);
			node.Add("atime2", Atime2);
			node.Add("atime3", Atime3);
			node.Add("atime4", Atime4);
			node.Add("atime5", Atime5);
			node.Add("atime6", Atime6);
			node.Add("atime7", Atime7);
			node.Add("m_Mode", Mode);
			node.Add("m_NumColorKeys", NumColorKeys);
			node.Add("m_NumAlphaKeys", NumAlphaKeys);
			return node;
		}

		public ushort Ctime0 { get; private set; }
		public ushort Ctime1 { get; private set; }
		public ushort Ctime2 { get; private set; }
		public ushort Ctime3 { get; private set; }
		public ushort Ctime4 { get; private set; }
		public ushort Ctime5 { get; private set; }
		public ushort Ctime6 { get; private set; }
		public ushort Ctime7 { get; private set; }
		public ushort Atime0 { get; private set; }
		public ushort Atime1 { get; private set; }
		public ushort Atime2 { get; private set; }
		public ushort Atime3 { get; private set; }
		public ushort Atime4 { get; private set; }
		public ushort Atime5 { get; private set; }
		public ushort Atime6 { get; private set; }
		public ushort Atime7 { get; private set; }
		public int Mode { get; private set; }
		public byte NumColorKeys { get; private set; }
		public byte NumAlphaKeys { get; private set; }
		
		public ColorRGBA32 Key32_0;
		public ColorRGBA32 Key32_1;
		public ColorRGBA32 Key32_2;
		public ColorRGBA32 Key32_3;
		public ColorRGBA32 Key32_4;
		public ColorRGBA32 Key32_5;
		public ColorRGBA32 Key32_6;
		public ColorRGBA32 Key32_7;
		public ColorRGBAf Key0;
		public ColorRGBAf Key1;
		public ColorRGBAf Key2;
		public ColorRGBAf Key3;
		public ColorRGBAf Key4;
		public ColorRGBAf Key5;
		public ColorRGBAf Key6;
		public ColorRGBAf Key7;
	}
}
