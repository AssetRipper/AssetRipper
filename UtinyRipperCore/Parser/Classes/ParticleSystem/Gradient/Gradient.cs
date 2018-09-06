using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// GradientNEW previously
	/// </summary>
	public struct Gradient : IScriptStructure
	{
		public Gradient(Gradient copy)
		{
			Ctime0 = copy.Ctime0;
			Ctime1 = copy.Ctime1;
			Ctime2 = copy.Ctime2;
			Ctime3 = copy.Ctime3;
			Ctime4 = copy.Ctime4;
			Ctime5 = copy.Ctime5;
			Ctime6 = copy.Ctime6;
			Ctime7 = copy.Ctime7;
			Atime0 = copy.Atime0;
			Atime1 = copy.Atime1;
			Atime2 = copy.Atime2;
			Atime3 = copy.Atime3;
			Atime4 = copy.Atime4;
			Atime5 = copy.Atime5;
			Atime6 = copy.Atime6;
			Atime7 = copy.Atime7;
			Mode = copy.Mode;
			NumColorKeys = copy.NumColorKeys;
			NumAlphaKeys = copy.NumAlphaKeys;
			Key32_0 = copy.Key32_0;
			Key32_1 = copy.Key32_1;
			Key32_2 = copy.Key32_2;
			Key32_3 = copy.Key32_3;
			Key32_4 = copy.Key32_4;
			Key32_5 = copy.Key32_5;
			Key32_6 = copy.Key32_6;
			Key32_7 = copy.Key32_7;
			Key0 = copy.Key0;
			Key1 = copy.Key1;
			Key2 = copy.Key2;
			Key3 = copy.Key3;
			Key4 = copy.Key4;
			Key5 = copy.Key5;
			Key6 = copy.Key6;
			Key7 = copy.Key7;
		}

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

		public IScriptStructure CreateCopy()
		{
			return new Gradient(this);
		}

		public void Read(AssetReader reader)
		{
			if (IsColor32(reader.Version))
			{
				Key32_0.Read(reader);
				Key32_1.Read(reader);
				Key32_2.Read(reader);
				Key32_3.Read(reader);
				Key32_4.Read(reader);
				Key32_5.Read(reader);
				Key32_6.Read(reader);
				Key32_7.Read(reader);
			}
			else
			{
				Key0.Read(reader);
				Key1.Read(reader);
				Key2.Read(reader);
				Key3.Read(reader);
				Key4.Read(reader);
				Key5.Read(reader);
				Key6.Read(reader);
				Key7.Read(reader);
			}
			Ctime0 = reader.ReadUInt16();
			Ctime1 = reader.ReadUInt16();
			Ctime2 = reader.ReadUInt16();
			Ctime3 = reader.ReadUInt16();
			Ctime4 = reader.ReadUInt16();
			Ctime5 = reader.ReadUInt16();
			Ctime6 = reader.ReadUInt16();
			Ctime7 = reader.ReadUInt16();
			Atime0 = reader.ReadUInt16();
			Atime1 = reader.ReadUInt16();
			Atime2 = reader.ReadUInt16();
			Atime3 = reader.ReadUInt16();
			Atime4 = reader.ReadUInt16();
			Atime5 = reader.ReadUInt16();
			Atime6 = reader.ReadUInt16();
			Atime7 = reader.ReadUInt16();
			if (IsReadMode(reader.Version))
			{
				Mode = reader.ReadInt32();
			}
			NumColorKeys = reader.ReadByte();
			NumAlphaKeys = reader.ReadByte();
			reader.AlignStream(AlignType.Align4);
			
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("key0", GetExportKey(container.Version, 0).ExportYAML(container));
			node.Add("key1", GetExportKey(container.Version, 1).ExportYAML(container));
			node.Add("key2", GetExportKey(container.Version, 2).ExportYAML(container));
			node.Add("key3", GetExportKey(container.Version, 3).ExportYAML(container));
			node.Add("key4", GetExportKey(container.Version, 4).ExportYAML(container));
			node.Add("key5", GetExportKey(container.Version, 5).ExportYAML(container));
			node.Add("key6", GetExportKey(container.Version, 6).ExportYAML(container));
			node.Add("key7", GetExportKey(container.Version, 7).ExportYAML(container));
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
		
		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public void AddColor(ushort time, float r, float g, float b)
		{
			switch(NumColorKeys)
			{
				case 0:
					Key0 = new ColorRGBAf(r, g, b, Key0.A);
					Key32_0 = new ColorRGBA32(Key0);
					Ctime0 = time;
					break;
				case 1:
					Key1 = new ColorRGBAf(r, g, b, Key1.A);
					Key32_1 = new ColorRGBA32(Key1);
					Ctime1 = time;
					break;
				case 2:
					Key2 = new ColorRGBAf(r, g, b, Key2.A);
					Key32_2 = new ColorRGBA32(Key2);
					Ctime2 = time;
					break;
				case 3:
					Key3 = new ColorRGBAf(r, g, b, Key3.A);
					Key32_3 = new ColorRGBA32(Key3);
					Ctime3 = time;
					break;
				case 4:
					Key4 = new ColorRGBAf(r, g, b, Key4.A);
					Key32_4 = new ColorRGBA32(Key4);
					Ctime4 = time;
					break;
				case 5:
					Key5 = new ColorRGBAf(r, g, b, Key5.A);
					Key32_5 = new ColorRGBA32(Key5);
					Ctime5 = time;
					break;
				case 6:
					Key6 = new ColorRGBAf(r, g, b, Key6.A);
					Key32_6 = new ColorRGBA32(Key6);
					Ctime6 = time;
					break;
				case 7:
					Key7 = new ColorRGBAf(r, g, b, Key7.A);
					Key32_7 = new ColorRGBA32(Key7);
					Ctime7 = time;
					break;
				default:
					throw new NotSupportedException();
			}
			NumColorKeys++;
		}

		public void AddAlpha(ushort time, float a)
		{
			switch (NumAlphaKeys)
			{
				case 0:
					Key0 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Key32_0 = new ColorRGBA32(Key0);
					Atime0 = time;
					break;
				case 1:
					Key1 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Key32_1 = new ColorRGBA32(Key1);
					Atime1 = time;
					break;
				case 2:
					Key2 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Key32_2 = new ColorRGBA32(Key2);
					Atime2 = time;
					break;
				case 3:
					Key3 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Key32_3 = new ColorRGBA32(Key3);
					Atime3 = time;
					break;
				case 4:
					Key4 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Key32_4 = new ColorRGBA32(Key4);
					Atime4 = time;
					break;
				case 5:
					Key5 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Key32_5 = new ColorRGBA32(Key5);
					Atime5 = time;
					break;
				case 6:
					Key6 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Key32_6 = new ColorRGBA32(Key6);
					Atime6 = time;
					break;
				case 7:
					Key7 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Key32_7 = new ColorRGBA32(Key7);
					Atime7 = time;
					break;
				default:
					throw new NotSupportedException();
			}
			NumAlphaKeys++;
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

		public IScriptStructure Base => null;
		public string Namespace => ScriptType.UnityEngineName;
		public string Name => ScriptType.GradientName;

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
