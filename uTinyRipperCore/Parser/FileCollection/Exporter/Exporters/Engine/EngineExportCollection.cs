using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.AssetExporters
{
	public class EngineExportCollection : IExportCollection
	{
		public EngineExportCollection(Object asset)
		{
			if (asset == null)
			{
				throw new ArgumentNullException(nameof(asset));
			}

			File = asset.File;
			if (IsEngineFile(asset.File.Name))
			{
				foreach (Object builtInAsset in File.FetchAssets())
				{
					if (IsEngineAsset(builtInAsset))
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
				
		public static bool IsEngineAsset(Object asset)
		{
			if (!GetEngineBuildInAsset(asset, out EngineBuiltInAsset _))
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
						return IsEngineAsset(shader);
					}

				case ClassIDType.Shader:
					return true;

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

		private static bool GetEngineBuildInAsset(Object asset, out EngineBuiltInAsset engineAsset)
		{
			switch (asset.ClassID)
			{
				case ClassIDType.Material:
					{
						Material material = (Material)asset;
						if(EngineBuiltInAssets.Materials.TryGetValue(material.Name, out engineAsset))
						{
							return true;
						}
					}
					break;

				case ClassIDType.Texture2D:
					{
						Texture2D texture = (Texture2D)asset;
						if (EngineBuiltInAssets.Textures.TryGetValue(texture.Name, out engineAsset))
						{
							return true;
						}
					}
					break;

				case ClassIDType.Mesh:
					{
						Mesh mesh = (Mesh)asset;
						if (EngineBuiltInAssets.Meshes.TryGetValue(mesh.Name, out engineAsset))
						{
							return true;
						}
					}
					break;

				case ClassIDType.Shader:
					{
						Shader shader = (Shader)asset;
						if (EngineBuiltInAssets.Shaders.TryGetValue(shader.ValidName, out engineAsset))
						{
							return true;
						}
					}
					break;

				case ClassIDType.Font:
					{
						Font font = (Font)asset;
						if (EngineBuiltInAssets.Fonts.TryGetValue(font.Name, out engineAsset))
						{
							return true;
						}
					}
					break;

				case ClassIDType.Sprite:
					{
						Sprite sprite = (Sprite)asset;
						if (EngineBuiltInAssets.Sprites.TryGetValue(sprite.Name, out engineAsset))
						{
							return true;
						}
					}
					break;

				case ClassIDType.LightmapParameters:
					{
						LightmapParameters lightParams = (LightmapParameters)asset;
						if (EngineBuiltInAssets.LightmapParams.TryGetValue(lightParams.Name, out engineAsset))
						{
							return true;
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
			GetEngineBuildInAsset(asset, out EngineBuiltInAsset engneAsset);
			if(!engneAsset.IsValid)
			{
				throw new NotImplementedException($"Unknown ExportID for asset {asset.ToLogString()} from file {asset.File.Name}");
			}
			return engneAsset.ExportID;
		}

		public ExportPointer CreateExportPointer(Object asset, bool isLocal)
		{
			if(isLocal)
			{
				throw new NotSupportedException();
			}
			GetEngineBuildInAsset(asset, out EngineBuiltInAsset engneAsset);
			if (!engneAsset.IsValid)
			{
				throw new NotImplementedException($"Unknown ExportID for asset {asset.ToLogString()} from file {asset.File.Name}");
			}
			long exportID = engneAsset.ExportID;
			EngineGUID guid = engneAsset.GUID;
			return new ExportPointer(exportID, guid, AssetType.Internal);
		}

		public ISerializedFile File { get; }
		public TransferInstructionFlags Flags => File.Flags;
		public IEnumerable<Object> Assets => m_assets;
		public string Name => "Engine 2017.3.0f3";

		private readonly HashSet<Object> m_assets = new HashSet<Object>();
	}
}
