using AssetRipper.Assets;
using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_130;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Classes.ClassID_8;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Classes.ClassID_89;
using AssetRipper.SourceGenerated.Extensions;
using AudioClip = AssetRipper.Mining.PredefinedAssets.AudioClip;
using Behaviour = AssetRipper.Mining.PredefinedAssets.Behaviour;
using Component = AssetRipper.Mining.PredefinedAssets.Component;
using Cubemap = AssetRipper.Mining.PredefinedAssets.Cubemap;
using GameObject = AssetRipper.Mining.PredefinedAssets.GameObject;
using Material = AssetRipper.Mining.PredefinedAssets.Material;
using Mesh = AssetRipper.Mining.PredefinedAssets.Mesh;
using MonoBehaviour = AssetRipper.Mining.PredefinedAssets.MonoBehaviour;
using MonoScript = AssetRipper.Mining.PredefinedAssets.MonoScript;
using Object = AssetRipper.Mining.PredefinedAssets.Object;
using Shader = AssetRipper.Mining.PredefinedAssets.Shader;
using Sprite = AssetRipper.Mining.PredefinedAssets.Sprite;
using TextAsset = AssetRipper.Mining.PredefinedAssets.TextAsset;
using Texture2D = AssetRipper.Mining.PredefinedAssets.Texture2D;

namespace AssetRipper.Export.UnityProjects.EngineAssets;

public static class EngineAssetsFactory
{
	public static Object? Create(IUnityObjectBase asset)
	{
		return asset switch
		{
			INamedObject namedObject => Create(namedObject),
			IGameObject gameObject => Create(gameObject),
			IComponent component => Create(component),
			_ => null,
		};
	}

	public static Object? Create(INamedObject namedObject)
	{
		return namedObject switch
		{
			IMaterial material => Create(material),
			ITexture2D texture2D => Create(texture2D),
			IMesh mesh => Create(mesh),
			ITextAsset textAsset => Create(textAsset),
			IShader shader => Create(shader),
			IAudioClip audioClip => Create(audioClip),
			ISprite sprite => Create(sprite),
			_ => new GenericNamedObject()
			{
				Name = namedObject.Name,
				TypeID = namedObject.ClassID,
			},
		};
	}

	public static Component? Create(IComponent component)
	{
		return component is IBehaviour behaviour
			? Create(behaviour)
			: new GenericComponent()
			{
				GameObject = component.GameObject_C2P?.Name,
				TypeID = component.ClassID,
			};
	}

	public static Behaviour? Create(IBehaviour behaviour)
	{
		return behaviour is IMonoBehaviour monoBehaviour
			? Create(monoBehaviour)
			: new GenericBehaviour()
			{
				Enabled = behaviour.Enabled_C8 != 0,
				GameObject = behaviour.GameObject_C8P?.Name,
				TypeID = behaviour.ClassID,
			};
	}

	public static Object Create(ITextAsset textAsset)
	{
		return textAsset switch
		{
			IMonoScript monoScript => Create(monoScript),
			IShader shader => Create(shader),
			_ => new TextAsset(textAsset.Name, textAsset.Script_C49),
		};
	}

	public static Texture2D Create(ITexture2D texture2D)
	{
		return texture2D is ICubemap cubemap
			? Create(cubemap)
			: new Texture2D()
			{
				Name = texture2D.Name,
				Width = texture2D.Width_C28,
				Height = texture2D.Height_C28,
			};
	}

	public static Cubemap Create(ICubemap cubemap)
	{
		return new Cubemap()
		{
			Name = cubemap.Name,
			Width = cubemap.Width,
			Height = cubemap.Height,
		};
	}

	public static Material? Create(IMaterial material)
	{
		IShader? shader = material.Shader_C21P;
		if (shader is null && !material.Shader_C21.IsNull())
		{
			return null;
		}

		return new Material()
		{
			Name = material.Name,
			Shader = shader?.Name,
		};
	}

	public static Sprite? Create(ISprite sprite)
	{
		ITexture2D? texture = sprite.TryGetTexture();
		if (texture is null && !sprite.RD.Texture.IsNull())
		{
			return null;
		}

		return new Sprite()
		{
			Name = sprite.Name,
			Texture = texture?.Name,
		};
	}

	public static Mesh Create(IMesh mesh)
	{
		return new Mesh()
		{
			Name = mesh.Name,
			VertexCount = (int)mesh.VertexData.VertexCount,
			SubMeshCount = mesh.SubMeshes.Count,
			LocalAABB = new()
			{
				Center = mesh.LocalAABB.Center,
				Extent = mesh.LocalAABB.Extent
			}
		};
	}

	public static AudioClip Create(IAudioClip audioClip)
	{
		return new AudioClip()
		{
			Name = audioClip.Name,
			Channels = audioClip.Channels,
			Frequency = audioClip.Frequency,
			Length = audioClip.Length,
		};
	}

	public static Shader Create(IShader shader)
	{
		return new Shader()
		{
			Name = shader.Name,
			PropertyNames = shader.ParsedForm?.PropInfo.Props.Select(p => p.Name.String).ToArray() ?? Array.Empty<string>(),
		};
	}

	public static MonoScript Create(IMonoScript monoScript)
	{
		return new MonoScript()
		{
			AssemblyName = monoScript.AssemblyName,
			Namespace = monoScript.Namespace,
			ClassName = monoScript.ClassName_R,
		};
	}

	public static GameObject Create(IGameObject gameObject)
	{
		return new GameObject()
		{
			Name = gameObject.Name,
			Layer = gameObject.Layer,
			Components = gameObject.GetComponentAccessList().Select(c => c?.ClassID ?? 2).ToArray(),
		};
	}

	public static MonoBehaviour? Create(IMonoBehaviour monoBehaviour)
	{
		IMonoScript? monoScript = monoBehaviour.ScriptP;
		if (monoScript is null)
		{
			return null;
		}

		IGameObject? gameObject = monoBehaviour.GameObjectP;
		if (gameObject is null && !monoBehaviour.GameObject.IsNull())
		{
			return null;
		}

		return new MonoBehaviour()
		{
			Name = monoBehaviour.Name,
			Enabled = monoBehaviour.Enabled != 0,
			AssemblyName = monoScript.AssemblyName,
			Namespace = monoScript.Namespace,
			ClassName = monoScript.ClassName_R,
			GameObject = gameObject?.Name
		};
	}
}
