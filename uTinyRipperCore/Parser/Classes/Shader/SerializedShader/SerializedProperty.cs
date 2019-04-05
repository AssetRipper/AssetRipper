using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedProperty : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			Description = reader.ReadString();
			m_attributes = reader.ReadStringArray();
			Type = (SerializedPropertyType)reader.ReadInt32();
			Flags = (SerializedPropertyFlag)reader.ReadUInt32();
			DefValue0 = reader.ReadSingle();
			DefValue1 = reader.ReadSingle();
			DefValue2 = reader.ReadSingle();
			DefValue3 = reader.ReadSingle();
			DefTexture.Read(reader);
		}

		public void Export(TextWriter writer)
		{
			writer.WriteIndent(2);
			foreach(string attribute in Attributes)
			{
				writer.Write("[{0}] ", attribute);
			}
			if(Flags.IsHideInEnspector())
			{
				writer.Write("[HideInInspector] ");
			}
			if (Flags.IsPerRendererData())
			{
				writer.Write("[PerRendererData] ");
			}
			if (Flags.IsNoScaleOffset())
			{
				writer.Write("[NoScaleOffset] ");
			}
			if (Flags.IsNormal())
			{
				writer.Write("[Normal] ");
			}
			if (Flags.IsHDR())
			{
				writer.Write("[HDR] ");
			}
			if (Flags.IsGamma())
			{
				writer.Write("[Gamma] ");
			}

			writer.Write("{0} (\"{1}\", ", Name, Description);

			switch(Type)
			{
				case SerializedPropertyType.Color:
				case SerializedPropertyType.Vector:
					writer.Write(nameof(SerializedPropertyType.Vector));
					break;

				case SerializedPropertyType.Int:
					//case SerializedPropertyType.Float:
					writer.Write(nameof(SerializedPropertyType.Float));
					break;

				case SerializedPropertyType.Range:
					writer.Write("{0}({1}, {2})",
						nameof(SerializedPropertyType.Range),
						DefValue1.ToString(CultureInfo.InvariantCulture),
						DefValue2.ToString(CultureInfo.InvariantCulture));
					break;

				case SerializedPropertyType._2D:
				//case SerializedPropertyType._3D:
				//case SerializedPropertyType.Cube:
					switch(DefTexture.TexDim)
					{
						case 1:
							writer.Write("any");
							break;
						case 2:
							writer.Write("2D");
							break;
						case 3:
							writer.Write("3D");
							break;
						case 4:
							writer.Write(nameof(SerializedPropertyType.Cube));
							break;
						case 5:
							writer.Write("2DArray");
							break;
						case 6:
							writer.Write(nameof(SerializedPropertyType.CubeArray));
							break;
						default:
							throw new NotSupportedException("Texture dimension isn't supported");

					}
					break;

				default:
					throw new NotSupportedException($"Serialized property type {Type} isn't supported");
			}
			writer.Write(") = ");

			switch(Type)
			{
				case SerializedPropertyType.Color:
				case SerializedPropertyType.Vector:
					writer.Write("({0},{1},{2},{3})",
						DefValue0.ToString(CultureInfo.InvariantCulture),
						DefValue1.ToString(CultureInfo.InvariantCulture),
						DefValue2.ToString(CultureInfo.InvariantCulture),
						DefValue3.ToString(CultureInfo.InvariantCulture));
					break;

				case SerializedPropertyType.Int:
				//case SerializedPropertyType.Float:
				case SerializedPropertyType.Range:
					writer.Write(DefValue0.ToString(CultureInfo.InvariantCulture));
					break;

				case SerializedPropertyType._2D:
				//case SerializedPropertyType._3D:
				//case SerializedPropertyType.Cube:
					writer.Write("\"{0}\" {{}}", DefTexture.DefaultName);
					break;

				default:
					throw new NotSupportedException($"Serialized property type {Type} isn't supported");
			}
			writer.Write('\n');
		}

		public string Name { get; private set; }
		public string Description { get; private set; }
		public IReadOnlyList<string> Attributes => m_attributes;
		public SerializedPropertyType Type { get; private set; }
		public SerializedPropertyFlag Flags { get; private set; }
		public float DefValue0 { get; private set; }
		public float DefValue1 { get; private set; }
		public float DefValue2 { get; private set; }
		public float DefValue3 { get; private set; }

		public SerializedTextureProperty DefTexture;

		private string[] m_attributes;
	}
}
