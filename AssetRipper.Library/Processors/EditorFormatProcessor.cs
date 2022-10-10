using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Core.Linq;
using AssetRipper.Core.Logging;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_157;
using AssetRipper.SourceGenerated.Classes.ClassID_19;
using AssetRipper.SourceGenerated.Classes.ClassID_196;
using AssetRipper.SourceGenerated.Classes.ClassID_218;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_30;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_47;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Classes.ClassID_78;
using System.Linq;

namespace AssetRipper.Library.Processors
{
	public class EditorFormatProcessor : IAssetProcessor
	{
		private ITagManager? tagManager;
		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			Logger.Info(LogCategory.Processing, "Editor Format Conversion");
			tagManager = gameBundle.FetchAssets().SelectType<IUnityObjectBase, ITagManager>().FirstOrDefault();
			foreach (AssetCollection collection in gameBundle.FetchAssetCollections().Where(c => c.Flags.IsRelease()))
			{
				foreach (IUnityObjectBase asset in collection)
				{
					Convert(asset);
				}
			}
			tagManager = null;
		}

		private void Convert(IUnityObjectBase asset)
		{
			switch (asset)
			{
				//ordered by approximate frequency
				case IGameObject gameObject:
					gameObject.ConvertToEditorFormat(tagManager);
					break;
				case ITransform transform:
					transform.ConvertToEditorFormat();
					break;
				case IRenderer renderer:
					renderer.ConvertToEditorFormat();
					break;
				case IMesh mesh:
					mesh.ConvertToEditorFormat();
					break;
				case ISpriteAtlas spriteAtlas:
					spriteAtlas.ConvertToEditorFormat();
					break;
				case ITerrain terrain:
					terrain.ConvertToEditorFormat();
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
				case INavMeshSettings navMeshSettings:
					navMeshSettings.ConvertToEditorFormat();
					break;
			}
		}
	}
}
