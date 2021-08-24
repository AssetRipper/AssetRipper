using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using System;
using System.IO;
using System.Text;

namespace AssetRipper.Core.Converters.Shader
{
	public static class DummyShaderTextExporter
	{
		public const string DummyShader = @"
	//DummyShaderTextExporter
	SubShader{
		Tags { ""RenderType"" = ""Opaque"" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
		}
		ENDCG
	}
";
		public static bool IsSerialized(UnityVersion version) => version.IsGreaterEqual(5, 5);
		public static bool IsEncoded(UnityVersion version) => version.IsGreaterEqual(5, 3);
		public static void ExportShader(Classes.Shader.Shader shader, IExportContainer container, Stream stream,
			Func<UnityVersion, GPUPlatform, ShaderTextExporter> exporterInstantiator)
		{
			if (Classes.Shader.Shader.IsSerialized(container.Version))
			{
				using (ShaderWriter writer = new ShaderWriter(stream, shader, exporterInstantiator))
				{
					writer.Write("Shader \"{0}\" {{\n", shader.ParsedForm.Name);
					shader.ParsedForm.PropInfo.Export(writer);
					writer.WriteIndent(1);
					writer.Write(DummyShader);
					if (shader.ParsedForm.FallbackName != string.Empty)
					{
						writer.WriteIndent(1);
						writer.Write("Fallback \"{0}\"\n", shader.ParsedForm.FallbackName);
					}
					if (shader.ParsedForm.CustomEditorName != string.Empty)
					{
						writer.WriteIndent(1);
						writer.Write("//CustomEditor \"{0}\"\n", shader.ParsedForm.CustomEditorName);
					}
					writer.Write('}');
				}
			}
			else
			{
				if (IsSerialized(container.Version))
				{
					using (ShaderWriter writer = new ShaderWriter(stream, shader, exporterInstantiator))
					{
						shader.ParsedForm.Export(writer);
					}
				}
				else if (IsEncoded(container.Version))
				{
					using (ShaderWriter writer = new ShaderWriter(stream, shader, exporterInstantiator))
					{
						string header = Encoding.UTF8.GetString(shader.Script);
						var subshaderIndex = header.IndexOf("SubShader");
						writer.WriteString(header, 0, subshaderIndex);
						writer.WriteIndent(1);
						writer.Write(DummyShader);
						writer.Write('}');
					}
				}
				else
				{
					shader.ExportBinary(container, stream);
				}
			}
		}
	}
}
