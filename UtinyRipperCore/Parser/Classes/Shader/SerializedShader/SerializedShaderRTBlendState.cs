using System.IO;

namespace UtinyRipper.Classes.Shaders
{
	public struct SerializedShaderRTBlendState : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			SrcBlend.Read(stream);
			DestBlend.Read(stream);
			SrcBlendAlpha.Read(stream);
			DestBlendAlpha.Read(stream);
			BlendOp.Read(stream);
			BlendOpAlpha.Read(stream);
			ColMask.Read(stream);
		}

		public void Export(TextWriter writer, int index)
		{
			if (!SrcBlendValue.IsOne() || !DestBlendValue.IsZero() || !SrcBlendAlphaValue.IsOne() || !DestBlendAlphaValue.IsZero())
			{
				writer.WriteIntent(3);
				writer.Write("Blend ");
				if(index != -1)
				{
					writer.Write("{0} ", index);
				}
				writer.Write("{0} {1}", SrcBlendValue, DestBlendValue);
				if(!SrcBlendAlphaValue.IsOne() || !DestBlendAlphaValue.IsZero())
				{
					writer.Write(", {0} {1}", SrcBlendAlphaValue, DestBlendAlphaValue);
				}
				writer.Write('\n');
			}

			if(!BlendOpValue.IsAdd() || !BlendOpAlphaValue.IsAdd())
			{
				writer.WriteIntent(3);
				writer.Write("BlendOp ");
				if(index != -1)
				{
					writer.Write("{0} ", index);
				}
				writer.Write(BlendOpValue.ToString());
				if (!BlendOpAlphaValue.IsAdd())
				{
					writer.Write(", {0}", BlendOpAlphaValue);
				}
				writer.Write('\n');
			}
			
			if(!ColMaskValue.IsRBGA())
			{
				writer.WriteIntent(3);
				writer.Write("ColorMask ");
				if (ColMaskValue.IsNone())
				{
					writer.Write(0);
				}
				else
				{
					if (ColMaskValue.IsRed())
					{
						writer.Write(nameof(ColorMask.R));
					}
					if (ColMaskValue.IsGreen())
					{
						writer.Write(nameof(ColorMask.G));
					}
					if (ColMaskValue.IsBlue())
					{
						writer.Write(nameof(ColorMask.B));
					}
					if (ColMaskValue.IsAlpha())
					{
						writer.Write(nameof(ColorMask.A));
					}
				}
				writer.Write(" {0}\n", index);
			}
		}

		public SerializedShaderFloatValue SrcBlend;
		public SerializedShaderFloatValue DestBlend;
		public SerializedShaderFloatValue SrcBlendAlpha;
		public SerializedShaderFloatValue DestBlendAlpha;
		public SerializedShaderFloatValue BlendOp;
		public SerializedShaderFloatValue BlendOpAlpha;
		public SerializedShaderFloatValue ColMask;

		private BlendFactor SrcBlendValue => (BlendFactor)SrcBlend.Val;
		private BlendFactor DestBlendValue => (BlendFactor)DestBlend.Val;
		private BlendFactor SrcBlendAlphaValue => (BlendFactor)SrcBlendAlpha.Val;
		private BlendFactor DestBlendAlphaValue => (BlendFactor)DestBlendAlpha.Val;
		private BlendOp BlendOpValue => (BlendOp)BlendOp.Val;
		private BlendOp BlendOpAlphaValue => (BlendOp)BlendOpAlpha.Val;
		private ColorMask ColMaskValue => (ColorMask)ColMask.Val;
	}
}
