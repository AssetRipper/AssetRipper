using System;
using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Game.Assembly;

namespace uTinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// GradientNEW previously
	/// </summary>
	public struct Gradient : IAsset, ISerializableStructure
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

		public static int ToSerializedVersion(Version version)
		{
			// ColorRBGA32 has been replaced by ColorRBGAf
			if (version.IsGreaterEqual(5, 6))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public static bool IsColor32(Version version) => version.IsLess(5, 6);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasMode(Version version) => version.IsGreaterEqual(5, 5);

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
			if (HasMode(reader.Version))
			{
				Mode = (GradientMode)reader.ReadInt32();
			}

			NumColorKeys = reader.ReadByte();
			NumAlphaKeys = reader.ReadByte();
			reader.AlignStream(AlignType.Align4);			
		}

		public void Write(AssetWriter writer)
		{
			if (IsColor32(writer.Version))
			{
				Key0.Write32(writer);
				Key1.Write32(writer);
				Key2.Write32(writer);
				Key3.Write32(writer);
				Key4.Write32(writer);
				Key5.Write32(writer);
				Key6.Write32(writer);
				Key7.Write32(writer);
			}
			else
			{
				Key0.Write(writer);
				Key1.Write(writer);
				Key2.Write(writer);
				Key3.Write(writer);
				Key4.Write(writer);
				Key5.Write(writer);
				Key6.Write(writer);
				Key7.Write(writer);
			}

			writer.Write(Ctime0);
			writer.Write(Ctime1);
			writer.Write(Ctime2);
			writer.Write(Ctime3);
			writer.Write(Ctime4);
			writer.Write(Ctime5);
			writer.Write(Ctime6);
			writer.Write(Ctime7);
			writer.Write(Atime0);
			writer.Write(Atime1);
			writer.Write(Atime2);
			writer.Write(Atime3);
			writer.Write(Atime4);
			writer.Write(Atime5);
			writer.Write(Atime6);
			writer.Write(Atime7);
			if (HasMode(writer.Version))
			{
				writer.Write((int)Mode);
			}

			writer.Write(NumColorKeys);
			writer.Write(NumAlphaKeys);
			writer.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			if (IsColor32(container.ExportVersion))
			{
				node.Add(KeyName + "0", ((ColorRGBA32)Key0).ExportYAML(container));
				node.Add(KeyName + "1", ((ColorRGBA32)Key1).ExportYAML(container));
				node.Add(KeyName + "2", ((ColorRGBA32)Key2).ExportYAML(container));
				node.Add(KeyName + "3", ((ColorRGBA32)Key3).ExportYAML(container));
				node.Add(KeyName + "4", ((ColorRGBA32)Key4).ExportYAML(container));
				node.Add(KeyName + "5", ((ColorRGBA32)Key5).ExportYAML(container));
				node.Add(KeyName + "6", ((ColorRGBA32)Key6).ExportYAML(container));
				node.Add(KeyName + "7", ((ColorRGBA32)Key7).ExportYAML(container));
			}
			else
			{
				node.Add(KeyName + "0", Key0.ExportYAML(container));
				node.Add(KeyName + "1", Key1.ExportYAML(container));
				node.Add(KeyName + "2", Key2.ExportYAML(container));
				node.Add(KeyName + "3", Key3.ExportYAML(container));
				node.Add(KeyName + "4", Key4.ExportYAML(container));
				node.Add(KeyName + "5", Key5.ExportYAML(container));
				node.Add(KeyName + "6", Key6.ExportYAML(container));
				node.Add(KeyName + "7", Key7.ExportYAML(container));
			}
			node.Add(CtimeName + "0", Ctime0);
			node.Add(CtimeName + "1", Ctime1);
			node.Add(CtimeName + "2", Ctime2);
			node.Add(CtimeName + "3", Ctime3);
			node.Add(CtimeName + "4", Ctime4);
			node.Add(CtimeName + "5", Ctime5);
			node.Add(CtimeName + "6", Ctime6);
			node.Add(CtimeName + "7", Ctime7);
			node.Add(AtimeName + "0", Atime0);
			node.Add(AtimeName + "1", Atime1);
			node.Add(AtimeName + "2", Atime2);
			node.Add(AtimeName + "3", Atime3);
			node.Add(AtimeName + "4", Atime4);
			node.Add(AtimeName + "5", Atime5);
			node.Add(AtimeName + "6", Atime6);
			node.Add(AtimeName + "7", Atime7);
			if (HasMode(container.ExportVersion))
			{
				node.Add(ModeName, (int)Mode);
			}

			node.Add(NumColorKeysName, NumColorKeys);
			node.Add(NumAlphaKeysName, NumAlphaKeys);
			return node;
		}
		
		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public void Add(ushort time, ColorRGBA32 color)
		{
			Add(time, (ColorRGBAf)color);
		}

		public void Add(ushort time, ColorRGBAf color)
		{
			AddColor(time, color.R, color.G, color.B);
			AddAlpha(time, color.A);
		}

		public void AddColor(ushort time, float r, float g, float b)
		{
			switch (NumColorKeys)
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
					Key1 = new ColorRGBAf(Key1.R, Key1.G, Key1.B, a);
					Atime1 = time;
					break;
				case 2:
					Key2 = new ColorRGBAf(Key2.R, Key2.G, Key2.B, a);
					Atime2 = time;
					break;
				case 3:
					Key3 = new ColorRGBAf(Key3.R, Key3.G, Key3.B, a);
					Atime3 = time;
					break;
				case 4:
					Key4 = new ColorRGBAf(Key4.R, Key4.G, Key4.B, a);
					Atime4 = time;
					break;
				case 5:
					Key5 = new ColorRGBAf(Key5.R, Key5.G, Key5.B, a);
					Atime5 = time;
					break;
				case 6:
					Key6 = new ColorRGBAf(Key6.R, Key6.G, Key6.B, a);
					Atime6 = time;
					break;
				case 7:
					Key7 = new ColorRGBAf(Key7.R, Key7.G, Key7.B, a);
					Atime7 = time;
					break;
				default:
					throw new NotSupportedException();
			}
			NumAlphaKeys++;
		}

		public ushort Ctime0 { get; set; }
		public ushort Ctime1 { get; set; }
		public ushort Ctime2 { get; set; }
		public ushort Ctime3 { get; set; }
		public ushort Ctime4 { get; set; }
		public ushort Ctime5 { get; set; }
		public ushort Ctime6 { get; set; }
		public ushort Ctime7 { get; set; }
		public ushort Atime0 { get; set; }
		public ushort Atime1 { get; set; }
		public ushort Atime2 { get; set; }
		public ushort Atime3 { get; set; }
		public ushort Atime4 { get; set; }
		public ushort Atime5 { get; set; }
		public ushort Atime6 { get; set; }
		public ushort Atime7 { get; set; }
		public GradientMode Mode { get; set; }
		public byte NumColorKeys { get; set; }
		public byte NumAlphaKeys { get; set; }

		public const string KeyName = "key";
		public const string CtimeName = "ctime";
		public const string AtimeName = "atime";
		public const string ModeName = "m_Mode";
		public const string NumColorKeysName = "m_NumColorKeys";
		public const string NumAlphaKeysName = "m_NumAlphaKeys";

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
