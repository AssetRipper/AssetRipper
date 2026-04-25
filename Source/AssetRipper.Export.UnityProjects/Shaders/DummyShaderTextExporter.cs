using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedProperties;
using AssetRipper.SourceGenerated.Subclasses.SerializedProperty;

namespace AssetRipper.Export.UnityProjects.Shaders;

public sealed class DummyShaderTextExporter : ShaderExporterBase
{
	public override bool Export(IExportContainer container, IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		return ExportShader((IShader)asset, path, fileSystem);
	}

	public static bool ExportShader(IShader shader, string path, FileSystem fileSystem)
	{
		using Stream fileStream = fileSystem.File.Create(path);
		using InvariantStreamWriter writer = new(fileStream);
		return ExportShader(shader, writer);
	}

	public static bool ExportShader(IShader shader, TextWriter writer)
	{
		// Technically, this outputs invalid shader code for Unity 5.5 because HLSLPROGRAM was not introduced until Unity 5.6.
		if (shader.Has_ParsedForm())
		{
			writer.Write($"Shader \"{shader.ParsedForm.Name}\" {{\n");
			ISerializedProperties properties = shader.ParsedForm.PropInfo;
			Export(properties, writer);
			ExportGeneratedSubShader(shader, properties, writer);

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
			if (subshaderIndex < 0)
			{
				return false;
			}
			writer.WriteString(header, 0, subshaderIndex);

			writer.Write("\t//DummyShaderTextExporter\n");
			writer.WriteIndent(1);
			writer.Write(DefaultFallbackSubShader);

			writer.Write('}');
		}
		return true;
	}

