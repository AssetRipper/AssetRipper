using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Font;
using AssetRipper.Core.Classes.Material;
using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Sprite;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Utils;
using AssetRipper.Core.Project.Exporters.Engine;
using System;
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

			File = asset.SerializedFile;
			m_version = version;
			if (IsEngineFile(asset.SerializedFile.Name))
			{
				foreach (IUnityObjectBase builtInAsset in File.FetchAssets())
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
			if (IsEngineFile(asset?.SerializedFile.Name))
			{
				return true;
			}

			if (asset is IMaterial material)
			{
				if (material.Name == EngineBuiltInAssets.FontMaterialName)
				{
					return false;
				}
				IShader shader = material.ShaderPtr.FindAsset(material.SerializedFile);
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
				return builtinAsset.Parameter == texture.CompleteImageSize;
			}
			else if (asset is ISprite sprite)
			{
				ITexture2D spriteTexture = sprite.TexturePtr.FindAsset(sprite.SerializedFile);
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

		private static bool IsEngineFile(string fileName)
		{
			if (FilenameUtils.IsDefaultResource(fileName))
			{
				return true;
			}
			if (FilenameUtils.IsBuiltinExtra(fileName))
			{
				return true;
			}
			if (FilenameUtils.IsEngineGeneratedF(fileName))
			{
				return true;
			}
			return false;
		}

		private static bool GetEngineBuildInAsset(IUnityObjectBase asset, UnityVersion version, out EngineBuiltInAsset engineAsset)
		{
			if (asset is IMaterial material)
			{
				if (EngineBuiltInAssets.TryGetMaterial(material.Name, version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is ITexture2D texture)
			{
				if (EngineBuiltInAssets.TryGetTexture(texture.Name, version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is IMesh mesh)
			{
				if (EngineBuiltInAssets.TryGetMesh(mesh.Name, version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is IShader shader)
			{
				if (EngineBuiltInAssets.TryGetShader(shader.GetValidShaderName(), version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is IFont font)
			{
				if (EngineBuiltInAssets.TryGetFont(font.Name, version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is ISprite sprite)
			{
				if (EngineBuiltInAssets.TryGetSprite(sprite.Name, version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is ILightmapParameters lightParams)
			{
				if (EngineBuiltInAssets.TryGetLightmapParams(lightParams.Name, version, out engineAsset))
				{
					return true;
				}
			}
			else if (asset is IMonoBehaviour behaviour)
			{
				if (behaviour.Name != string.Empty)
				{
					if (EngineBuiltInAssets.TryGetBehaviour(behaviour.Name, version, out engineAsset))
					{
						return true;
					}
				}
			}

			engineAsset = default;
			return false;
		}

		public bool Export(ProjectAssetContainer container, string dirPath)
		{
			return false;
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
			GetEngineBuildInAsset(asset, m_version, out EngineBuiltInAsset engneAsset);
			if (!engneAsset.IsValid)
			{
				throw new NotImplementedException($"Unknown ExportID for asset {asset.PathID} from file {asset.SerializedFile.Name}");
			}
			long exportID = engneAsset.ExportID;
			UnityGUID guid = engneAsset.GUID;
			return new MetaPtr(exportID, guid, AssetType.Internal);
		}

		public ISerializedFile File { get; }
		public TransferInstructionFlags Flags => File.Flags;
		public IEnumerable<IUnityObjectBase> Assets => m_assets;
		public string Name => "Engine 2017.3.0f3";

		private readonly HashSet<IUnityObjectBase> m_assets = new HashSet<IUnityObjectBase>();

		private readonly UnityVersion m_version;
	}
}
