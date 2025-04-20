using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Processing.Prefabs;
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

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class InformationTab(IUnityObjectBase asset, AssetPath path) : AssetHtmlTab(asset)
{
	public override string DisplayName => Localization.AssetTabInformation;
	public override string HtmlName => "information";

	public override void Write(TextWriter writer)
	{
		using (new Table(writer).WithClass("table").End())
		{
			using (new Tbody(writer).End())
			{
				if (Asset.MainAsset is SceneHierarchyObject sceneHierarchyObject)
				{
					using (new Tr(writer).End())
					{
						new Th(writer).Close(Localization.Scene);
						using (new Td(writer).End())
						{
							PathLinking.WriteLink(writer, (ScenePath)sceneHierarchyObject.Scene.Collections[0].GetPath(), sceneHierarchyObject.Scene.Name);
						}
					}
				}
				using (new Tr(writer).End())
				{
					new Th(writer).Close(Localization.Collection);
					using (new Td(writer).End())
					{
						PathLinking.WriteLink(writer, path.CollectionPath, Asset.Collection.Name);
					}
				}
				using (new Tr(writer).End())
				{
					new Th(writer).Close(Localization.PathId);
					new Td(writer).Close(Asset.PathID.ToString());
				}
				using (new Tr(writer).End())
				{
					new Th(writer).Close(Localization.ClassIdTypeNumber);
					new Td(writer).Close(Asset.ClassID.ToString());
				}
				using (new Tr(writer).End())
				{
					new Th(writer).Close(Localization.ClassIdTypeName);
					new Td(writer).Close(Asset.ClassName);
				}
				if (Asset.OriginalPath is not null)
				{
					using (new Tr(writer).End())
					{
						new Th(writer).Close(Localization.OriginalPath);
						new Td(writer).Close(Asset.OriginalPath);
					}
				}
				if (Asset.OverridePath is not null)
				{
					using (new Tr(writer).End())
					{
						new Th(writer).Close(Localization.OverridePath);
						new Td(writer).Close(Asset.OverridePath);
					}
				}
				if (!string.IsNullOrEmpty(Asset.AssetBundleName))
				{
					using (new Tr(writer).End())
					{
						new Th(writer).Close(Localization.AssetBundleName);
						new Td(writer).Close(Asset.AssetBundleName);
					}
				}
				foreach ((string key, IUnityObjectBase value) in GetCustomReferenceProperties(Asset))
				{
					using (new Tr(writer).End())
					{
						new Th(writer).Close(key);
						using (new Td(writer).End())
						{
							PathLinking.WriteLink(writer, value);
						}
					}
				}
				foreach ((string key, string value) in GetCustomStringProperties(Asset))
				{
					using (new Tr(writer).End())
					{
						new Th(writer).Close(key);
						new Td(writer).Close(value);
					}
				}
			}
		}
	}

	private static IEnumerable<(string, IUnityObjectBase)> GetCustomReferenceProperties(IUnityObjectBase asset)
	{
		if (asset.MainAsset is not null && asset.MainAsset != asset)
		{
			yield return (Localization.MainAsset, asset.MainAsset);
		}
		switch (asset)
		{
			case IComponent component:
				{
					if (component.GameObject_C2P is { } gameObject)
					{
						yield return (Localization.GameObject, gameObject);
						if (component is IRenderer && gameObject.TryGetComponent<IMeshFilter>()?.MeshP is { } rendererMesh)
						{
							yield return (Localization.Mesh, rendererMesh);
						}
					}
					if (component is IMonoBehaviour monoBehaviour && monoBehaviour.ScriptP is { } monoScript)
					{
						yield return (Localization.Script, monoScript);
					}
					else if (component is IMeshFilter meshFilter && meshFilter.MeshP is { } mesh)
					{
						yield return (Localization.Mesh, mesh);
					}
					else if (component is IAudioSource audioSource && audioSource.AudioClipP is { } audioSourceClip)
					{
						yield return (Localization.AudioClip, audioSourceClip);
					}
				}
				break;
			case IGameObject gameObject:
				{
					if (gameObject.TryGetComponent(out ITransform? transform))
					{
						yield return (Localization.Transform, transform);
					}
				}
				break;
			case IMaterial material:
				{
					if (material.Shader_C21P is { } shader)
					{
						yield return (Localization.Shader, shader);
					}
				}
				break;
			case SpriteInformationObject spriteInformationObject:
				{
					yield return (Localization.Texture, spriteInformationObject.Texture);
				}
				break;
			default:
				break;
		}
	}

	private static IEnumerable<(string, string)> GetCustomStringProperties(IUnityObjectBase asset)
	{
		switch (asset)
		{
			case IMonoScript monoScript:
				{
					return GetProperties(monoScript);
				}
			case IMonoBehaviour monoBehaviour:
				{
					if (monoBehaviour.ScriptP is { } monoScript)
					{
						return GetProperties(monoScript);
					}
					else
					{
						goto default;
					}
				}
			case ITexture2D texture2D:
				{
					return
					[
						(Localization.Width, texture2D.Width_C28.ToString()),
						(Localization.Height, texture2D.Height_C28.ToString()),
						(Localization.Format, texture2D.Format_C28E.ToString()),
					];
				}
			case IMesh mesh:
				{
					return
					[
						(Localization.VertexCount, mesh.VertexData.VertexCount.ToString()),
						(Localization.SubmeshCount, mesh.SubMeshes.Count.ToString()),
					];
				}
			case IAudioClip audioClip:
				{
					return
					[
						(Localization.Channels, audioClip.Channels.ToString()),
						(Localization.Frequency, audioClip.Frequency.ToString()),
						(Localization.Length, audioClip.Length.ToString()),
					];
				}
			case ITerrainData terrainData:
				{
					return
					[
						(Localization.Width, terrainData.Heightmap.GetWidth().ToString()),
						(Localization.Height, terrainData.Heightmap.GetHeight().ToString()),
					];
				}
			default:
				return Enumerable.Empty<(string, string)>();
		}

		static IEnumerable<(string, string)> GetProperties(IMonoScript monoScript)
		{
			return
			[
				(Localization.AssemblyName, monoScript.AssemblyName),
				(Localization.Namespace, monoScript.Namespace),
				(Localization.ClassName, monoScript.ClassName_R),
			];
		}
	}
}
