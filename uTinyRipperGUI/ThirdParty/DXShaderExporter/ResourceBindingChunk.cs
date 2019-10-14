using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;

namespace DXShaderExporter
{
	internal class ResourceBindingChunk
	{
		private ShaderSubProgram shaderSubprogram;
		private uint resourceBindingOffset;
		private Dictionary<string, uint> nameLookup;
		uint size;
		public ResourceBindingChunk(ShaderSubProgram shaderSubprogram, uint resourceBindingOffset, Dictionary<string, uint> nameLookup)
		{
			this.shaderSubprogram = shaderSubprogram;
			this.resourceBindingOffset = resourceBindingOffset;
			this.nameLookup = nameLookup;

			const uint bindingHeaderSize = 32;
			uint nameOffset = resourceBindingOffset + bindingHeaderSize * Count;
			foreach (var bufferParam in shaderSubprogram.BufferParameters)
			{
				nameLookup[bufferParam.Name] = nameOffset;
				nameOffset += (uint)bufferParam.Name.Length + 1;
			}
			foreach (var textureParam in shaderSubprogram.TextureParameters)
			{
				nameLookup[textureParam.Name] = nameOffset;
				nameOffset += (uint)textureParam.Name.Length + 1;
			}
			foreach (var constantBuffer in shaderSubprogram.ConstantBufferBindings)
			{
				nameLookup[constantBuffer.Name] = nameOffset;
				nameOffset += (uint)constantBuffer.Name.Length + 1;
			}
			size = nameOffset - resourceBindingOffset;
		}

		internal uint Size => size;
		internal uint Count => (uint)shaderSubprogram.ConstantBuffers.Count +
			(uint)shaderSubprogram.TextureParameters.Count * 2 +
			(uint)shaderSubprogram.BufferParameters.Count;

		internal void Write(EndianWriter writer)
		{
			uint bindPoint = 0;
			foreach (var bufferParam in shaderSubprogram.BufferParameters)
			{
				//Resource bindings
				//nameOffset
				writer.Write(nameLookup[bufferParam.Name]);
				//shader input type
				writer.Write((uint)ShaderInputType.Structured);
				//Resource return type
				writer.Write((uint)ResourceReturnType.Mixed);
				//Resource view dimension
				writer.Write((uint)ShaderResourceViewDimension.Buffer);
				//Number of samples
				writer.Write((uint)56); //TODO: Check this
										//Bind point
				writer.Write((uint)bufferParam.Index);
				bindPoint += 1;
				//Bind count
				writer.Write((uint)1);
				//Shader input flags
				writer.Write((uint)ShaderInputFlags.None);
			}
			bindPoint = 0;
			foreach (var textureParam in shaderSubprogram.TextureParameters)
			{
				//Resource bindings
				//nameOffset
				writer.Write(nameLookup[textureParam.Name]);
				//shader input type
				writer.Write((uint)ShaderInputType.Sampler);
				//Resource return type
				writer.Write((uint)ResourceReturnType.NotApplicable);
				//Resource view dimension
				writer.Write((uint)ShaderResourceViewDimension.Unknown);
				//Number of samples
				writer.Write((uint)0);
				//Bind point
				writer.Write((uint)textureParam.Index);
				bindPoint += 1;
				//Bind count
				writer.Write((uint)1);
				//Shader input flags
				writer.Write((uint)ShaderInputFlags.None);
			}
			bindPoint = 0;
			foreach (var textureParam in shaderSubprogram.TextureParameters)
			{
				//Resource bindings
				//nameOffset
				writer.Write(nameLookup[textureParam.Name]);
				//shader input type
				writer.Write((uint)ShaderInputType.Texture);
				//Resource return type
				writer.Write((uint)ResourceReturnType.NotApplicable);
				//Resource view dimension
				writer.Write((uint)textureParam.Dim);
				//Number of samples
				writer.Write(uint.MaxValue);
				//Bind point
				writer.Write((uint)textureParam.Index);
				bindPoint += 1;
				//Bind count
				writer.Write((uint)1);
				//Shader input flags
				writer.Write((uint)ShaderInputFlags.None);
			}
			bindPoint = 0;
			foreach (var constantBuffer in shaderSubprogram.ConstantBufferBindings)
			{
				//Resource bindings
				//nameOffset
				writer.Write(nameLookup[constantBuffer.Name]);
				//shader input type
				writer.Write((uint)ShaderInputType.CBuffer);
				//Resource return type
				writer.Write((uint)ResourceReturnType.NotApplicable);
				//Resource view dimension
				writer.Write((uint)ShaderResourceViewDimension.Unknown);
				//Number of samples
				writer.Write((uint)0);
				//Bind point
				writer.Write((uint)constantBuffer.Index);
				bindPoint += 1;
				//Bind count
				writer.Write((uint)1);
				//Shader input flags
				writer.Write((uint)ShaderInputFlags.None);
			}

			foreach (var bufferParam in shaderSubprogram.BufferParameters)
			{
				writer.WriteStringZeroTerm(bufferParam.Name);
			}
			foreach (var textureParam in shaderSubprogram.TextureParameters)
			{
				writer.WriteStringZeroTerm(textureParam.Name);
			}
			foreach (var constantBuffer in shaderSubprogram.ConstantBufferBindings)
			{
				writer.WriteStringZeroTerm(constantBuffer.Name);
			}
		}
	}
}
