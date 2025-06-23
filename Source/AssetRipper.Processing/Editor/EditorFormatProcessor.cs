using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.AssetCreation;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Processing.AnimationClips;
using AssetRipper.Processing.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_142;
using AssetRipper.SourceGenerated.Classes.ClassID_147;
using AssetRipper.SourceGenerated.Classes.ClassID_157;
using AssetRipper.SourceGenerated.Classes.ClassID_19;
using AssetRipper.SourceGenerated.Classes.ClassID_196;
using AssetRipper.SourceGenerated.Classes.ClassID_218;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_30;
using AssetRipper.SourceGenerated.Classes.ClassID_320;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_47;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_78;
using AssetRipper.SourceGenerated.Classes.ClassID_850595691;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using System.Diagnostics;

namespace AssetRipper.Processing.Editor;

/// <summary>
/// <para>
/// This processor primarily handles "editor-only" fields.
/// These fields exist in the Unity Editor, but not in compiled game files.
/// Without this processing, those fields would have C# default values of zero.
/// </para>
/// <para>
/// For most fields, this is just setting the field to the Unity default.
/// However for some fields, there can be a calculation to recover an appropriate
/// value for the field. For example, <see cref="ITransform.LocalEulerAnglesHint_C4"/>
/// is set using <see cref="ITransform.LocalRotation_C4"/> with a Quaternion to
/// Euler angle conversion. Similarly, <see cref="ITransform.RootOrder_C4"/> is
/// calculated from <see cref="ITransform.Father_C4P"/> and <see cref="ITransform.Children_C4P"/>.
/// </para>
/// <para>
/// Compiled game files can be identified from binary editor files by the 
/// <see cref="TransferInstructionFlags.SerializeGameRelease"/> flag.
/// However, those binary editor files are not commonly ripped with AssetRipper.
/// More often, generated <see cref="ProcessedAssetCollection"/>s are given editor flags
/// so as to exclude them from unnecessary processing. This is the default for
/// <see cref="GameBundle.AddNewProcessedCollection(string, UnityVersion)"/>.
/// </para>
/// </summary>
public class EditorFormatProcessor : IAssetProcessor
{
	private ITagManager? tagManager;
	private readonly BundledAssetsExportMode bundledAssetsExportMode;
	private IAssemblyManager? assemblyManager;
	private PathChecksumCache? checksumCache;

	public EditorFormatProcessor(BundledAssetsExportMode bundledAssetsExportMode)
	{
		this.bundledAssetsExportMode = bundledAssetsExportMode;
	}

	public void Process(GameData gameData)
	{
		Logger.Info(LogCategory.Processing, "Editor Format Conversion");
		tagManager = gameData.GameBundle.FetchAssets().OfType<ITagManager>().FirstOrDefault();
		assemblyManager = gameData.AssemblyManager;
		checksumCache = new PathChecksumCache(gameData);

		//Sequential processing
		foreach (IUnityObjectBase asset in GetReleaseAssets(gameData))
		{
			Convert(asset);
		}

		//Parallel processing
		Parallel.ForEach(GetReleaseAssets(gameData), ConvertAsync);

		checksumCache = null;
		assemblyManager = null;
		tagManager = null;
	}

	private static IEnumerable<IUnityObjectBase> GetReleaseAssets(GameData gameData)
	{
		return GetReleaseCollections(gameData).SelectMany(c => c);
	}

	private static IEnumerable<AssetCollection> GetReleaseCollections(GameData gameData)
	{
		return gameData.GameBundle.FetchAssetCollections().Where(c => c.Flags.IsRelease());
	}

	private void Convert(IUnityObjectBase asset)
	{
		switch (asset)
		{
			//ordered by approximate frequency
			case IGameObject gameObject:
				gameObject.ConvertToEditorFormat(tagManager);
				break;
			case IRenderer renderer:
				EditorFormatConverter.Convert(renderer);
				break;
			case ISpriteAtlas spriteAtlas:
				spriteAtlas.ConvertToEditorFormat();
				break;
			case IAnimationClip animationClip:
				AnimationClipConverter.Process(animationClip, checksumCache!.Value);
				break;
			case IAssetBundle assetBundle:
				OriginalPathHelper.SetOriginalPaths(assetBundle, bundledAssetsExportMode);
				break;
			case INavMeshSettings navMeshSettings:
				navMeshSettings.ConvertToEditorFormat();
				break;
			case IResourceManager resourceManager:
				OriginalPathHelper.SetOriginalPaths(resourceManager);
				break;
			case TypeTreeObject { IsPlayerSettings: true } playerSettings:
				SerializableStructure editorStructure = playerSettings.EditorFields;
				if (editorStructure.ContainsField("webGLLinkerTarget"))
				{
					editorStructure["webGLLinkerTarget"].AsInt32 = 1;
				}
				if (editorStructure.ContainsField("allowUnsafeCode"))
				{
					editorStructure["allowUnsafeCode"].AsBoolean = true;
				}
				ApiCompatibilityLevel compatibilityLevel;
				ScriptingRuntimeVersion runtimeVersion;
				Debug.Assert(assemblyManager is not null);
				if (assemblyManager.HasMscorlib2)
				{
					compatibilityLevel = ApiCompatibilityLevel.NET_2_0;
					runtimeVersion = ScriptingRuntimeVersion.Legacy;
				}
				else
				{
					compatibilityLevel = ApiCompatibilityLevel.NET_Unity_4_8;
					runtimeVersion = ScriptingRuntimeVersion.Latest;
				}
				if (editorStructure.ContainsField("apiCompatibilityLevel"))
				{
					editorStructure["apiCompatibilityLevel"].AsInt32 = (int)compatibilityLevel;
				}
				if (editorStructure.ContainsField("scriptingRuntimeVersion"))
				{
					editorStructure["scriptingRuntimeVersion"].AsInt32 = (int)runtimeVersion;
				}
				break;
		}
	}

	private static void ConvertAsync(IUnityObjectBase asset)
	{
		switch (asset)
		{
			//ordered by approximate frequency
			case ITransform transform:
				EditorFormatConverterAsync.Convert(transform);
				break;
			case IMesh mesh:
				mesh.SetMeshOptimizationFlags(MeshOptimizationFlags.Everything);
				break;
			case ITerrain terrain:
				terrain.ScaleInLightmap = 0.0512f;
				break;
			case IPlayableDirector playableDirector:
				EditorFormatConverterAsync.Convert(playableDirector);
				break;
			case IGraphicsSettings graphicsSettings:
				graphicsSettings.ConvertToEditorFormat();
				break;
			case IQualitySettings qualitySettings:
				qualitySettings.ConvertToEditorFormat();
				break;
			case IPhysics2DSettings physics2DSettings:
				physics2DSettings.ConvertToEditorFormat();
				break;
			case ILightmapSettings lightmapSettings:
				lightmapSettings.ConvertToEditorFormat();
				break;
			case ILightingSettings lightingSettings:
				lightingSettings.ConvertToEditorFormat();
				break;
		}
	}
}
