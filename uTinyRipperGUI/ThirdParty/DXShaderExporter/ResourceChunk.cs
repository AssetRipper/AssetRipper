using System.Collections.Generic;
using System.Text;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;

namespace DXShaderExporter
{
	internal class ResourceChunk
	{
		//Length of Resource starting from constantBufferCount
		uint chunkSize;
		uint constantBufferOffset;
		uint resourceBindingOffset;
		DXProgramType programType;
		uint creatorStringOffset;
		ConstantBufferChunk constantBuffers;
		ResourceBindingChunk resourceBindings;
		string creatorString;
		Dictionary<string, uint> NameLookup = new Dictionary<string, uint>();
		byte majorVersion;
		public ResourceChunk(ShaderSubProgram shaderSubprogram)
		{
			majorVersion = (byte)DXShaderObjectExporter.GetMajorVersion(shaderSubprogram.ProgramType);
			resourceBindingOffset = majorVersion >= 5 ? (uint)60 : (uint)28;
			resourceBindings = new ResourceBindingChunk(shaderSubprogram, resourceBindingOffset, NameLookup);
			constantBufferOffset = resourceBindingOffset + resourceBindings.Size;
			constantBuffers = new ConstantBufferChunk(shaderSubprogram, constantBufferOffset, NameLookup);
			creatorStringOffset = constantBufferOffset + constantBuffers.Size;
			creatorString = "uTinyRipper";
			chunkSize = creatorStringOffset + (uint)creatorString.Length + 1;

			programType = DXShaderObjectExporter.GetDXProgramType(shaderSubprogram.ProgramType);


		}
		public void Write(EndianWriter writer)
		{
			writer.Write(Encoding.ASCII.GetBytes("RDEF"));
			writer.Write(chunkSize);
			writer.Write(constantBuffers.Count);
			writer.Write(constantBufferOffset);
			writer.Write(resourceBindings.Count);
			writer.Write(resourceBindingOffset);
			byte minorVersion = 0;
			writer.Write(minorVersion);
			writer.Write(majorVersion);
			writer.Write((ushort)programType);
			var flags = ShaderFlags.NoPreshader;
			writer.Write((uint)flags);
			writer.Write(creatorStringOffset);
			if (majorVersion >= 5)
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
			resourceBindings.Write(writer);
			constantBuffers.Write(writer);
			writer.WriteStringZeroTerm(creatorString);
		}
		public uint Size => chunkSize + 8;
	}
}
