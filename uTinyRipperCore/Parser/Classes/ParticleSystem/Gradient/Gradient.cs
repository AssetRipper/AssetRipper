using System;
using System.Collections.Generic;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// GradientNEW previously
	/// </summary>
	public struct Gradient : ISerializableStructure
	{
		public Gradient(ColorRGBAf color1, ColorRGBAf color2):
			this()
		{
			Ctime0 = 0;
			Atime0 = 0;
			Ctime1 = ushort.MaxValue;
			Atime1 = ushort.MaxValue;
			Key0 = color1;
			Key1 = color2;
			NumColorKeys = 2;
			NumAlphaKeys = 2;
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

		public ISerializableStructure CreateDuplicate()
		{
			return this;
		}

		public void Read(AssetReader reader)
		{
			if (IsColor32(reader.Version))
			{
				Key0.Read32(reader);
				Key1.Read32(reader);
				Key2.Read32(reader);
				Key3.Read32(reader);
				Key4.Read32(reader);
				Key5.Read32(reader);
				Key6.Read32(reader);
				Key7.Read32(reader);
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
				Mode = (GradientMode)reader.ReadInt32();
			}
			NumColorKeys = reader.ReadByte();
			NumAlphaKeys = reader.ReadByte();
			reader.AlignStream(AlignType.Align4);
			
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("key0", Key0.ExportYAML(container));
			node.Add("key1", Key1.ExportYAML(container));
			node.Add("key2", Key2.ExportYAML(container));
			node.Add("key3", Key3.ExportYAML(container));
			node.Add("key4", Key4.ExportYAML(container));
			node.Add("key5", Key5.ExportYAML(container));
			node.Add("key6", Key6.ExportYAML(container));
			node.Add("key7", Key7.ExportYAML(container));
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
			node.Add("m_Mode", (int)Mode);
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
					Ctime0 = time;
					break;
				case 1:
					Key1 = new ColorRGBAf(r, g, b, Key1.A);
					Ctime1 = time;
					break;
				case 2:
					Key2 = new ColorRGBAf(r, g, b, Key2.A);
					Ctime2 = time;
					break;
				case 3:
					Key3 = new ColorRGBAf(r, g, b, Key3.A);
					Ctime3 = time;
					break;
				case 4:
					Key4 = new ColorRGBAf(r, g, b, Key4.A);
					Ctime4 = time;
					break;
				case 5:
					Key5 = new ColorRGBAf(r, g, b, Key5.A);
					Ctime5 = time;
					break;
				case 6:
					Key6 = new ColorRGBAf(r, g, b, Key6.A);
					Ctime6 = time;
					break;
				case 7:
					Key7 = new ColorRGBAf(r, g, b, Key7.A);
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
					Atime0 = time;
					break;
				case 1:
					Key1 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Atime1 = time;
					break;
				case 2:
					Key2 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Atime2 = time;
					break;
				case 3:
					Key3 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Atime3 = time;
					break;
				case 4:
					Key4 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Atime4 = time;
					break;
				case 5:
					Key5 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Atime5 = time;
					break;
				case 6:
					Key6 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
					Atime6 = time;
					break;
				case 7:
					Key7 = new ColorRGBAf(Key0.R, Key0.G, Key0.B, a);
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
		public GradientMode Mode { get; private set; }
		public byte NumColorKeys { get; private set; }
		public byte NumAlphaKeys { get; private set; }
		
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
