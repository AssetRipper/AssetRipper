using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedStencilOp : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Pass.Read(reader);
			Fail.Read(reader);
			ZFail.Read(reader);
			Comp.Read(reader);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("pass", Pass.ExportYaml(container));
			node.Add("fail", Fail.ExportYaml(container));
			node.Add("zFail", ZFail.ExportYaml(container));
			node.Add("comp", Comp.ExportYaml(container));
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
