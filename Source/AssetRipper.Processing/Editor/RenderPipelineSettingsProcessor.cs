using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AssetRipper.Assets.Generics;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_30;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Object;

namespace AssetRipper.Processing.Editor;

/// <summary>
/// Restores the editor registration of the player's active render pipeline settings.
/// </summary>
public sealed class RenderPipelineSettingsProcessor : IAssetProcessor
{
	public void Process(GameData gameData)
	{
		foreach (IGraphicsSettings settings in gameData.GameBundle.FetchAssets().OfType<IGraphicsSettings>())
		{
			if (!settings.Collection.Flags.IsRelease() || settings.RenderPipelineGlobalSettingsMap is not { } map ||
				settings.CurrentRenderPipelineGlobalSettingsP is not IMonoBehaviour globalSettings ||
				globalSettings.ScriptP is not { } script || !script.IsScriptPresents(gameData.AssemblyManager))
			{
				continue;
			}

			string? pipelineName = GetPipelineTypeName(script.GetTypeDefinition(gameData.AssemblyManager), gameData.AssemblyManager.RuntimeContext);
			if (pipelineName is not null && !map.ContainsKey(pipelineName))
			{
				// The player stores only the active settings pointer. The editor looks it
				// up by pipeline type in this map instead.
				AssetPair<Utf8String, PPtr_Object_5> pair = map.AddNew();
				pair.Key = pipelineName;
				pair.Value.SetAsset(settings.Collection, globalSettings);
			}
		}
	}

	private static string? GetPipelineTypeName(TypeDefinition type, RuntimeContext? context)
	{
		HashSet<TypeDefinition> visited = [];
		while (visited.Add(type) && type.BaseType is ITypeDefOrRef baseType)
		{
			if (baseType is TypeSpecification { Signature: GenericInstanceTypeSignature signature } &&
				signature.GenericType.FullName == "UnityEngine.Rendering.RenderPipelineGlobalSettings`2" &&
				signature.TypeArguments.Count == 2 && signature.TypeArguments[1] is TypeDefOrRefSignature pipelineType)
			{
				return pipelineType.FullName;
			}
			if (context is null)
			{
				break;
			}
			baseType.Resolve(context, out TypeDefinition? definition);
			if (definition is null)
			{
				break;
			}
			type = definition;
		}
		return null;
	}
}
