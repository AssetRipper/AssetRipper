namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedShader : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			PropInfo.Read(reader);
			SubShaders = reader.ReadAssetArray<SerializedSubShader>();
			Name = reader.ReadString();
			CustomEditorName = reader.ReadString();
			FallbackName = reader.ReadString();
			Dependencies = reader.ReadAssetArray<SerializedShaderDependency>();
			DisableNoSubshadersMessage = reader.ReadBoolean();
			reader.AlignStream();
		}

		public void Export(ShaderWriter writer)
		{
			writer.Write("Shader \"{0}\" {{\n", Name);

			PropInfo.Export(writer);

			for (int i = 0; i < SubShaders.Length; i++)
			{
				SubShaders[i].Export(writer);
			}

			if (FallbackName.Length != 0)
			{
				writer.WriteIndent(1);
				writer.Write("Fallback \"{0}\"\n", FallbackName);
			}

			if (CustomEditorName.Length != 0)
			{
				writer.WriteIndent(1);
				writer.Write("CustomEditor \"{0}\"\n", CustomEditorName);
			}

			writer.Write('}');
		}

		public SerializedSubShader[] SubShaders { get; set; }
		public string Name { get; set; }
		public string CustomEditorName { get; set; }
		public string FallbackName { get; set; }
		public SerializedShaderDependency[] Dependencies { get; set; }
		public bool DisableNoSubshadersMessage { get; set; }

		public SerializedProperties PropInfo;
	}
}
