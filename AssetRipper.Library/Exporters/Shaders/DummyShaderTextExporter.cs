using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
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

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IShader;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset);
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			IShader shader = (IShader)asset;

			//Importing Hidden/Internal shaders causes the unity editor screen to turn black
			if (shader.ParsedForm_C48?.NameString?.StartsWith("Hidden/Internal", StringComparison.Ordinal) ?? false)
			{
				return false;
			}

			using (FileStream fileStream = File.Create(path))
			{
				ExportShader(shader, container, fileStream);
			}
			return true;
		}

		public static void ExportShader(IShader shader, IExportContainer container, Stream stream)
		{
			if (shader.Has_ParsedForm_C48())
			{
				using InvariantStreamWriter writer = new InvariantStreamWriter(stream);
				writer.Write("Shader \"{0}\" {{\n", shader.ParsedForm_C48.NameString);
				Export(shader.ParsedForm_C48.PropInfo, writer);

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

				if (shader.ParsedForm_C48.FallbackName != string.Empty)
				{
					writer.WriteIndent(1);
					writer.Write("Fallback \"{0}\"\n", shader.ParsedForm_C48.FallbackName);
				}
				if (shader.ParsedForm_C48.CustomEditorName != string.Empty)
				{
					writer.WriteIndent(1);
					writer.Write("//CustomEditor \"{0}\"\n", shader.ParsedForm_C48.CustomEditorName);
				}
				writer.Write('}');
			}
			else if (shader is ITextAsset textAsset)
			{
				using InvariantStreamWriter writer = new InvariantStreamWriter(stream);
				string header = textAsset.Script_C49.String;
				int subshaderIndex = header.IndexOf("SubShader");
				writer.WriteString(header, 0, subshaderIndex);

				writer.Write("\t//DummyShaderTextExporter\n");
				writer.WriteIndent(1);
				writer.Write(FALLBACK_DUMMY_SHADER.Replace("\r", ""));

				writer.Write('}');
			}
			else //should never happen
			{
				throw new NotSupportedException();
			}
		}

		private static void Export(SourceGenerated.Subclasses.SerializedProperties.ISerializedProperties _this, TextWriter writer)
		{
			writer.WriteIndent(1);
			writer.Write("Properties {\n");
			foreach (SourceGenerated.Subclasses.SerializedProperty.ISerializedProperty prop in _this.Props)
			{
				Export(prop, writer);
			}
			writer.WriteIndent(1);
			writer.Write("}\n");
		}

		private static void Export(SourceGenerated.Subclasses.SerializedProperty.ISerializedProperty _this, TextWriter writer)
		{
			writer.WriteIndent(2);
			foreach (SourceGenerated.Subclasses.Utf8String.Utf8String? attribute in _this.Attributes)
			{
				writer.Write("[{0}] ", attribute);
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

			writer.Write("{0} (\"{1}\", ", _this.NameString, _this.Description);

			switch ((SerializedPropertyType)_this.Type)
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
						_this.DefValue_1_.ToString(CultureInfo.InvariantCulture),
						_this.DefValue_2_.ToString(CultureInfo.InvariantCulture));
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

				default:
					throw new NotSupportedException($"Serialized property type {_this.Type} isn't supported");
			}
			writer.Write(") = ");

			switch ((SerializedPropertyType)_this.Type)
			{
				case SerializedPropertyType.Color:
				case SerializedPropertyType.Vector:
					writer.Write("({0},{1},{2},{3})",
						_this.DefValue_0_.ToString(CultureInfo.InvariantCulture),
						_this.DefValue_1_.ToString(CultureInfo.InvariantCulture),
						_this.DefValue_2_.ToString(CultureInfo.InvariantCulture),
						_this.DefValue_3_.ToString(CultureInfo.InvariantCulture));
					break;

				case SerializedPropertyType.Int:
				//case SerializedPropertyType.Float:
				case SerializedPropertyType.Range:
					writer.Write(_this.DefValue_0_.ToString(CultureInfo.InvariantCulture));
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
