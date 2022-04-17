using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedShaderRTBlendState : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			SrcBlend.Read(reader);
			DestBlend.Read(reader);
			SrcBlendAlpha.Read(reader);
			DestBlendAlpha.Read(reader);
			BlendOp.Read(reader);
			BlendOpAlpha.Read(reader);
			ColMask.Read(reader);
		}

		public SerializedShaderFloatValue SrcBlend = new();
		public SerializedShaderFloatValue DestBlend = new();
		public SerializedShaderFloatValue SrcBlendAlpha = new();
		public SerializedShaderFloatValue DestBlendAlpha = new();
		public SerializedShaderFloatValue BlendOp = new();
		public SerializedShaderFloatValue BlendOpAlpha = new();
		public SerializedShaderFloatValue ColMask = new();

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("srcBlend", SrcBlend.ExportYaml(container));
			node.Add("destBlend", DestBlend.ExportYaml(container));
			node.Add("srcBlendAlpha", SrcBlendAlpha.ExportYaml(container));
			node.Add("destBlendAlpha", DestBlendAlpha.ExportYaml(container));
			node.Add("blendOp", BlendOp.ExportYaml(container));
			node.Add("blendOpAlpha", BlendOpAlpha.ExportYaml(container));
			node.Add("colMask", ColMask.ExportYaml(container));
			return node;
		}

		public BlendMode SrcBlendValue => (BlendMode)SrcBlend.Val;
		public BlendMode DestBlendValue => (BlendMode)DestBlend.Val;
		public BlendMode SrcBlendAlphaValue => (BlendMode)SrcBlendAlpha.Val;
		public BlendMode DestBlendAlphaValue => (BlendMode)DestBlendAlpha.Val;
		public BlendOp BlendOpValue => (BlendOp)BlendOp.Val;
		public BlendOp BlendOpAlphaValue => (BlendOp)BlendOpAlpha.Val;
		public ColorWriteMask ColMaskValue => (ColorWriteMask)ColMask.Val;
	}
}
