using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.SerializedShader;
using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;
using System;
using System.Globalization;
using System.IO;

namespace AssetRipper.Library.Exporters.Shaders
{
	public class DummyShaderTextExporter : BinaryAssetExporter
	{
		private const string FALLBACK_DUMMY_SHADER = @"
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

		private readonly ShaderExportMode exportMode;

		public DummyShaderTextExporter(LibraryConfiguration configuration)
		{
			exportMode = configuration.ShaderExportMode;
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return exportMode is ShaderExportMode.Dummy && asset is IShader shader && (shader.HasParsedForm || asset is ITextAsset);
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, "shader");
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			IShader shader = (IShader)asset;

			//Importing Hidden/Internal shaders causes the unity editor screen to turn black
			if (shader.ParsedForm?.Name?.StartsWith("Hidden/Internal", StringComparison.Ordinal) ?? false)
				return false;

			using (FileStream fileStream = File.Create(path))
			{
				ExportShader(shader, container, fileStream);
			}
			return true;
		}

		public static void ExportShader(IShader shader, IExportContainer container, Stream stream)
		{
			if (shader.HasParsedForm)
			{
				using InvariantStreamWriter writer = new InvariantStreamWriter(stream);
				writer.Write("Shader \"{0}\" {{\n", shader.ParsedForm.Name);
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
					writer.Write(FALLBACK_DUMMY_SHADER.Replace("\r", ""));
				}
				writer.Write('\n');

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
			else if (shader is ITextAsset textAsset)
			{
				using InvariantStreamWriter writer = new InvariantStreamWriter(stream);
				string header = textAsset.ParseWithUTF8();
				var subshaderIndex = header.IndexOf("SubShader");
				writer.WriteString(header, 0, subshaderIndex);

				writer.Write("\t//DummyShaderTextExporter\n");
				writer.WriteIndent(1);
				writer.Write(FALLBACK_DUMMY_SHADER.Replace("\r", ""));

				writer.Write('}');
			}
			else
			{
				throw new NotSupportedException();
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
			foreach (string attribute in _this.Attributes.ToStringArray())
			{
				writer.Write("[{0}] ", attribute);
			}
			if (_this.Flags.IsHideInInspector())
			{
				writer.Write("[HideInInspector] ");
			}
			if (_this.Flags.IsPerRendererData())
			{
				writer.Write("[PerRendererData] ");
			}
			if (_this.Flags.IsNoScaleOffset())
			{
				writer.Write("[NoScaleOffset] ");
			}
			if (_this.Flags.IsNormal())
			{
				writer.Write("[Normal] ");
			}
			if (_this.Flags.IsHDR())
			{
				writer.Write("[HDR] ");
			}
			if (_this.Flags.IsGamma())
			{
				writer.Write("[Gamma] ");
			}

			writer.Write("{0} (\"{1}\", ", _this.Name, _this.Description);

			switch (_this.Type)
			{
				case SerializedPropertyType.Color:
				case SerializedPropertyType.Vector:
					writer.Write(nameof(SerializedPropertyType.Vector));
					break;

				case SerializedPropertyType.Int:
					//case SerializedPropertyType.Float:
					writer.Write(nameof(SerializedPropertyType.Float));
					break;

				case SerializedPropertyType.Range:
					writer.Write("{0}({1}, {2})",
						nameof(SerializedPropertyType.Range),
						_this.DefValue1.ToString(CultureInfo.InvariantCulture),
						_this.DefValue2.ToString(CultureInfo.InvariantCulture));
					break;

				case SerializedPropertyType._2D:
					//case SerializedPropertyType._3D:
					//case SerializedPropertyType.Cube:
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
							writer.Write(nameof(SerializedPropertyType.Cube));
							break;
						case 5:
							writer.Write("2DArray");
							break;
						case 6:
							writer.Write(nameof(SerializedPropertyType.CubeArray));
							break;
						default:
							throw new NotSupportedException("Texture dimension isn't supported");

					}
					break;

				default:
					throw new NotSupportedException($"Serialized property type {_this.Type} isn't supported");
			}
			writer.Write(") = ");

			switch (_this.Type)
			{
				case SerializedPropertyType.Color:
				case SerializedPropertyType.Vector:
					writer.Write("({0},{1},{2},{3})",
						_this.DefValue0.ToString(CultureInfo.InvariantCulture),
						_this.DefValue1.ToString(CultureInfo.InvariantCulture),
						_this.DefValue2.ToString(CultureInfo.InvariantCulture),
						_this.DefValue3.ToString(CultureInfo.InvariantCulture));
					break;

				case SerializedPropertyType.Int:
				//case SerializedPropertyType.Float:
				case SerializedPropertyType.Range:
					writer.Write(_this.DefValue0.ToString(CultureInfo.InvariantCulture));
					break;

				case SerializedPropertyType._2D:
					//case SerializedPropertyType._3D:
					//case SerializedPropertyType.Cube:
					writer.Write("\"{0}\" {{}}", _this.DefTexture.DefaultName);
					break;

				default:
					throw new NotSupportedException($"Serialized property type {_this.Type} isn't supported");
			}
			writer.Write('\n');
		}
	}
}
