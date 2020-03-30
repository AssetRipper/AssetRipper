using System.Collections.Generic;
using System.Text;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;

namespace DXShaderRestorer
{
	internal class ResourceChunk
	{
		public ResourceChunk(Version version, ref ShaderSubProgram shaderSubprogram)
		{
			ShaderGpuProgramType programType = shaderSubprogram.GetProgramType(version);
			m_majorVersion = (byte)programType.GetMajorDXVersion();
			m_resourceBindingOffset = m_majorVersion >= 5 ? (uint)60 : (uint)28;
			m_resourceBindings = new ResourceBindingChunk(ref shaderSubprogram, m_resourceBindingOffset, m_nameLookup);
			m_constantBufferOffset = m_resourceBindingOffset + m_resourceBindings.Size;
			m_constantBuffers = new ConstantBufferChunk(version, ref shaderSubprogram, m_constantBufferOffset, m_nameLookup);
			m_creatorStringOffset = m_constantBufferOffset + m_constantBuffers.Size;
			m_creatorString = "uTinyRipper";
			m_chunkSize = m_creatorStringOffset + (uint)m_creatorString.Length + 1;
			m_programType = programType.ToDXProgramType();
		}

		public void Write(EndianWriter writer)
		{
			writer.Write(Encoding.ASCII.GetBytes("RDEF"));
			writer.Write(m_chunkSize);
			writer.Write(m_constantBuffers.Count);
			writer.Write(m_constantBufferOffset);
			writer.Write(m_resourceBindings.Count);
			writer.Write(m_resourceBindingOffset);
			byte minorVersion = 0;
			writer.Write(minorVersion);
			writer.Write(m_majorVersion);
			writer.Write((ushort)m_programType);
			var flags = ShaderFlags.NoPreshader;
			writer.Write((uint)flags);
			writer.Write(m_creatorStringOffset);
			if (m_majorVersion >= 5)
			{
				//rd11
				writer.Write(Encoding.ASCII.GetBytes("RD11"));
				//unknown1
				writer.Write((uint)60);
				//unknown2
				writer.Write((uint)24);
				//unknown3
				writer.Write((uint)32);
				//unknown4
				writer.Write((uint)40);
				//unknown5
				writer.Write((uint)36);
				//unknown6
				writer.Write((uint)12);
				//InterfaceSlotCount
				writer.Write((uint)0);
			}
			m_resourceBindings.Write(writer);
			m_constantBuffers.Write(writer);
			writer.WriteStringZeroTerm(m_creatorString);
		}

		public uint Size => m_chunkSize + 8;

		private readonly Dictionary<string, uint> m_nameLookup = new Dictionary<string, uint>();

		//Length of Resource starting from constantBufferCount
		private readonly uint m_chunkSize;
		private readonly uint m_constantBufferOffset;
		private readonly uint m_resourceBindingOffset;
		private readonly DXProgramType m_programType;
		private readonly uint m_creatorStringOffset;
		private readonly ConstantBufferChunk m_constantBuffers;
		private readonly ResourceBindingChunk m_resourceBindings;
		private readonly string m_creatorString;
		private readonly byte m_majorVersion;
	}
}
