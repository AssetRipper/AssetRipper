using System;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// GradientNEW previously
	/// </summary>
	public struct Gradient : IAsset
	{
		public Gradient(ColorRGBAf color1, ColorRGBAf color2) :
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

		public void Read(AssetReader reader)
		{
			GradientLayout layout = reader.Layout.Serialized.Gradient;
			if (layout.Version == 1)
			{
				Key0_32 = reader.ReadAsset<ColorRGBA32>();
				Key1_32 = reader.ReadAsset<ColorRGBA32>();
				Key2_32 = reader.ReadAsset<ColorRGBA32>();
				Key3_32 = reader.ReadAsset<ColorRGBA32>();
				Key4_32 = reader.ReadAsset<ColorRGBA32>();
				Key5_32 = reader.ReadAsset<ColorRGBA32>();
				Key6_32 = reader.ReadAsset<ColorRGBA32>();
				Key7_32 = reader.ReadAsset<ColorRGBA32>();
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
			if (layout.HasMode)
			{
				Mode = (GradientMode)reader.ReadInt32();
			}

			NumColorKeys = reader.ReadByte();
			NumAlphaKeys = reader.ReadByte();
			reader.AlignStream();
		}

		public void Write(AssetWriter writer)
		{
			GradientLayout layout = writer.Layout.Serialized.Gradient;
			if (layout.Version == 1)
			{
				Key0_32.Write(writer);
				Key1_32.Write(writer);
				Key2_32.Write(writer);
				Key3_32.Write(writer);
				Key4_32.Write(writer);
				Key5_32.Write(writer);
				Key6_32.Write(writer);
				Key7_32.Write(writer);
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
			if (layout.HasMode)
			{
				writer.Write((int)Mode);
			}

			writer.Write(NumColorKeys);
			writer.Write(NumAlphaKeys);
			writer.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			GradientLayout layout = container.ExportLayout.Serialized.Gradient;
			node.AddSerializedVersion(layout.Version);
			if (layout.Version == 1)
			{
				node.Add(layout.Key0Name, Key0_32.ExportYAML(container));
				node.Add(layout.Key1Name, Key1_32.ExportYAML(container));
				node.Add(layout.Key2Name, Key2_32.ExportYAML(container));
				node.Add(layout.Key3Name, Key3_32.ExportYAML(container));
				node.Add(layout.Key4Name, Key4_32.ExportYAML(container));
				node.Add(layout.Key5Name, Key5_32.ExportYAML(container));
				node.Add(layout.Key6Name, Key6_32.ExportYAML(container));
				node.Add(layout.Key7Name, Key7_32.ExportYAML(container));
			}
			else
			{
				node.Add(layout.Key0Name, Key0.ExportYAML(container));
				node.Add(layout.Key1Name, Key1.ExportYAML(container));
				node.Add(layout.Key2Name, Key2.ExportYAML(container));
				node.Add(layout.Key3Name, Key3.ExportYAML(container));
				node.Add(layout.Key4Name, Key4.ExportYAML(container));
				node.Add(layout.Key5Name, Key5.ExportYAML(container));
				node.Add(layout.Key6Name, Key6.ExportYAML(container));
				node.Add(layout.Key7Name, Key7.ExportYAML(container));
			}

			node.Add(layout.Ctime0Name, Ctime0);
			node.Add(layout.Ctime1Name, Ctime1);
			node.Add(layout.Ctime2Name, Ctime2);
			node.Add(layout.Ctime3Name, Ctime3);
			node.Add(layout.Ctime4Name, Ctime4);
			node.Add(layout.Ctime5Name, Ctime5);
			node.Add(layout.Ctime6Name, Ctime6);
			node.Add(layout.Ctime7Name, Ctime7);
			node.Add(layout.Atime0Name, Atime0);
			node.Add(layout.Atime1Name, Atime1);
			node.Add(layout.Atime2Name, Atime2);
			node.Add(layout.Atime3Name, Atime3);
			node.Add(layout.Atime4Name, Atime4);
			node.Add(layout.Atime5Name, Atime5);
			node.Add(layout.Atime6Name, Atime6);
			node.Add(layout.Atime7Name, Atime7);
			if (layout.HasMode)
			{
				node.Add(layout.ModeName, (int)Mode);
			}

			node.Add(layout.NumColorKeysName, NumColorKeys);
			node.Add(layout.NumAlphaKeysName, NumAlphaKeys);
			return node;
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

		public ColorRGBA32 Key0_32
		{
			get => (ColorRGBA32)Key0;
			set => Key0 = (ColorRGBAf)value;
		}
		public ColorRGBA32 Key1_32
		{
			get => (ColorRGBA32)Key1;
			set => Key1 = (ColorRGBAf)value;
		}
		public ColorRGBA32 Key2_32
		{
			get => (ColorRGBA32)Key2;
			set => Key2 = (ColorRGBAf)value;
		}
		public ColorRGBA32 Key3_32
		{
			get => (ColorRGBA32)Key3;
			set => Key3 = (ColorRGBAf)value;
		}
		public ColorRGBA32 Key4_32
		{
			get => (ColorRGBA32)Key4;
			set => Key4 = (ColorRGBAf)value;
		}
		public ColorRGBA32 Key5_32
		{
			get => (ColorRGBA32)Key5;
			set => Key5 = (ColorRGBAf)value;
		}
		public ColorRGBA32 Key6_32
		{
			get => (ColorRGBA32)Key6;
			set => Key6 = (ColorRGBAf)value;
		}
		public ColorRGBA32 Key7_32
		{
			get => (ColorRGBA32)Key7;
			set => Key7 = (ColorRGBAf)value;
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