	private static void ExportGeneratedSubShader(IShader shader, ISerializedProperties properties, TextWriter writer)
	{
		ISerializedProperty? mainTexture = GetPrimaryTextureProperty(properties);
		ISerializedProperty? mainColor = GetPrimaryColorProperty(properties);
		ISerializedProperty? normalMap = GetTextureProperty(properties, "_BumpMap", "_NormalMap");
		ISerializedProperty? metallicGlossMap = GetTextureProperty(properties, "_MetallicGlossMap");
		ISerializedProperty? occlusionMap = GetTextureProperty(properties, "_OcclusionMap");
		ISerializedProperty? emissionMap = GetTextureProperty(properties, "_EmissionMap");
		ISerializedProperty? metallic = GetScalarProperty(properties, "_Metallic");
		ISerializedProperty? smoothness = GetScalarProperty(properties, "_Glossiness", "_Smoothness");
		ISerializedProperty? occlusionStrength = GetScalarProperty(properties, "_OcclusionStrength");
		ISerializedProperty? cutoff = GetScalarProperty(properties, "_Cutoff", "_AlphaClip");
		ISerializedProperty? emissionColor = GetColorProperty(properties, "_EmissionColor");
		bool transparent = IsTransparentShader(shader, properties);
		bool cutout = IsCutoutShader(shader, properties);
		bool hasNormalMap = normalMap is not null && normalMap.DefTexture.TexDim == 2;
		HashSet<string> sampledUvProperties = [];
		AddSampledUvProperty(sampledUvProperties, mainTexture);
		AddSampledUvProperty(sampledUvProperties, normalMap);
		AddSampledUvProperty(sampledUvProperties, metallicGlossMap);
		AddSampledUvProperty(sampledUvProperties, occlusionMap);
		AddSampledUvProperty(sampledUvProperties, emissionMap);

		writer.Write("\t//DummyShaderTextExporter\n");
		writer.WriteIndent(1);
		writer.WriteLine("SubShader{");
		writer.WriteIndent(2);
		writer.Write("Tags { ");
		if (transparent)
		{
			writer.Write("\"Queue\" = \"Transparent\" \"RenderType\" = \"Transparent\"");
		}
		else if (cutout)
		{
			writer.Write("\"Queue\" = \"AlphaTest\" \"RenderType\" = \"TransparentCutout\"");
		}
		else
		{
			writer.Write("\"RenderType\" = \"Opaque\"");
		}
		writer.WriteLine(" }");
		if (transparent)
		{
			writer.WriteIndent(2);
			writer.WriteLine("Blend SrcAlpha OneMinusSrcAlpha");
			writer.WriteIndent(2);
			writer.WriteLine("ZWrite Off");
		}
		else if (cutout)
		{
			writer.WriteIndent(2);
			writer.WriteLine("AlphaToMask On");
		}
		writer.WriteIndent(2);
		writer.WriteLine("LOD 200");
		writer.WriteIndent(2);
		writer.WriteLine("CGPROGRAM");
		writer.WriteIndent(2);
		writer.Write(" #pragma surface surf Standard fullforwardshadows");
		if (transparent)
		{
			writer.Write(" alpha:fade");
		}
		if (cutout && cutoff is not null && IsPropertyNamed(cutoff, "_Cutoff"))
		{
			writer.Write(" alphatest:_Cutoff");
		}
		if (hasNormalMap)
		{
			writer.Write(" addshadow");
		}
		writer.WriteLine();
		writer.WriteIndent(2);
		writer.WriteLine("#pragma target 3.0");
		writer.WriteLine();

		foreach (ISerializedProperty property in properties.Props)
		{
			ExportCgDeclaration(property, writer);
		}

		writer.WriteIndent(2);
		writer.WriteLine("struct Input");
		writer.WriteIndent(2);
		writer.WriteLine("{");
		foreach (string textureName in sampledUvProperties.Order())
		{
			writer.WriteIndent(3);
			writer.WriteLine($"float2 uv_{textureName};");
		}
		writer.WriteIndent(2);
		writer.WriteLine("};");
		writer.WriteLine();

		writer.WriteIndent(2);
		writer.WriteLine("void surf(Input IN, inout SurfaceOutputStandard o)");
		writer.WriteIndent(2);
		writer.WriteLine("{");
		if (mainTexture is not null && mainTexture.DefTexture.TexDim == 2)
		{
			string textureName = SanitizeIdentifier(mainTexture.Name);
			writer.WriteIndent(3);
			writer.WriteLine($"fixed4 c = tex2D({textureName}, IN.uv_{textureName});");
		}
		else
		{
			writer.WriteIndent(3);
			writer.WriteLine("fixed4 c = fixed4(1,1,1,1);");
		}
		if (mainColor is not null)
		{
			writer.WriteIndent(3);
			writer.WriteLine($"c *= {SanitizeIdentifier(mainColor.Name)};");
		}
		if (emissionColor is not null)
		{
			writer.WriteIndent(3);
			writer.WriteLine($"fixed4 emissionTint = {SanitizeIdentifier(emissionColor.Name)};");
		}
		else
		{
			writer.WriteIndent(3);
			writer.WriteLine("fixed4 emissionTint = fixed4(0,0,0,0);");
		}
		writer.WriteIndent(3);
		writer.WriteLine("o.Albedo = c.rgb;");
		writer.WriteIndent(3);
		writer.WriteLine("o.Alpha = c.a;");
		if (hasNormalMap)
		{
			string normalName = SanitizeIdentifier(normalMap!.Name);
			string uvName = SanitizeIdentifier(normalMap.Name);
			writer.WriteIndent(3);
			writer.WriteLine($"o.Normal = UnpackNormal(tex2D({normalName}, IN.uv_{uvName}));");
		}
		if (metallicGlossMap is not null && metallicGlossMap.DefTexture.TexDim == 2)
		{
			string metallicGlossName = SanitizeIdentifier(metallicGlossMap.Name);
			string uvName = SanitizeIdentifier(metallicGlossMap.Name);
			writer.WriteIndent(3);
			writer.WriteLine($"fixed4 metallicGloss = tex2D({metallicGlossName}, IN.uv_{uvName});");
			writer.WriteIndent(3);
			writer.WriteLine("o.Metallic = metallicGloss.r;");
			writer.WriteIndent(3);
			writer.WriteLine("o.Smoothness = metallicGloss.a;");
		}
		else
		{
			writer.WriteIndent(3);
			writer.WriteLine($"o.Metallic = {GetScalarExpression(metallic, "0")};");
			writer.WriteIndent(3);
			writer.WriteLine($"o.Smoothness = {GetScalarExpression(smoothness, "0.5")};");
		}
		if (occlusionMap is not null && occlusionMap.DefTexture.TexDim == 2)
		{
			string occlusionName = SanitizeIdentifier(occlusionMap.Name);
			string uvName = SanitizeIdentifier(occlusionMap.Name);
			writer.WriteIndent(3);
			writer.WriteLine($"fixed occlusionSample = tex2D({occlusionName}, IN.uv_{uvName}).g;");
			writer.WriteIndent(3);
			writer.WriteLine($"o.Occlusion = lerp(1.0, occlusionSample, {GetScalarExpression(occlusionStrength, "1")});");
		}
		if (emissionMap is not null && emissionMap.DefTexture.TexDim == 2)
		{
			string emissionName = SanitizeIdentifier(emissionMap.Name);
			string uvName = SanitizeIdentifier(emissionMap.Name);
			writer.WriteIndent(3);
			writer.WriteLine($"o.Emission = tex2D({emissionName}, IN.uv_{uvName}).rgb * emissionTint.rgb;");
		}
		else if (emissionColor is not null)
		{
			writer.WriteIndent(3);
			writer.WriteLine("o.Emission = emissionTint.rgb;");
		}
		if (cutout && cutoff is not null)
		{
			writer.WriteIndent(3);
			writer.WriteLine($"clip(o.Alpha - {SanitizeIdentifier(cutoff.Name)});");
		}
		else if (cutout)
		{
			writer.WriteIndent(3);
			writer.WriteLine("clip(o.Alpha - 0.5);");
		}
		writer.WriteIndent(2);
		writer.WriteLine("}");
		writer.WriteIndent(2);
		writer.WriteLine("ENDCG");
		writer.WriteIndent(1);
		writer.WriteLine("}");
		writer.WriteLine();
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
				writer.Write($"Range({_this.DefValue_1_.ToStringInvariant()}, {_this.DefValue_2_.ToStringInvariant()})");
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
				writer.Write($"({_this.DefValue_0_.ToStringInvariant()},{_this.DefValue_1_.ToStringInvariant()},{_this.DefValue_2_.ToStringInvariant()},{_this.DefValue_3_.ToStringInvariant()})");
				break;

			case SerializedPropertyType.Float:
			case SerializedPropertyType.Range:
			case SerializedPropertyType.Int:
				writer.Write(_this.DefValue_0_.ToStringInvariant());
				break;

			case SerializedPropertyType.Texture:
				writer.Write($"\"{_this.DefTexture.DefaultName}\" {{}}");
				break;

			default:
				throw new NotSupportedException($"Serialized property type {_this.Type} isn't supported");
		}
		writer.Write('\n');
	}

	private static void ExportCgDeclaration(ISerializedProperty property, TextWriter writer)
	{
		string name = SanitizeIdentifier(property.Name);
		switch (property.GetType_())
		{
			case SerializedPropertyType.Color:
			case SerializedPropertyType.Vector:
				writer.WriteIndent(2);
				writer.WriteLine($"float4 {name};");
				break;
			case SerializedPropertyType.Float:
			case SerializedPropertyType.Range:
				writer.WriteIndent(2);
				writer.WriteLine($"float {name};");
				break;
			case SerializedPropertyType.Int:
				writer.WriteIndent(2);
				writer.WriteLine($"int {name};");
				break;
			case SerializedPropertyType.Texture:
				writer.WriteIndent(2);
				writer.WriteLine($"{GetTextureSamplerType(property)} {name};");
				break;
		}
	}

	private static string GetTextureSamplerType(ISerializedProperty property) => property.DefTexture.TexDim switch
	{
		2 => "sampler2D",
		3 => "sampler3D",
		4 => "samplerCUBE",
		_ => "sampler2D",
	};

	private static ISerializedProperty? GetPrimaryTextureProperty(ISerializedProperties properties)
	{
		return properties.Props.FirstOrDefault(p => p.GetType_() == SerializedPropertyType.Texture && (IsPropertyNamed(p, "_MainTex") || IsPropertyNamed(p, "_BaseMap")))
			?? properties.Props.FirstOrDefault(p => p.GetType_() == SerializedPropertyType.Texture && p.DefTexture.TexDim == 2)
			?? properties.Props.FirstOrDefault(p => p.GetType_() == SerializedPropertyType.Texture);
	}

	private static ISerializedProperty? GetPrimaryColorProperty(ISerializedProperties properties)
	{
		return properties.Props.FirstOrDefault(p => p.GetType_() == SerializedPropertyType.Color && (IsPropertyNamed(p, "_Color") || IsPropertyNamed(p, "_BaseColor") || IsPropertyNamed(p, "_Tint")))
			?? properties.Props.FirstOrDefault(p => p.GetType_() == SerializedPropertyType.Color);
	}

	private static ISerializedProperty? GetTextureProperty(ISerializedProperties properties, params string[] names)
	{
		return properties.Props.FirstOrDefault(p => p.GetType_() == SerializedPropertyType.Texture && names.Any(name => IsPropertyNamed(p, name)));
	}

	private static ISerializedProperty? GetScalarProperty(ISerializedProperties properties, params string[] names)
	{
		return properties.Props.FirstOrDefault(p => (p.GetType_() == SerializedPropertyType.Float || p.GetType_() == SerializedPropertyType.Range) && names.Any(name => IsPropertyNamed(p, name)));
	}

	private static ISerializedProperty? GetColorProperty(ISerializedProperties properties, params string[] names)
	{
		return properties.Props.FirstOrDefault(p => p.GetType_() == SerializedPropertyType.Color && names.Any(name => IsPropertyNamed(p, name)));
	}

	private static string GetScalarExpression(ISerializedProperty? property, string fallback)
	{
		return property is null ? fallback : SanitizeIdentifier(property.Name);
	}

	private static void AddSampledUvProperty(HashSet<string> sampledUvProperties, ISerializedProperty? property)
	{
		if (property is not null && property.GetType_() == SerializedPropertyType.Texture && property.DefTexture.TexDim == 2)
		{
			sampledUvProperties.Add(SanitizeIdentifier(property.Name));
		}
	}

	private static bool IsTransparentShader(IShader shader, ISerializedProperties properties)
	{
		string name = shader.ParsedForm?.Name ?? shader.Name;
		return name.Contains("Transparent", StringComparison.OrdinalIgnoreCase)
			|| name.Contains("Fade", StringComparison.OrdinalIgnoreCase)
			|| name.Contains("Alpha", StringComparison.OrdinalIgnoreCase)
			|| properties.Props.Any(p => IsPropertyNamed(p, "_Surface") || IsPropertyNamed(p, "_Mode") || IsPropertyNamed(p, "_SrcBlend") || IsPropertyNamed(p, "_DstBlend"));
	}

	private static bool IsCutoutShader(IShader shader, ISerializedProperties properties)
	{
		string name = shader.ParsedForm?.Name ?? shader.Name;
		return name.Contains("Cutout", StringComparison.OrdinalIgnoreCase)
			|| name.Contains("AlphaTest", StringComparison.OrdinalIgnoreCase)
			|| properties.Props.Any(p => IsPropertyNamed(p, "_Cutoff") || IsPropertyNamed(p, "_AlphaClip"));
	}

	private static bool IsPropertyNamed(ISerializedProperty property, string name)
	{
		return property.Name.String == name;
	}

	private static string SanitizeIdentifier(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return "_Property";
		}

		char[] chars = name.ToCharArray();
		for (int i = 0; i < chars.Length; i++)
		{
			if (!(char.IsLetterOrDigit(chars[i]) || chars[i] == '_'))
			{
				chars[i] = '_';
			}
		}
		if (!(char.IsLetter(chars[0]) || chars[0] == '_'))
		{
			return "_" + new string(chars);
		}
		return new string(chars);
	}

	// This uses CGPROGRAM instead of HLSLPROGRAM because the latter was supposedly introduced in Unity 5.6.
	// https://github.com/UnityCommunity/UnityReleaseNotes/blob/7b417b8ff64415e1e509d8c345b829c7cc11b650/5.6-Beta/5.6.0b1.txt#L143
	private static string DefaultFallbackSubShader { get; } = """

			SubShader{
				Tags { "RenderType" = "Opaque" }
				LOD 200
				CGPROGRAM
		#pragma surface surf Lambert
		#pragma target 3.0
				sampler2D _MainTex;
				struct Input
				{
					float2 uv_MainTex;
				};
				void surf(Input IN, inout SurfaceOutput o)
				{
					float4 c = tex2D(_MainTex, IN.uv_MainTex);
					o.Albedo = c.rgb;
				}
				ENDCG
			}

		""".Replace("\r", "");
}
