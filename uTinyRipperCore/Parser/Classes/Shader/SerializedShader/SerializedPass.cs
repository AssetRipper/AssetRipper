using System;
using System.Collections.Generic;

namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedPass : IAssetReadable
	{
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasProgRayTracing(Version version) => version.IsGreaterEqual(2019, 3);

		public void Read(AssetReader reader)
		{
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

		public void Export(ShaderWriter writer)
		{
			writer.WriteIndent(2);
			writer.Write("{0} ", Type.ToString());

			if (Type == SerializedPassType.UsePass)
			{
				writer.Write("\"{0}\"\n", UseName);
			}
			else
			{
				writer.Write("{\n");

				if (Type == SerializedPassType.GrabPass)
				{
					if (TextureName.Length > 0)
					{
						writer.WriteIndent(3);
						writer.Write("\"{0}\"\n", TextureName);
					}
				}
				else if (Type == SerializedPassType.Pass)
				{
					State.Export(writer);

					if ((ProgramMask & ShaderType.Vertex.ToProgramMask()) != 0)
					{
						ProgVertex.Export(writer, ShaderType.Vertex);
					}
					if ((ProgramMask & ShaderType.Fragment.ToProgramMask()) != 0)
					{
						ProgFragment.Export(writer, ShaderType.Fragment);
					}
					if ((ProgramMask & ShaderType.Geometry.ToProgramMask()) != 0)
					{
						ProgGeometry.Export(writer, ShaderType.Geometry);
					}
					if ((ProgramMask & ShaderType.Hull.ToProgramMask()) != 0)
					{
						ProgHull.Export(writer, ShaderType.Hull);
					}
					if ((ProgramMask & ShaderType.Domain.ToProgramMask()) != 0)
					{
						ProgDomain.Export(writer, ShaderType.Domain);
					}
					if ((ProgramMask & ShaderType.RayTracing.ToProgramMask()) != 0)
					{
						ProgDomain.Export(writer, ShaderType.RayTracing);
					}

#warning HasInstancingVariant?
				}
				else
				{
					throw new NotSupportedException($"Unsupported pass type {Type}");
				}

				writer.WriteIndent(2);
				writer.Write("}\n");
			}
		}

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
