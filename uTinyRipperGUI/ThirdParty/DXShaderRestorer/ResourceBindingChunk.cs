using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;

namespace DXShaderRestorer
{
	internal class ResourceBindingChunk
	{
		public enum SamplerFilterMode
		{
			Point,
			Linear,
			Trilinear
		};

		public enum SamplerWrapMode
		{
			Repeat,
			Clamp,
			Mirror,
			MirrorOnce
		};

		//TODO: Move to seprate file
		private class Sampler
		{
			public string Name;
			public uint BindPoint;
			public bool IsComparisonSampler;
			public Sampler(string name, uint bindPoint, bool isComparisonSampler)
			{
				this.Name = name;
				this.BindPoint = bindPoint;
				this.IsComparisonSampler = isComparisonSampler;
			}
		}

		public ResourceBindingChunk(ref ShaderSubProgram shaderSubprogram, uint resourceBindingOffset, Dictionary<string, uint> nameLookup)
		{
			m_shaderSubprogram = shaderSubprogram;
			m_nameLookup = nameLookup;
			m_Samplers = CreateSamplers(ref shaderSubprogram);
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
			foreach (Sampler sampler in m_Samplers)
			{
				nameLookup[sampler.Name] = nameOffset;
				nameOffset += (uint)sampler.Name.Length + 1;
			}
			foreach (BufferBinding constantBuffer in shaderSubprogram.ConstantBufferBindings)
			{
				nameLookup[constantBuffer.Name] = nameOffset;
				nameOffset += (uint)constantBuffer.Name.Length + 1;
			}
			Size = nameOffset - resourceBindingOffset;
		}

		internal uint Count => (uint)m_shaderSubprogram.ConstantBuffers.Length +
			(uint)m_shaderSubprogram.TextureParameters.Length +
			(uint)m_Samplers.Count +
			(uint)m_shaderSubprogram.BufferParameters.Length;

		internal uint Size { get; }

		private static List<Sampler> CreateSamplers(ref ShaderSubProgram shaderSubprogram)
		{
			/*
			 * Unity supports three types of samplers
			 * Coupled textures and sampler:
			 *		sampler2D _MainTex;
			 *		TODO: Investigate how they work
			 * Separate textures and samplers:
			 *		Texture2D _MainTex;
			 *		SamplerState sampler_MainTex; // "sampler" + “_MainTex”
			 *		These samplers do not contain an entry in SamplerParameters
			 * Inline sampler states: 
			 *		Texture2D _MainTex;
			 *		SamplerState my_point_clamp_sampler;
			 *		These samplers do contain an entry in SamplerParameters
			 * See https://docs.unity3d.com/Manual/SL-SamplerStates.html
			 */
			List<Sampler> samplers = new List<Sampler>();
			foreach (TextureParameter textureParam in shaderSubprogram.TextureParameters)
			{
				if (textureParam.SamplerIndex < 0 || textureParam.SamplerIndex == 0xFFFF) continue;
				string samplerName = "sampler" + textureParam.Name;
				samplers.Add(new Sampler(samplerName, (uint)textureParam.SamplerIndex, false));
			}
			foreach (SamplerParameter samplerParam in shaderSubprogram.SamplerParameters ?? Array.Empty<SamplerParameter>())
			{
				SamplerFilterMode filterMode = (SamplerFilterMode)(samplerParam.Sampler & 0x3);
				SamplerWrapMode wrapU = (SamplerWrapMode)((samplerParam.Sampler >> 2) & 0x3);
				SamplerWrapMode wrapV = (SamplerWrapMode)((samplerParam.Sampler >> 4) & 0x3);
				SamplerWrapMode wrapW = (SamplerWrapMode)((samplerParam.Sampler >> 6) & 0x3);
				bool isComparisonSampler = (samplerParam.Sampler & 0x100) != 0;
				string samplerName;
				if (wrapU == wrapV && wrapU == wrapW)
				{
					samplerName = $"{filterMode}_{wrapU}";
				}
				else
				{
					samplerName = $"{filterMode}_{wrapU}U_{wrapV}V_{wrapW}W";
				}
				if (isComparisonSampler)
				{
					samplerName += $"_Comparison";
				}
				samplerName += $"_Sampler{samplerParam.BindPoint}";
				samplers.Add(new Sampler(samplerName, (uint)samplerParam.BindPoint, isComparisonSampler));
			}
			samplers = samplers.OrderBy(s => s.BindPoint).ToList();
			System.Diagnostics.Debug.Assert(samplers.Select(s => s.BindPoint).Distinct().Count()
				== samplers.Select(s => s.BindPoint).Count(), "Duplicate sampler bindpoint");
			System.Diagnostics.Debug.Assert(samplers.Select(s => s.BindPoint).Distinct().Count()
				== samplers.Select(s => s.BindPoint).Count(), "Duplicate sampler name");
			return samplers;
		}
		internal void Write(EndianWriter writer)
		{
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
				//Bind count
				writer.Write((uint)1);
				//Shader input flags
				writer.Write((uint)ShaderInputFlags.None);
			}
			foreach (Sampler sampler in m_Samplers)
			{
				//Resource bindings
				//nameOffset
				writer.Write(m_nameLookup[sampler.Name]);
				//shader input type
				writer.Write((uint)ShaderInputType.Sampler);
				//Resource return type
				writer.Write((uint)ResourceReturnType.NotApplicable);
				//Resource view dimension
				writer.Write((uint)ShaderResourceViewDimension.Unknown);
				//Number of samples
				writer.Write((uint)0);
				//Bind point
				writer.Write((uint)sampler.BindPoint);
				//Bind count
				writer.Write((uint)1);
				//Shader input flags
				ShaderInputFlags samplerFlags = sampler.IsComparisonSampler ?
					ShaderInputFlags.ComparisonSampler : ShaderInputFlags.None;
				writer.Write((uint)samplerFlags);
			}
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
				//Bind count
				writer.Write((uint)1);
				//Shader input flags
				writer.Write((uint)ShaderInputFlags.TextureComponents);
			}
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
			foreach (Sampler sampler in m_Samplers)
			{
				writer.WriteStringZeroTerm(sampler.Name);
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
		private readonly List<Sampler> m_Samplers;

		private ShaderSubProgram m_shaderSubprogram;
	}
}
