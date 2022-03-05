using AssemblyDumper.Unity;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace AssemblyDumper.Passes
{
	public static class Pass002_RenameSubnodes
	{
		private static readonly Regex badCharactersRegex = new Regex(@"[<>\[\]\s&\(\):]", RegexOptions.Compiled);
		public const string Utf8StringName = "Utf8String";
		private const string OffsetPtrName = "OffsetPtr";
		private const string KeyframeName = "Keyframe";
		private const string AnimationCurveName = "AnimationCurve";
		private const string ColorRGBAName = "ColorRGBA";
		private const string PackedBitVectorName = "PackedBitVector";
		private const string VFXEntryExposedName = "VFXEntryExposed";
		private const string VFXEntryExpressionValueName = "VFXEntryExpressionValue";
		private const string VFXFieldName = "VFXField";
		private const string VFXPropertySheetSerializedBaseName = "VFXPropertySheetSerializedBase";
		private const string TilemapRefCountedDataName = "TilemapRefCountedData";
		private const string Blend1dConstantName = "Blend1dDataConstant";
		private const string Blend2dConstantName = "Blend2dDataConstant";

		public static void DoPass()
		{
			Console.WriteLine("Pass 002: Rename Class Subnodes");
			foreach (UnityClass unityClass in SharedState.ClassDictionary.Values)
			{
				unityClass.CorrectInheritedTypeNames();
				unityClass.EditorRootNode?.FixNamesRecursively();
				unityClass.ReleaseRootNode?.FixNamesRecursively();
				unityClass.EditorRootNode?.DoSecondaryRenamingRecursively();
				unityClass.ReleaseRootNode?.DoSecondaryRenamingRecursively();
			}
		}

		/// <summary>
		/// Corrects the root nodes of classes to have the correct Type Name.<br/>
		/// For example, Behaviour uses Component as its type name in the root nodes
		/// </summary>
		/// <param name="unityClass"></param>
		private static void CorrectInheritedTypeNames(this UnityClass unityClass)
		{
			if (unityClass.EditorRootNode != null && unityClass.EditorRootNode.TypeName != unityClass.Name)
			{
				//Console.WriteLine($"Correcting editor type name from {unityClass.EditorRootNode.TypeName} to {unityClass.Name}");
				unityClass.EditorRootNode.TypeName = unityClass.Name;
			}
			if (unityClass.ReleaseRootNode != null && unityClass.ReleaseRootNode.TypeName != unityClass.Name)
			{
				//Console.WriteLine($"Correcting release type name from {unityClass.ReleaseRootNode.TypeName} to {unityClass.Name}");
				unityClass.ReleaseRootNode.TypeName = unityClass.Name;
			}
		}

		/// <summary>
		/// Fix all type and field names to be valid if decompiled<br/>
		/// For example, it uses a regex to replace invalid characters with an underscore, ie data[0] to data_0_
		/// </summary>
		/// <param name="node"></param>
		private static void FixNamesRecursively(this UnityNode node)
		{
			if (node == null)
			{
				return;
			}

			node.OriginalName = node.Name;
			node.OriginalTypeName = node.TypeName;

			node.Name = GetValidName(node.Name!);
			if (!PrimitiveTypes.primitives.Contains(node.TypeName)) //don't rename special type names like long long, map, or Array
			{
				node.TypeName = GetValidTypeName(node.TypeName!);
			}
			if (node.SubNodes != null)
			{
				foreach (UnityNode subnode in node.SubNodes)
				{
					subnode?.FixNamesRecursively();
				}
			}
		}

		/// <summary>
		/// Fixes the string to be a valid field name
		/// </summary>
		/// <param name="originalName"></param>
		/// <returns>A new string with the valid content</returns>
		private static string GetValidName(string originalName)
		{
			if (string.IsNullOrWhiteSpace(originalName))
			{
				throw new ArgumentException("Nodes cannot have a null or whitespace type name", nameof(originalName));
			}
			string result = originalName.ReplaceBadCharacters();
			if (char.IsDigit(result[0]))
			{
				result = "_" + result;
			}
			return result;
		}

		/// <summary>
		/// Fixes the string to be a valid type name
		/// </summary>
		/// <param name="originalName"></param>
		/// <returns>A new string with the valid content</returns>
		private static string GetValidTypeName(string originalName)
		{
			string result = GetValidName(originalName);
			if (char.IsLower(result[0]) && result.Length > 1)
			{
				result = char.ToUpperInvariant(result[0]) + result.Substring(1);
			}
			return result;
		}

		private static string ReplaceBadCharacters(this string str) => badCharactersRegex.Replace(str, "_");

		private static void DoSecondaryRenamingRecursively(this UnityNode node)
		{
			if (node == null)
			{
				return;
			}

			if (node.SubNodes != null)
			{
				foreach (UnityNode subnode in node.SubNodes)
				{
					subnode.DoSecondaryRenamingRecursively();
				}
			}

			node.DoSecondaryRenaming();
		}

		private static void DoSecondaryRenaming(this UnityNode node)
		{
			if (node.TypeName == "string")
			{
				ChangeStringToUtf8String(node);
			}
			else if (node.IsOffsetPtr(out string? offsetPtrElement))
			{
				node.TypeName = $"{OffsetPtrName}_{offsetPtrElement}";
			}
			else if (node.IsKeyframe(out string? keyframeElement))
			{
				node.TypeName = $"{KeyframeName}_{keyframeElement}";
			}
			else if (node.IsAnimationCurve(out string? animationCurveElement))
			{
				node.TypeName = $"{AnimationCurveName}_{animationCurveElement}";
			}
			else if (node.IsColorRGBA(out string? newColorName))
			{
				node.TypeName = newColorName;
			}
			else if (node.IsPackedBitVector(out string? newBitVectorName))
			{
				node.TypeName = newBitVectorName;
			}
			else if (node.IsEditorScene())
			{
				node.TypeName = "EditorScene";
			}
			else if (node.IsVFXEntryExposed(out string? vfxEntryExposedElement))
			{
				node.TypeName = $"{VFXEntryExposedName}_{vfxEntryExposedElement}";
			}
			else if (node.IsVFXEntryExpressionValue(out string? vfxEntryExpressionValueElement))
			{
				node.TypeName = $"{VFXEntryExpressionValueName}_{vfxEntryExpressionValueElement}";
			}
			else if (node.IsVFXField(out string? vfxFieldElement))
			{
				node.TypeName = $"{VFXFieldName}_{vfxFieldElement}";
			}
			else if (node.IsVFXPropertySheetSerializedBase(out string? vfxPropertySheetElement))
			{
				node.TypeName = $"{VFXPropertySheetSerializedBaseName}_{vfxPropertySheetElement}";
			}
			else if (node.IsTilemapRefCountedData(out string? tilemapRefCountedDataElement))
			{
				node.TypeName = $"{TilemapRefCountedDataName}_{tilemapRefCountedDataElement}";
			}
			else if (node.IsBlend1dAs2d())
			{
				node.TypeName = Blend1dConstantName;
			}
		}

		private static void ChangeStringToUtf8String(UnityNode node)
		{
			node.TypeName = Utf8StringName;
			List<UnityNode> subnodes = node.SubNodes!;
			if(subnodes.Count != 1)
			{
				throw new Exception($"String has {subnodes.Count} subnodes");
			}
			UnityNode subnode = subnodes[0];
			if(subnode.TypeName == "Array")
			{
				subnode.Name = "data";
			}
			else if (subnode.TypeName == Utf8StringName)
			{
				//ExposedReferenceTable on late 2019 and after
				Console.WriteLine("Warning: modifying type tree for string");
				subnodes[0] = subnode.SubNodes![0];
			}
			else
			{
				//ExposedReferenceTable on 2017 - early 2019
				Console.WriteLine($"String subnode has typename: {subnode.TypeName}");
				//throw new NotSupportedException($"String subnode has typename: {subnode.TypeName}");
			}
		}

		private static bool IsOffsetPtr(this UnityNode node, [NotNullWhen(true)] out string? elementType)
		{
			List<UnityNode> subnodes = node.SubNodes;
			if (node.TypeName == OffsetPtrName && subnodes.Count == 1 && subnodes[0].Name == "data")
			{
				elementType = subnodes[0].TypeName!;
				return true;
			}

			elementType = null;
			return false;
		}

		private static bool IsKeyframe(this UnityNode node, [NotNullWhen(true)] out string? elementType)
		{
			List<UnityNode> subnodes = node.SubNodes;
			if (node.TypeName == KeyframeName && subnodes.Any(n => n.Name == "value"))
			{
				elementType = subnodes.Single(n => n.Name == "value").TypeName!;
				return true;
			}

			elementType = null;
			return false;
		}

		private static bool IsAnimationCurve(this UnityNode node, [NotNullWhen(true)] out string? elementType)
		{
			elementType = null;

			if(node.TypeName != AnimationCurveName)
				return false;

			UnityNode? curveNode = node.SubNodes.SingleOrDefault(subnode => subnode.Name == "m_Curve");
			if (curveNode == null || curveNode.TypeName != "vector")
				return false;
			UnityNode keyframeNode = curveNode.SubNodes![0].SubNodes![1];
			
			if(!keyframeNode.TypeName!.StartsWith($"{KeyframeName}_"))
				return false;
			
			elementType = keyframeNode.TypeName.Substring(KeyframeName.Length + 1);
			return true;
		}

		private static bool IsColorRGBA(this UnityNode node, [NotNullWhen(true)] out string? newName)
		{
			newName = null;

			if (node.TypeName != ColorRGBAName)
				return false;

			List<UnityNode> subnodes = node.SubNodes;
			
			if (subnodes.Count == 4 && subnodes.All(n => n.TypeName == "float"))
			{
				newName = $"{ColorRGBAName}f";
				return true;
			}

			if (subnodes.Count == 1 && subnodes[0].Name == "rgba")
			{
				newName = $"{ColorRGBAName}32";
				return true;
			}

			return false;
		}

		private static bool IsPackedBitVector(this UnityNode node, [NotNullWhen(true)] out string? newName)
		{
			newName = null;

			if (node.TypeName != PackedBitVectorName)
				return false;

			List<UnityNode> subnodes = node.SubNodes;

			//The packed bit vectors are constant throughout all the unity versions and identifiable by their number of fields
			if (subnodes.Count == 5)
			{
				newName = $"{PackedBitVectorName}_float";
				return true;
			}
			if (subnodes.Count == 3)
			{
				newName = $"{PackedBitVectorName}_int";
				return true;
			}
			if (subnodes.Count == 2)
			{
				newName = $"{PackedBitVectorName}_Quaternionf";
				return true;
			}

			return false;
		}

		private static bool IsEditorScene(this UnityNode node)
		{
			List<UnityNode> subnodes = node.SubNodes;
			if (node.TypeName == "Scene" && subnodes.Any(n => n.Name == "enabled") && subnodes.Any(n => n.Name == "path"))
			{
				return true;
			}
			return false;
		}

		private static bool IsVFXEntryExposed(this UnityNode node, [NotNullWhen(true)] out string? elementType)
		{
			List<UnityNode> subnodes = node.SubNodes;
			if (node.TypeName == VFXEntryExposedName && subnodes.Any(n => n.Name == "m_Value"))
			{
				elementType = subnodes.Single(n => n.Name == "m_Value").TypeName!.ReplaceBadCharacters();
				return true;
			}

			elementType = null;
			return false;
		}

		private static bool IsVFXEntryExpressionValue(this UnityNode node, [NotNullWhen(true)] out string? elementType)
		{
			List<UnityNode> subnodes = node.SubNodes;
			if (node.TypeName == VFXEntryExpressionValueName && subnodes.Any(n => n.Name == "m_Value"))
			{
				elementType = subnodes.Single(n => n.Name == "m_Value").TypeName!.ReplaceBadCharacters();
				return true;
			}

			elementType = null;
			return false;
		}

		private static bool IsVFXField(this UnityNode node, [NotNullWhen(true)] out string? elementType)
		{
			elementType = null;

			if (node.TypeName != VFXFieldName)
				return false;

			List<UnityNode> subnodes = node.SubNodes;

			UnityNode? arrayNode = subnodes.SingleOrDefault(subnode => subnode.Name == "m_Array");
			if (arrayNode == null || arrayNode.TypeName != "vector")
				return false;
			UnityNode elementNode = arrayNode.SubNodes![0].SubNodes![1];

			elementType = elementNode.TypeName!;
			return true;
		}

		private static bool IsVFXPropertySheetSerializedBase(this UnityNode node, [NotNullWhen(true)] out string? elementType)
		{
			elementType = null;
			List<UnityNode> subnodes = node.SubNodes;
			if (node.TypeName == VFXPropertySheetSerializedBaseName && subnodes.Any(n => n.Name == "m_Float"))
			{
				string floatFieldType = subnodes.Single(n => n.Name == "m_Float").TypeName!;
				if (floatFieldType.StartsWith($"{VFXFieldName}_{VFXEntryExposedName}"))
				{
					elementType = VFXEntryExposedName;
					return true;
				}
				else if (floatFieldType.StartsWith($"{VFXFieldName}_{VFXEntryExpressionValueName}"))
				{
					elementType = VFXEntryExpressionValueName;
					return true;
				}
			}

			return false;
		}

		private static bool IsTilemapRefCountedData(this UnityNode node, [NotNullWhen(true)] out string? elementType)
		{
			List<UnityNode> subnodes = node.SubNodes;
			if (node.TypeName == TilemapRefCountedDataName && subnodes.Any(n => n.Name == "m_Data"))
			{
				elementType = subnodes.Single(n => n.Name == "m_Data").TypeName!.ReplaceBadCharacters();
				return true;
			}

			elementType = null;
			return false;
		}

		/// <summary>
		/// Some Unity 4 versions have this issue where Blend1d and Blend2d were initially both called Blend2d
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private static bool IsBlend1dAs2d(this UnityNode node)
		{
			List<UnityNode> subnodes = node.SubNodes;
			if (node.TypeName == Blend2dConstantName && subnodes.Count == 1 && subnodes[0].Name == "m_ChildThresholdArray")
			{
				return true;
			}
			return false;
		}
	}
}
