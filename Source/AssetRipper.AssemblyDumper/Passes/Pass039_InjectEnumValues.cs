using AssetRipper.DocExtraction.DataStructures;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass039_InjectEnumValues
{
	private static readonly List<InjectedTypeData> injectedTypes = new()
	{
		new()
		{
			Name = "CurveLoopTypes",
			Summary = "Enum for AnimationCurve.PreInfinity and AnimationCurve.PostInfinity",
			Members = new()
			{
				("Constant", 0, ""),
				("Cycle", 1, ""),
				("CycleWithOffset", 2, ""),
				("Oscillate", 3, ""),
				("Linear", 4, ""),
			},
		},
	};
	private static readonly Dictionary<string, List<(string, long, string?)>> injectedValues = new()
	{
		{ "UnityEngine.TextureFormat",
			new()
			{
				("DXT3", 11, ""),
			} },
		{ "UnityEditor.TextureImporterFormat",
			new()
			{
				("DXT3", 11, ""),
			} },
		{ "UnityEngine.MeshTopology",
			new()
			{
				("TriangleStrip", 1, "Mesh is a triangle strip."),
			} },
		{ "UnityEngine.SpritePackingRotation",
			new()
			{
				("Rotate90", 4, "Might not exist. It was included in legacy code."),
				("Any_Old", 5, "Might not exist. It was included in legacy code."),
			} },
		{ "UnityEngine.AnimatorControllerParameterType",
			new()
			{
				("Vector", 0, "Added to allow merging. Remove once complex merging is implemented."),
			} },
		{ "UnityEditor.Animations.AnimatorConditionMode",
			new()
			{
				("ExitTime", 5, "The condition is true when the source state has stepped over the exit time value."),
			} },
	};
	private static readonly Dictionary<string, List<(string?, string)>> injectedDocumentation = new()
	{
		{ "UnityEditor.ModelImporterMeshCompression",
			new()
			{
				(null, "Compressing meshes saves space in the built game, but more compression introduces more artifacts in vertex data."),
			}
		},
		{ "UnityEditor.TextureUsageMode",
			new()
			{
				("Default", "Not a lightmap"),
				("LightmapDoubleLDR", "Range [0;2] packed to [0;1] with loss of precision"),
				("BakedLightmapDoubleLDR", "Range [0;2] packed to [0;1] with loss of precision"),
				("LightmapRGBM", "Range [0;kLightmapRGBMMax] packed to [0;1] with multiplier stored in the alpha channel"),
				("BakedLightmapRGBM", "Range [0;kLightmapRGBMMax] packed to [0;1] with multiplier stored in the alpha channel"),
				("NormalmapDXT5nm", "Compressed DXT5 normal map"),
				("NormalmapPlain", "Plain RGB normal map"),
				("AlwaysPadded", "Texture is always padded if NPOT and on low-end hardware"),
				("BakedLightmapFullHDR", "Baked lightmap without any encoding"),
			} },
	};

	public static void DoPass()
	{
		Dictionary<string, EnumHistory> dictionary = SharedState.Instance.HistoryFile.Enums;

		//Inject types
		foreach (InjectedTypeData injectedTypeData in injectedTypes)
		{
			UnityVersion minVersion = SharedState.Instance.MinSourceVersion;
			EnumHistory history = new();
			history.Name = injectedTypeData.Name;
			history.FullName = $"Injected.{injectedTypeData.Name}";
			history.IsFlagsEnum = injectedTypeData.IsFlags;
			history.ElementType.Add(minVersion, injectedTypeData.ElementType);
			history.NativeName.Add(minVersion, null);
			history.DocumentationString.Add(minVersion, injectedTypeData.Summary);
			history.ObsoleteMessage.Add(minVersion, null);
			history.Exists.Add(minVersion, true);
			foreach ((string fieldName, long value, string? description) in injectedTypeData.Members)
			{
				EnumMemberHistory member = new();
				member.Name = fieldName;
				member.NativeName.Add(minVersion, null);
				member.Value.Add(minVersion, value);
				member.DocumentationString.Add(minVersion, string.IsNullOrEmpty(description) ? null : description);
				member.ObsoleteMessage.Add(minVersion, null);
				member.Exists.Add(minVersion, true);
				history.Members.Add(fieldName, member);
			}
			dictionary.Add(history.FullName, history);
		}

		//Inject values
		foreach ((string fullName, List<(string, long, string?)> list) in injectedValues)
		{
			EnumHistory history = dictionary[fullName];
			UnityVersion minVersion = history.Exists[0].Key;
			foreach ((string fieldName, long value, string? description) in list)
			{
				if (history.Members.ContainsKey(fieldName))
				{
					Console.WriteLine($"{fullName} already has an entry for {fieldName}");
				}
				else
				{
					EnumMemberHistory member = new();
					member.Name = fieldName;
					member.NativeName.Add(minVersion, null);
					member.Value.Add(minVersion, value);
					member.DocumentationString.Add(minVersion, string.IsNullOrEmpty(description) ? "Injected" : $"Injected. {description}");
					member.ObsoleteMessage.Add(minVersion, null);
					member.Exists.Add(minVersion, true);
					history.Members.Add(fieldName, member);
				}
			}
		}

		//Inject documentation
		foreach ((string fullName, List<(string?, string)> list) in injectedDocumentation)
		{
			EnumHistory history = dictionary[fullName];
			foreach ((string? fieldName, string description) in list)
			{
				if (string.IsNullOrEmpty(fieldName))
				{
					//Inject documentation for the enum itself
					history.InjectedDocumentation = description;
				}
				else
				{
					EnumMemberHistory member = history.Members[fieldName];
					member.InjectedDocumentation = description;
				}
			}
		}

		//Fix TextureFormat
		{
			EnumHistory history = dictionary["UnityEngine.TextureFormat"];
			foreach (EnumMemberHistory member in history.Members.Values)
			{
				if (member.Value.Count == 2 && member.Value[1].Value == -127)
				{
					member.Value.RemoveAt(1);
				}
			}
		}
	}

	private record class InjectedTypeData
	{
		public required string Name { get; init; }
		public string? Summary { get; init; }
		public bool IsFlags { get; init; } = false;
		public ElementType ElementType { get; init; } = ElementType.I4;
		public required List<(string, long, string?)> Members { get; init; }
	}
}
