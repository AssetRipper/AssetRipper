using AssetRipper.AssemblyDumper.Enums;
using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.DocExtraction.Extensions;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass055_CreateEnumProperties
{
	public static void DoPass()
	{
		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			foreach (InterfaceProperty interfaceProperty in group.InterfaceProperties)
			{
				if (interfaceProperty.TryGetEnumFullName(out string? fullName) && Pass040_AddEnums.EnumDictionary.TryGetValue(fullName, out (TypeDefinition, EnumDefinitionBase) tuple))
				{
					CreateEnumProperty(group, interfaceProperty, tuple.Item1);
				}
			}
		}
	}

	private static void CreateEnumProperty(ClassGroupBase group, InterfaceProperty interfaceProperty, TypeDefinition enumType)
	{
		ElementType enumElementType = ((CorLibTypeSignature)enumType.GetFieldByName("value__").Signature!.FieldType).ElementType;
		string propertyName = $"{interfaceProperty.Definition.Name}E";

		interfaceProperty.SpecialDefinition = group.Interface.AddFullProperty(
			propertyName,
			InterfaceUtils.InterfacePropertyDeclaration,
			enumType.ToTypeSignature());

		foreach (ClassProperty classProperty in interfaceProperty.Implementations)
		{
			if (classProperty.BackingField?.Signature?.FieldType is CorLibTypeSignature fieldTypeSignature
				&& fieldTypeSignature.ElementType.IsFixedSizeInteger())
			{
				classProperty.SpecialDefinition = classProperty.Class.Type.AddFullProperty(
					propertyName,
					InterfaceUtils.InterfacePropertyImplementation,
					enumType.ToTypeSignature());
				classProperty.SpecialDefinition.GetMethod!.GetInstructions().FillGetter(classProperty.BackingField, fieldTypeSignature.ElementType, enumElementType);
				classProperty.SpecialDefinition.SetMethod!.GetInstructions().FillSetter(classProperty.BackingField, fieldTypeSignature.ElementType, enumElementType);
				if (classProperty.Class.Type.IsAbstract)
				{
					classProperty.SpecialDefinition.AddDebuggerBrowsableNeverAttribute();//Properties in base classes are redundant in the debugger.
				}
			}
			else
			{
				classProperty.SpecialDefinition = classProperty.Class.Type.ImplementFullProperty(
					propertyName,
					InterfaceUtils.InterfacePropertyImplementation,
					enumType.ToTypeSignature(),
					null);
				classProperty.SpecialDefinition.AddDebuggerBrowsableNeverAttribute();//Dummy properties should not be visible in the debugger.
			}
		}
	}

	private static bool TryGetEnumFullName(this InterfaceProperty interfaceProperty, [NotNullWhen(true)] out string? fullName)
	{
		if (interfaceProperty.Group is ClassGroup
			&& (interfaceProperty.Name == "HideFlags" || interfaceProperty.Name.StartsWith("HideFlags_C", StringComparison.Ordinal)))
		{
			fullName = "UnityEngine.HideFlags";
			return true;
		}
		interfaceProperty.TryGetEnumFullNameFromHistory(out string? metadata);
		if (interfaceProperty.TryGetOverridenEnumFullName(out string? overriden))
		{
			fullName = metadata != overriden
				? overriden
				: throw new Exception($"{interfaceProperty.Group.Name}.{interfaceProperty.Name} did not need to be overriden.");
		}
		else
		{
			fullName = metadata;
		}
		return fullName is not null;
	}

	private static bool TryGetEnumFullNameFromHistory(this InterfaceProperty interfaceProperty, [NotNullWhen(true)] out string? fullName)
	{
		if (interfaceProperty.History is not null && interfaceProperty.History.TypeFullName.Count == 1)
		{
			fullName = interfaceProperty.History.TypeFullName[0].Value.ToString();
			return true;
		}
		else
		{
			fullName = null;
			return false;
		}
	}

	private static bool TryGetOverridenEnumFullName(this InterfaceProperty interfaceProperty, out string? fullName)
	{
		if (interfaceProperty.Group switch { _ => false })
		{
			fullName = null;
			return true;
		}

		fullName = interfaceProperty.Group switch
		{
			ClassGroup classGroup => classGroup.ID switch
			{
				1 => interfaceProperty.Name switch
				{
					"StaticEditorFlags" => "UnityEditor.StaticEditorFlags",
					_ => null,
				},
				11 => interfaceProperty.Name switch
				{
					"Default_Speaker_Mode" => "UnityEngine.AudioSpeakerMode",
					_ => null,
				},
				28 => interfaceProperty.Name switch
				{
					"ColorSpace_C28" => "UnityEngine.ColorSpace",
					"LightmapFormat_C28" => "UnityEditor.TextureUsageMode",
					_ => null,
				},
				30 => interfaceProperty.Name switch
				{
					"DefaultMobileRenderingPath" => "UnityEngine.RenderingPath",
					"DefaultRenderingPath" => "UnityEngine.RenderingPath",
					_ => null,
				},
				43 => interfaceProperty.Name switch
				{
					"MeshCompression" => "UnityEditor.ModelImporterMeshCompression",
					_ => null,
				},
				83 => interfaceProperty.Name switch
				{
					"CompressionFormat" => "UnityEngine.AudioCompressionFormat",
					_ => null,
				},
				117 => interfaceProperty.Name switch
				{
					"ColorSpace" => "UnityEngine.ColorSpace",
					"Dimension" => "UnityEngine.Rendering.TextureDimension",
					"LightmapFormat" => "UnityEditor.TextureUsageMode",
					"UsageMode" => "UnityEditor.TextureUsageMode",
					_ => null,
				},
				187 => interfaceProperty.Name switch
				{
					"ColorSpace" => "UnityEngine.ColorSpace",
					"UsageMode" => "UnityEditor.TextureUsageMode",
					_ => null,
				},
				188 => interfaceProperty.Name switch
				{
					"ColorSpace" => "UnityEngine.ColorSpace",
					"UsageMode" => "UnityEditor.TextureUsageMode",
					_ => null,
				},
				206 => interfaceProperty.Name switch
				{
					"BlendType_Int32" => "UnityEditor.Animations.BlendTreeType",
					"BlendType_UInt32" => "UnityEditor.Animations.BlendTreeType",
					_ => null,
				},
				1006 => interfaceProperty.Name switch
				{
					"Alignment" => "UnityEngine.SpriteAlignment",
					"AlphaUsage" => "UnityEditor.TextureImporterAlphaSource",
					"SpriteMeshType" => "UnityEngine.SpriteMeshType",
					_ => null,
				},
				_ => null,
			},
			SubclassGroup subclassGroup => subclassGroup.Name switch
			{
				"AnimationCurve_Single" or "AnimationCurve_Vector3f" or "AnimationCurve_Quaternionf" => interfaceProperty.Name switch
				{
					"PreInfinity" => "Injected.CurveLoopTypes",
					"PostInfinity" => "Injected.CurveLoopTypes",
					"RotationOrder" => "UnityEngine.RotationOrder",
					_ => null,
				},
				"BlendTreeNodeConstant" => interfaceProperty.Name switch
				{
					"BlendType" => "UnityEditor.Animations.BlendTreeType",
					_ => null,
				},
				"ConditionConstant" => interfaceProperty.Name switch
				{
					"ConditionMode" => "UnityEditor.Animations.AnimatorConditionMode",
					_ => null,
				},
				"Keyframe_Single" or "Keyframe_Vector3f" or "Keyframe_Quaternionf" => interfaceProperty.Name switch
				{
					"WeightedMode" => "UnityEngine.WeightedMode",
					_ => null,
				},
				"SubMesh" => interfaceProperty.Name switch
				{
					"Topology" or "IsTriStrip" => "UnityEngine.MeshTopology",
					_ => null,
				},
				"TextureImporterBumpMapSettings" => interfaceProperty.Name switch
				{
					"NormalMapFilter" => "UnityEditor.TextureImporterNormalFilter",
					_ => null,
				},
				"TextureImporterMipMapSettings" => interfaceProperty.Name switch
				{
					"MipMapMode" => "UnityEditor.TextureImporterMipFilter",
					_ => null,
				},
				"TextureImporterPlatformSettings" => interfaceProperty.Name switch
				{
					"AndroidETC2FallbackOverride" => "UnityEditor.AndroidETC2FallbackOverride",
					"Format" => "UnityEditor.TextureImporterFormat",
					"ResizeAlgorithm" => "UnityEditor.TextureResizeAlgorithm",
					"TextureCompression" => "UnityEditor.TextureImporterCompression",
					_ => null,
				},
				"TextureSettings" => interfaceProperty.Name switch
				{
					"FilterMode" => "UnityEngine.FilterMode",
					"TextureCompression" => "UnityEditor.TextureImporterCompression",
					_ => null,
				},
				"TierGraphicsSettings" => interfaceProperty.Name switch
				{
					"HdrMode" => "UnityEngine.Rendering.CameraHDRMode",
					"RealtimeGICPUUsage" => "UnityEngine.Rendering.RealtimeGICPUUsage",
					"RenderingPath" => "UnityEngine.RenderingPath",
					_ => null,
				},
				"TierGraphicsSettingsEditor" => interfaceProperty.Name switch
				{
					"HdrMode" => "UnityEngine.Rendering.CameraHDRMode",
					"RealtimeGICPUUsage" => "UnityEngine.Rendering.RealtimeGICPUUsage",
					"RenderingPath" => "UnityEngine.RenderingPath",
					"StandardShaderQuality" => "UnityEditor.Rendering.ShaderQuality",
					_ => null,
				},
				"TierSettings" => interfaceProperty.Name switch
				{
					"Tier" => "UnityEngine.Rendering.GraphicsTier",
					_ => null,
				},
				"TransitionConstant" => interfaceProperty.Name switch
				{
					"InterruptionSource" => "UnityEditor.Animations.TransitionInterruptionSource",
					_ => null,
				},
				"UVModule" => interfaceProperty.Name switch
				{
					"AnimationType" => "UnityEngine.ParticleSystemAnimationType",
					"Mode" => "UnityEngine.ParticleSystemAnimationMode",
					"RowMode" => "UnityEngine.ParticleSystemAnimationRowMode",
					"TimeMode" => "UnityEngine.ParticleSystemAnimationTimeMode",
					_ => null,
				},
				"ValueConstant" => interfaceProperty.Name switch
				{
					"Type" => "UnityEngine.AnimatorControllerParameterType",
					_ => null,
				},
				"VariantInfo" => interfaceProperty.Name switch
				{
					"PassType" => "UnityEngine.Rendering.PassType",
					_ => null,
				},
				_ => null,
			},
			_ => throw new(),
		};
		return fullName is not null;
	}

	private static CilInstruction? AddConversion(this CilInstructionCollection instructions, ElementType from, ElementType to)
	{
		if (from == to)
		{
			return null;
		}

		CilOpCode opCode = to switch
		{
			//ElementType.I1 => from.IsSigned() ? CilOpCodes.Conv_Ovf_I1 : CilOpCodes.Conv_Ovf_I1_Un,
			//ElementType.U1 => from.IsSigned() ? CilOpCodes.Conv_Ovf_U1 : CilOpCodes.Conv_Ovf_U1_Un,
			//ElementType.I2 => from.IsSigned() ? CilOpCodes.Conv_Ovf_I2 : CilOpCodes.Conv_Ovf_I2_Un,
			//ElementType.U2 => from.IsSigned() ? CilOpCodes.Conv_Ovf_U2 : CilOpCodes.Conv_Ovf_U2_Un,
			//ElementType.I4 => from.IsSigned() ? CilOpCodes.Conv_Ovf_I4 : CilOpCodes.Conv_Ovf_I4_Un,
			//ElementType.U4 => from.IsSigned() ? CilOpCodes.Conv_Ovf_U4 : CilOpCodes.Conv_Ovf_U4_Un,
			//ElementType.I8 => from.IsSigned() ? CilOpCodes.Conv_Ovf_I8 : CilOpCodes.Conv_Ovf_I8_Un,
			//ElementType.U8 => from.IsSigned() ? CilOpCodes.Conv_Ovf_U8 : CilOpCodes.Conv_Ovf_U8_Un,
			ElementType.I1 => CilOpCodes.Conv_I1,
			ElementType.U1 => CilOpCodes.Conv_U1,
			ElementType.I2 => CilOpCodes.Conv_I2,
			ElementType.U2 => CilOpCodes.Conv_U2,
			ElementType.I4 => CilOpCodes.Conv_I4,
			ElementType.U4 => CilOpCodes.Conv_U4,
			ElementType.I8 => CilOpCodes.Conv_I8,
			ElementType.U8 => CilOpCodes.Conv_U8,
			_ => throw new ArgumentOutOfRangeException(nameof(to)),
		};

		return instructions.Add(opCode);
	}

	private static void FillGetter(this CilInstructionCollection instructions, FieldDefinition field, ElementType fieldType, ElementType enumType)
	{
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, field);
		instructions.AddConversion(fieldType, enumType);
		instructions.Add(CilOpCodes.Ret);
	}

	private static void FillSetter(this CilInstructionCollection instructions, FieldDefinition field, ElementType fieldType, ElementType enumType)
	{
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.AddConversion(enumType, fieldType);
		instructions.Add(CilOpCodes.Stfld, field);
		instructions.Add(CilOpCodes.Ret);
	}
}
