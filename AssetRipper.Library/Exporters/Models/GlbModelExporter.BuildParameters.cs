using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Library.Exporters.Meshes;
using AssetRipper.Library.Exporters.Textures;
using AssetRipper.Library.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Subclasses.UnityTexEnv;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace AssetRipper.Library.Exporters.Models
{
	public partial class GlbModelExporter
	{
		private readonly record struct BuildParameters(
			MaterialBuilder DefaultMaterial,
			Dictionary<ITexture2D, MemoryImage> ImageCache,
			Dictionary<IMaterial, MaterialBuilder> MaterialCache,
			Dictionary<IMesh, MeshData> MeshCache,
			bool IsScene)
		{
			public BuildParameters(bool isScene) : this(new MaterialBuilder("DefaultMaterial"), new(), new(), new(), isScene) { }
			public bool TryGetOrMakeMeshData(IMesh mesh, out MeshData meshData)
			{
				if (MeshCache.TryGetValue(mesh, out meshData))
				{
					return true;
				}
				else if (MeshData.TryMakeFromMesh(mesh, out meshData))
				{
					MeshCache.Add(mesh, meshData);
					return true;
				}
				return false;
			}
			
			public MaterialBuilder GetOrMakeMaterial(IMaterial? material)
			{
				if (material is null)
				{
					return DefaultMaterial;
				}
				if (!MaterialCache.TryGetValue(material, out MaterialBuilder? materialBuilder))
				{
					materialBuilder = MakeMaterialBuilder(material);
					MaterialCache.Add(material, materialBuilder);
				}
				return materialBuilder;
			}

			public bool TryGetOrMakeImage(ITexture2D texture, out MemoryImage image)
			{
				if (!ImageCache.TryGetValue(texture, out image))
				{
					if (TryConvertToBitmap(texture, out DirectBitmap? bitmap))
					{
						using MemoryStream memoryStream = new();
						bitmap.SaveAsPng(memoryStream);
						image = new MemoryImage(memoryStream.ToArray());
						ImageCache.Add(texture, image);
						return true;
					}
					return false;
				}
				else
				{
					return true;
				}
			}

			private MaterialBuilder MakeMaterialBuilder(IMaterial material)
			{
				MaterialBuilder materialBuilder = new MaterialBuilder(material.NameString);
				GetTextures(material, out ITexture2D? mainTexture, out ITexture2D? normalTexture);
				if (mainTexture is not null && TryGetOrMakeImage(mainTexture, out MemoryImage mainImage))
				{
					materialBuilder.WithBaseColor(mainImage);
				}
				if (normalTexture is not null && TryGetOrMakeImage(normalTexture, out MemoryImage normalImage))
				{
					materialBuilder.WithNormal(normalImage);
				}
				return materialBuilder;
			}

			private static void GetTextures(IMaterial material, out ITexture2D? mainTexture, out ITexture2D? normalTexture)
			{
				mainTexture = null;
				normalTexture = null;
				ITexture2D? mainReplacement = null;
				foreach ((Utf8String utf8Name, IUnityTexEnv textureParameter) in material.GetTextureProperties())
				{
					string name = utf8Name.String;
					if (IsMainTexture(name))
					{
						mainTexture ??= textureParameter.Texture.TryGetAsset(material.SerializedFile) as ITexture2D;
					}
					else if (IsNormalTexture(name))
					{
						normalTexture ??= textureParameter.Texture.TryGetAsset(material.SerializedFile) as ITexture2D;
					}
					else
					{
						mainReplacement ??= textureParameter.Texture.TryGetAsset(material.SerializedFile) as ITexture2D;
					}
				}
				mainTexture ??= mainReplacement;
			}

			private static bool TryConvertToBitmap(ITexture2D texture, [NotNullWhen(true)] out DirectBitmap? bitmap)
			{
				bitmap = TextureAssetExporter.ConvertToBitmap(texture);
				return bitmap is not null;
			}

			private static bool IsMainTexture(string textureName)
			{
				return textureName is "_MainTex" or "texture" or "Texture" or "_Texture";
			}

			private static bool IsNormalTexture(string textureName)
			{
				return textureName is "_Normal" or "Normal" or "normal";
			}
		}
	}
}
