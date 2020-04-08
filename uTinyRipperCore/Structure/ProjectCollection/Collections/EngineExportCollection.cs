using System;
using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.Classes;

using Object = uTinyRipper.Classes.Object;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Converters.Project
{
	public class EngineExportCollection : IExportCollection
	{
		public EngineExportCollection(Object asset, Version version)
		{
			if (asset == null)
			{
				throw new ArgumentNullException(nameof(asset));
			}

			File = asset.File;
			m_version = version;
			if (IsEngineFile(asset.File.Name))
			{
				foreach (Object builtInAsset in File.FetchAssets())
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

		public static bool IsEngineAsset(Object asset, Version version)
		{
			if (!GetEngineBuildInAsset(asset, version, out EngineBuiltInAsset builtinAsset))
			{
				return false;
			}
			if (IsEngineFile(asset.File.Name))
			{
				return true;
			}

			switch (asset.ClassID)
			{
				case ClassIDType.Material:
					{
						Material material = (Material)asset;
						if (material.Name == EngineBuiltInAssets.FontMaterialName)
						{
							return false;
						}
						Shader shader = material.Shader.FindAsset(material.File);
						if (shader == null)
						{
							return true;
						}
						return IsEngineAsset(shader, version);
					}

				case ClassIDType.Texture2D:
					{
						Texture2D texture = (Texture2D)asset;
						return builtinAsset.Parameter == texture.CompleteImageSize;
					}

				case ClassIDType.Shader:
					return true;

				case ClassIDType.Sprite:
					{
						Sprite sprite = (Sprite)asset;
						Texture2D texture = sprite.RD.Texture.FindAsset(sprite.File);
						if (texture == null)
						{
							return false;
						}
						return IsEngineAsset(texture, version);
					}

				default:
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

		private static bool GetEngineBuildInAsset(Object asset, Version version, out EngineBuiltInAsset engineAsset)
		{
			switch (asset.ClassID)
			{
				case ClassIDType.Material:
					{
						Material material = (Material)asset;
						if (EngineBuiltInAssets.TryGetMaterial(material.Name, version, out engineAsset))
						{
							return true;
						}
					}
					break;

				case ClassIDType.Texture2D:
					{
						Texture2D texture = (Texture2D)asset;
						if (EngineBuiltInAssets.TryGetTexture(texture.Name, version, out engineAsset))
						{
							return true;
						}
					}
					break;

				case ClassIDType.Mesh:
					{
						Mesh mesh = (Mesh)asset;
						if (EngineBuiltInAssets.TryGetMesh(mesh.Name, version, out engineAsset))
						{
							return true;
						}
					}
					break;

				case ClassIDType.Shader:
					{
						Shader shader = (Shader)asset;
						if (EngineBuiltInAssets.TryGetShader(shader.ValidName, version, out engineAsset))
						{
							return true;
						}
					}
					break;

				case ClassIDType.Font:
					{
						Font font = (Font)asset;
						if (EngineBuiltInAssets.TryGetFont(font.Name, version, out engineAsset))
						{
							return true;
						}
					}
					break;

				case ClassIDType.Sprite:
					{
						Sprite sprite = (Sprite)asset;
						if (EngineBuiltInAssets.TryGetSprite(sprite.Name, version, out engineAsset))
						{
							return true;
						}
					}
					break;

				case ClassIDType.LightmapParameters:
					{
						LightmapParameters lightParams = (LightmapParameters)asset;
						if (EngineBuiltInAssets.TryGetLightmapParams(lightParams.Name, version, out engineAsset))
						{
							return true;
						}
					}
					break;

				case ClassIDType.MonoBehaviour:
					{
						MonoBehaviour behaviour = (MonoBehaviour)asset;
						if (behaviour.Name != string.Empty)
						{
							if (EngineBuiltInAssets.TryGetBehaviour(behaviour.Name, version, out engineAsset))
							{
								return true;
							}
						}
					}
					break;
			}
			engineAsset = default;
			return false;
		}

		public bool Export(ProjectAssetContainer container, string dirPath)
		{
			return false;
		}

		public bool IsContains(Object asset)
		{
			return m_assets.Contains(asset);
		}

		public long GetExportID(Object asset)
		{
			GetEngineBuildInAsset(asset, m_version, out EngineBuiltInAsset engneAsset);
			return engneAsset.ExportID;
		}

		public MetaPtr CreateExportPointer(Object asset, bool isLocal)
		{
			if (isLocal)
			{
				throw new NotSupportedException();
			}
			GetEngineBuildInAsset(asset, m_version, out EngineBuiltInAsset engneAsset);
			if (!engneAsset.IsValid)
			{
				throw new NotImplementedException($"Unknown ExportID for asset {asset.PathID} from file {asset.File.Name}");
			}
			long exportID = engneAsset.ExportID;
			UnityGUID guid = engneAsset.GUID;
			return new MetaPtr(exportID, guid, AssetType.Internal);
		}

		public ISerializedFile File { get; }
		public TransferInstructionFlags Flags => File.Flags;
		public IEnumerable<Object> Assets => m_assets;
		public string Name => "Engine 2017.3.0f3";

		private readonly HashSet<Object> m_assets = new HashSet<Object>();

		private readonly Version m_version;
	}
}
