using System.Collections.Generic;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;

namespace DXShaderRestorer
{
	internal class ResourceBindingChunk
	{
		public ResourceBindingChunk(ShaderSubProgram shaderSubprogram, uint resourceBindingOffset, Dictionary<string, uint> nameLookup)
		{
			m_shaderSubprogram = shaderSubprogram;
			m_nameLookup = nameLookup;

			const uint bindingHeaderSize = 32;
			uint nameOffset = resourceBindingOffset + bindingHeaderSize * Count;
			foreach (BufferBinding bufferParam in shaderSubprogram.BufferParameters)
			{
				nameLookup[bufferParam.Name] = nameOffset;
				nameOffset += (uint)bufferParam.Name.Length + 1;
			}
			foreach (TextureParameter textureParam in shaderSubprogram.TextureParameters)
			{
				nameLookup[textureParam.Name] = nameOffset;
				nameOffset += (uint)textureParam.Name.Length + 1;
			}
			foreach (BufferBinding constantBuffer in shaderSubprogram.ConstantBufferBindings)
			{
				nameLookup[constantBuffer.Name] = nameOffset;
				nameOffset += (uint)constantBuffer.Name.Length + 1;
			}
			Size = nameOffset - resourceBindingOffset;
		}

		internal uint Count => (uint)m_shaderSubprogram.ConstantBuffers.Length +
			(uint)m_shaderSubprogram.TextureParameters.Length * 2 + (uint)m_shaderSubprogram.BufferParameters.Length;

		internal uint Size { get; }

		internal void Write(EndianWriter writer)
		{
			uint bindPoint = 0;
			foreach (BufferBinding bufferParam in m_shaderSubprogram.BufferParameters)
			{
				//Resource bindings
				//nameOffset
				writer.Write(m_nameLookup[bufferParam.Name]);
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
			//Unity doesn't give us a good way of reconstructing the sampler header,
			//this is probably wrong but good enough
			bindPoint = 0;
			foreach (TextureParameter textureParam in m_shaderSubprogram.TextureParameters)
			{
				//Resource bindings
				//nameOffset
				writer.Write(m_nameLookup[textureParam.Name]);
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
			foreach (TextureParameter textureParam in m_shaderSubprogram.TextureParameters)
			{
				//Resource bindings
				//nameOffset
				writer.Write(m_nameLookup[textureParam.Name]);
				//shader input type
				writer.Write((uint)ShaderInputType.Texture);
				//Resource return type
				writer.Write((uint)ResourceReturnType.Float);
				//Resource view dimension
				writer.Write((uint)GetTextureDimension(textureParam));
				//Number of samples
				writer.Write(uint.MaxValue);
				//Bind point
				writer.Write((uint)textureParam.Index);
				bindPoint += 1;
				//Bind count
				writer.Write((uint)1);
				//Shader input flags
				writer.Write((uint)ShaderInputFlags.TextureComponents);
			}
			bindPoint = 0;
			foreach (BufferBinding constantBuffer in m_shaderSubprogram.ConstantBufferBindings)
			{
				//Resource bindings
				//nameOffset
				writer.Write(m_nameLookup[constantBuffer.Name]);
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

			foreach (BufferBinding bufferParam in m_shaderSubprogram.BufferParameters)
			{
				writer.WriteStringZeroTerm(bufferParam.Name);
			}
			foreach (TextureParameter textureParam in m_shaderSubprogram.TextureParameters)
			{
				writer.WriteStringZeroTerm(textureParam.Name);
			}
			foreach (BufferBinding constantBuffer in m_shaderSubprogram.ConstantBufferBindings)
			{
				writer.WriteStringZeroTerm(constantBuffer.Name);
			}
		}

		ShaderResourceViewDimension GetTextureDimension(TextureParameter param)
		{
			switch (param.Dim)
			{
				case 2:
					return ShaderResourceViewDimension.Texture2D;
				case 3:
					return ShaderResourceViewDimension.Texture3D;
				case 4:
					return ShaderResourceViewDimension.TextureCube;
				case 5:
					return ShaderResourceViewDimension.Texture2DArray;
				case 6:
					return ShaderResourceViewDimension.TextureCubeArray;
				default:
					return ShaderResourceViewDimension.Texture2D;
			}
		}

		private readonly Dictionary<string, uint> m_nameLookup;

		private ShaderSubProgram m_shaderSubprogram;
	}
}
