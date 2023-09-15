using AssetRipper.Assets;
using AssetRipper.GUI.Localizations;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_33;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_82;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.GUI.Electron.Pages.Assets;

internal static class InformationTabContentExtensions
{
	public static IEnumerable<(string, IUnityObjectBase)> GetCustomReferenceProperties(this IUnityObjectBase Asset)
	{
		List<(string, IUnityObjectBase)> list = new();
		if (Asset.MainAsset is not null && Asset.MainAsset != Asset)
		{
			list.Add((Localization.MainAsset, Asset.MainAsset));
		}
		switch (Asset)
		{
			case IComponent component:
				{
					if (component.GameObject_C2P is { } gameObject)
					{
						list.Add((Localization.GameObject, gameObject));
						if (component is IRenderer && gameObject.TryGetComponent<IMeshFilter>()?.Mesh_C33P is { } rendererMesh)
						{
							list.Add((Localization.Mesh, rendererMesh));
						}
					}
					if (component is IMonoBehaviour monoBehaviour && monoBehaviour.Script_C114P is { } monoScript)
					{
						list.Add((Localization.Script, monoScript));
					}
					else if (component is IMeshFilter meshFilter && meshFilter.Mesh_C33P is { } mesh)
					{
						list.Add((Localization.Mesh, mesh));
					}
					else if (component is IAudioSource audioSource && audioSource.AudioClip_C82P is { } audioSourceClip)
					{
						list.Add((Localization.AudioClip, audioSourceClip));
					}
				}
				break;
			case IGameObject gameObject:
				{
					if (gameObject.TryGetComponent(out ITransform? transform))
					{
						list.Add((Localization.Transform, transform));
					}
				}
				break;
			case IMaterial material:
				{
					if (material.Shader_C21P is { } shader)
					{
						list.Add((Localization.Shader, shader));
					}
				}
				break;
			case SpriteInformationObject spriteInformationObject:
				{
					list.Add((Localization.Texture, spriteInformationObject.Texture));
				}
				break;
			default:
				break;
		}
		return list.Count != 0 ? list : Enumerable.Empty<(string, IUnityObjectBase)>();
	}

	public static IEnumerable<(string, string)> GetCustomStringProperties(this IUnityObjectBase Asset)
	{
		switch (Asset)
		{
			case IMonoScript monoScript:
				{
					return ToPropertyArray(monoScript);
				}
			case IMonoBehaviour monoBehaviour:
				{
					if (monoBehaviour.Script_C114P is { } monoScript)
					{
						return ToPropertyArray(monoScript);
					}
					else
					{
						goto default;
					}
				}
			case ITexture2D texture2D:
				{
					return new (string, string)[]
					{
						(Localization.Width, texture2D.Width_C28.ToString()),
						(Localization.Height, texture2D.Height_C28.ToString()),
						(Localization.Format, texture2D.Format_C28E.ToString()),
					};
				}
			case IMesh mesh:
				{
					return new (string, string)[]
					{
						(Localization.VertexCount, mesh.VertexData_C43.VertexCount.ToString()),
						(Localization.SubmeshCount, mesh.SubMeshes_C43.Count.ToString()),
					};
				}
			case IAudioClip audioClip:
				{
					return new (string, string)[]
					{
						(Localization.Channels, audioClip.Channels_C83.ToString()),
						(Localization.Frequency, audioClip.Frequency_C83.ToString()),
						(Localization.Length, audioClip.Length_C83.ToString()),
					};
				}
			case ITerrainData terrainData:
				{
					return new (string, string)[]
					{
						(Localization.Width, terrainData.Heightmap_C156.GetWidth().ToString()),
						(Localization.Height, terrainData.Heightmap_C156.GetHeight().ToString()),
					};
				}
			default:
				return Enumerable.Empty<(string, string)>();
		}

		static (string, string)[] ToPropertyArray(IMonoScript monoScript)
		{
			return new (string, string)[]
			{
				(Localization.AssemblyName, monoScript.AssemblyName_C115),
				(Localization.Namespace, monoScript.Namespace_C115),
				(Localization.ClassName, monoScript.ClassName_C115),
			};
		}
	}
}
