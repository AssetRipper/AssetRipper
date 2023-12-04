using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Export.Modules.Shaders.IO;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedProperties;
using AssetRipper.SourceGenerated.Subclasses.SerializedProperty;
using System.Globalization;

namespace AssetRipper.Export.UnityProjects.Shaders
{
	public sealed class DummyShaderTextExporter : ShaderExporterBase
	{
		private static string FallbackDummyShader { get; } = """

				SubShader{
					Tags { "RenderType" = "Opaque" }
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

			""".Replace("\r", "");

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			using FileStream fileStream = File.Create(path);
			using InvariantStreamWriter writer = new InvariantStreamWriter(fileStream);
			ExportShader((IShader)asset, writer);
			return true;
		}

		public static void ExportShader(IShader shader, TextWriter writer)
		{
			if (shader.Has_ParsedForm())
			{
				writer.Write($"Shader \"{shader.ParsedForm.Name}\" {{\n");
				Export(shader.ParsedForm.PropInfo, writer);

				TemplateShader templateShader = TemplateList.GetBestTemplate(shader);
				writer.Write("\t//DummyShaderTextExporter\n");
				if (templateShader != null)
				{
					writer.Write(templateShader.ShaderText);
				}
				else
				{
					writer.WriteIndent(1);
					writer.Write(FallbackDummyShader);
				}
				writer.Write('\n');

				if (shader.ParsedForm.FallbackName != string.Empty)
				{
					writer.WriteIndent(1);
					writer.Write($"Fallback \"{shader.ParsedForm.FallbackName}\"\n");
				}
				if (shader.ParsedForm.CustomEditorName != string.Empty)
				{
					writer.WriteIndent(1);
					writer.Write($"//CustomEditor \"{shader.ParsedForm.CustomEditorName}\"\n");
				}
				writer.Write('}');
			}
			else
			{
				string header = shader.Script.String;
				int subshaderIndex = header.IndexOf("SubShader");
				writer.WriteString(header, 0, subshaderIndex);

				writer.Write("\t//DummyShaderTextExporter\n");
				writer.WriteIndent(1);
				writer.Write(FallbackDummyShader);

				writer.Write('}');
			}
		}

		private static void Export(ISerializedProperties _this, TextWriter writer)
		{
			writer.WriteIndent(1);
			writer.Write("Properties {\n");
			foreach (ISerializedProperty prop in _this.Props)
			{
				Export(prop, writer);
			}
			writer.WriteIndent(1);
			writer.Write("}\n");
		}

		private static void Export(ISerializedProperty _this, TextWriter writer)
		{
			writer.WriteIndent(2);
			foreach (Utf8String attribute in _this.Attributes)
			{
				writer.Write($"[{attribute}] ");
			}
			SerializedPropertyFlag flags = (SerializedPropertyFlag)_this.Flags;
			if (flags.IsHideInInspector())
			{
				writer.Write("[HideInInspector] ");
			}
			if (flags.IsPerRendererData())
			{
				writer.Write("[PerRendererData] ");
			}
			if (flags.IsNoScaleOffset())
			{
				writer.Write("[NoScaleOffset] ");
			}
			if (flags.IsNormal())
			{
				writer.Write("[Normal] ");
			}
			if (flags.IsHDR())
			{
				writer.Write("[HDR] ");
			}
			if (flags.IsGamma())
			{
				writer.Write("[Gamma] ");
			}

			writer.Write($"{_this.Name} (\"{_this.Description}\", ");

			switch (_this.GetType_())
			{
				case SerializedPropertyType.Color:
				case SerializedPropertyType.Vector:
					writer.Write("Vector");
					break;

				case SerializedPropertyType.Float:
					writer.Write("Float");
					break;

				case SerializedPropertyType.Range:
					writer.Write($"Range({
						_this.DefValue_1_.ToString(CultureInfo.InvariantCulture)}, {
						_this.DefValue_2_.ToString(CultureInfo.InvariantCulture)})");
					break;

				case SerializedPropertyType.Texture:
					switch (_this.DefTexture.TexDim)
					{
						case 1:
							writer.Write("any");
							break;
						case 2:
							writer.Write("2D");
							break;
						case 3:
							writer.Write("3D");
							break;
						case 4:
							writer.Write("Cube");
							break;
						case 5:
							writer.Write("2DArray");
							break;
						case 6:
							writer.Write("CubeArray");
							break;
						default:
							throw new NotSupportedException("Texture dimension isn't supported");

					}
					break;

				case SerializedPropertyType.Int:
					writer.Write("Int");
					break;

				default:
					throw new NotSupportedException($"Serialized property type {_this.Type} isn't supported");
			}
			writer.Write(") = ");

			switch (_this.GetType_())
			{
				case SerializedPropertyType.Color:
				case SerializedPropertyType.Vector:
					writer.Write($"({
						_this.DefValue_0_.ToString(CultureInfo.InvariantCulture)},{
						_this.DefValue_1_.ToString(CultureInfo.InvariantCulture)},{
						_this.DefValue_2_.ToString(CultureInfo.InvariantCulture)},{
						_this.DefValue_3_.ToString(CultureInfo.InvariantCulture)})");
					break;

				case SerializedPropertyType.Float:
				case SerializedPropertyType.Range:
				case SerializedPropertyType.Int:
					writer.Write(_this.DefValue_0_.ToString(CultureInfo.InvariantCulture));
					break;

				case SerializedPropertyType.Texture:
					writer.Write($"\"{_this.DefTexture.DefaultName}\" {{}}");
					break;

				default:
					throw new NotSupportedException($"Serialized property type {_this.Type} isn't supported");
			}
			writer.Write('\n');
		}
	}
}
