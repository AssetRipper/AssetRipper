using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Project.Exporters.Engine;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_1113;
using AssetRipper.SourceGenerated.Classes.ClassID_128;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using System.Collections.Generic;


namespace AssetRipper.Core.Project.Collections
{
	public class EngineExportCollection : IExportCollection
	{
		public EngineExportCollection(IUnityObjectBase asset, UnityVersion version)
		{
			if (asset == null)
			{
				throw new ArgumentNullException(nameof(asset));
			}

			File = asset.Collection;
			m_version = version;
			if (IsEngineFile(asset.Collection.Name))
			{
				foreach (IUnityObjectBase builtInAsset in File)
				{
					if (IsEngineAsset(builtInAsset, version))
					{
						m_assets.Add(builtInAsset);
					}
				}
			}
			else
			{
				m_assets.Add(asset);
			}
		}

		public static bool IsEngineAsset(IUnityObjectBase asset, UnityVersion version)
		{
			if (!GetEngineBuildInAsset(asset, version, out EngineBuiltInAsset builtinAsset))
			{
				return false;
			}
			if (IsEngineFile(asset?.Collection.Name))
			{
				return true;
			}

			if (asset is IMaterial material)
			{
				if (material.NameString == EngineBuiltInAssets.FontMaterialName)
				{
					return false;
				}
				IShader? shader = material.Shader_C21.TryGetAsset(material.Collection);
				if (shader == null)
				{
					return true;
				}
				return IsEngineAsset(shader, version);
			}
			else if (asset is IShader)
			{
				return true;
			}
			else if (asset is ITexture2D texture)
			{
				return builtinAsset.Parameter == texture.GetCompleteImageSize();
			}
			else if (asset is ISprite sprite)
			{
				ITexture2D? spriteTexture = sprite.RD_C213.Texture.TryGetAsset(sprite.Collection);
				if (spriteTexture == null)
				{
					return false;
				}
				return IsEngineAsset(spriteTexture, version);
			}
			else
			{
				return false;
			}
		}

		private static bool IsEngineFile(string? fileName)
		{
			return fileName is not null
				&& (FilenameUtils.IsDefaultResource(fileName) || FilenameUtils.IsBuiltinExtra(fileName) || FilenameUtils.IsEngineGeneratedF(fileName));
		}

		private static bool GetEngineBuildInAsset(IUnityObjectBase asset, UnityVersion version, out EngineBuiltInAsset engineAsset)
		{
			if (asset is IMaterial material)
			{
				if (EngineBuiltInAssets.TryGetMaterial(material.NameString, version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is ITexture2D texture)
			{
				if (EngineBuiltInAssets.TryGetTexture(texture.NameString, version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is IMesh mesh)
			{
				if (EngineBuiltInAssets.TryGetMesh(mesh.NameString, version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is IShader shader)
			{
				if (EngineBuiltInAssets.TryGetShader(shader.TryGetName() ?? "", version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is IFont font)
			{
				if (EngineBuiltInAssets.TryGetFont(font.NameString, version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is ISprite sprite)
			{
				if (EngineBuiltInAssets.TryGetSprite(sprite.NameString, version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is ILightmapParameters lightParams)
			{
				if (EngineBuiltInAssets.TryGetLightmapParams(lightParams.NameString, version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is SourceGenerated.Classes.ClassID_114.IMonoBehaviour behaviour)
			{
				if (behaviour.NameString != string.Empty)
				{
					if (EngineBuiltInAssets.TryGetBehaviour(behaviour.NameString, version, out engineAsset))
					{
						return true;
					}
				}
			}

			engineAsset = default;
			return false;
		}

		public bool Export(IProjectAssetContainer container, string projectDirectory)
		{
			return true; //successfully redirected to an engine asset
		}

		public bool IsContains(IUnityObjectBase asset)
		{
			return m_assets.Contains(asset);
		}

		public long GetExportID(IUnityObjectBase asset)
		{
			GetEngineBuildInAsset(asset, m_version, out EngineBuiltInAsset engneAsset);
			return engneAsset.ExportID;
		}

		public MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			if (isLocal)
			{
				throw new NotSupportedException();
			}
			GetEngineBuildInAsset(asset, m_version, out EngineBuiltInAsset engineAsset);
			if (!engineAsset.IsValid)
			{
				throw new NotImplementedException($"Unknown ExportID for asset {asset.PathID} from file {asset.Collection.Name}");
			}
			long exportID = engineAsset.ExportID;
			UnityGUID guid = engineAsset.GUID;
			return new MetaPtr(exportID, guid, AssetType.Internal);
		}

		public AssetCollection File { get; }
		public TransferInstructionFlags Flags => File.Flags;
		public IEnumerable<IUnityObjectBase> Assets => m_assets;
		public string Name => $"Engine {m_version}";

		private readonly HashSet<IUnityObjectBase> m_assets = new();

		private readonly UnityVersion m_version;
	}
}
