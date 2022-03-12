using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedStencilOp : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Pass.Read(reader);
			Fail.Read(reader);
			ZFail.Read(reader);
			Comp.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("pass", Pass.ExportYAML(container));
			node.Add("fail", Fail.ExportYAML(container));
			node.Add("zFail", ZFail.ExportYAML(container));
			node.Add("comp", Comp.ExportYAML(container));
			return node;
		}

		public bool IsDefault => PassValue.IsKeep() && FailValue.IsKeep() && ZFailValue.IsKeep() && CompValue.IsAlways();

		public SerializedShaderFloatValue Pass = new();
		public SerializedShaderFloatValue Fail = new();
		public SerializedShaderFloatValue ZFail = new();
		public SerializedShaderFloatValue Comp = new();

		public StencilOp PassValue => (StencilOp)Pass.Val;
		public StencilOp FailValue => (StencilOp)Fail.Val;
		public StencilOp ZFailValue => (StencilOp)ZFail.Val;
		public StencilComp CompValue => (StencilComp)Comp.Val;
	}
}
