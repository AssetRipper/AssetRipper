using AssetRipper.Assets;
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
			list.Add(("Main Asset", Asset.MainAsset));
		}
		switch (Asset)
		{
			case IComponent component:
				{
					if (component.GameObject_C2P is { } gameObject)
					{
						list.Add(("GameObject", gameObject));
						if (component is IRenderer && gameObject.TryGetComponent<IMeshFilter>()?.Mesh_C33P is { } rendererMesh)
						{
							list.Add(("Mesh", rendererMesh));
						}
					}
					if (component is IMonoBehaviour monoBehaviour && monoBehaviour.Script_C114P is { } monoScript)
					{
						list.Add(("Script", monoScript));
					}
					else if (component is IMeshFilter meshFilter && meshFilter.Mesh_C33P is { } mesh)
					{
						list.Add(("Mesh", mesh));
					}
					else if (component is IAudioSource audioSource && audioSource.AudioClip_C82P is { } audioSourceClip)
					{
						list.Add(("Audio Clip", audioSourceClip));
					}
				}
				break;
			case IGameObject gameObject:
				{
					if (gameObject.TryGetComponent(out ITransform? transform))
					{
						list.Add(("Transform", transform));
					}
				}
				break;
			case IMaterial material:
				{
					if (material.Shader_C21P is { } shader)
					{
						list.Add(("Shader", shader));
					}
				}
				break;
			case SpriteInformationObject spriteInformationObject:
				{
					list.Add(("Texture", spriteInformationObject.Texture));
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
						("Width", texture2D.Width_C28.ToString()),
						("Height", texture2D.Height_C28.ToString()),
						("Format", texture2D.Format_C28E.ToString()),
					};
				}
			case IMesh mesh:
				{
					return new (string, string)[]
					{
						("Vertex Count", mesh.VertexData_C43.VertexCount.ToString()),
						("Submesh Count", mesh.SubMeshes_C43.Count.ToString()),
					};
				}
			case IAudioClip audioClip:
				{
					return new (string, string)[]
					{
						("Channels", audioClip.Channels_C83.ToString()),
						("Frequency", audioClip.Frequency_C83.ToString()),
						("Length", audioClip.Length_C83.ToString()),
					};
				}
			case ITerrainData terrainData:
				{
					return new (string, string)[]
					{
						("Width", terrainData.Heightmap_C156.GetWidth().ToString()),
						("Height", terrainData.Heightmap_C156.GetHeight().ToString()),
					};
				}
			default:
				return Enumerable.Empty<(string, string)>();
		}

		static (string, string)[] ToPropertyArray(IMonoScript monoScript)
		{
			return new (string, string)[]
			{
				("Assembly Name", monoScript.AssemblyName_C115),
				("Namespace", monoScript.Namespace_C115),
				("Class Name", monoScript.ClassName_C115),
			};
		}
	}
}
