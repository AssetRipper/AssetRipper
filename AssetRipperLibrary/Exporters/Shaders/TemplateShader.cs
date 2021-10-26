using AssetRipper.Core.Classes.Shader;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetRipper.Library.Exporters.Shaders
{
	public class TemplateShader
	{
		private ushort m_Lod = 200;
		private string m_ShaderText;
		public List<RequiredProperty> RequiredProperties { get; init; } = new List<RequiredProperty>();
		public ushort Lod
		{
			get => m_Lod;
			set => m_Lod = value;
		}
		public string ShaderText
		{
			get => m_ShaderText;
			set => m_ShaderText = value;
		}

		public override string ToString() => ToString(false);
		public string ToString(bool twoSided)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append('\n');
			sb.Append("\t//DummyShaderTextExporter\n");
			sb.Append("\tSubShader{\n");

			sb.Append('\t');
			sb.Append(@"Tags { ""RenderType"" = ""Opaque"" }");
			sb.Append('\n');

			sb.Append($"\t\tLOD {m_Lod}\n");

			if (twoSided)
				sb.Append("\t\tCull off");

			sb.Append("\t\tCGPROGRAM\n");
			sb.Append("#pragma surface surf Standard fullforwardshadows\n");
			sb.Append("#pragma target 3.0\n");

			sb.Append(m_ShaderText ?? "\t\t//No shader text");
			sb.Append('\n');

			sb.Append("ENDCG\n");
			sb.Append("\t}\n");
			sb.Append('\n');
			return sb.ToString();
		}

		public bool IsMatch(Shader shader)
		{
			if (RequiredProperties == null || RequiredProperties.Count == 0)
				return true;
			var properties = shader.ParsedForm.PropInfo.Props;
			if(properties == null || properties.Length == 0)
				return false;
			foreach(var reqProp in RequiredProperties)
			{
				int matches = properties.Where(prop => reqProp.IsMatch(prop)).Count();
				if (matches == 0)
					return false;
			}
			return true;
		}
	}
}
