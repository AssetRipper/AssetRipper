using AssetRipper.Assets;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.SourceGenerated;
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
using System.Text;
using AssetType = AssetRipper.IO.Files.AssetType;
using AudioClip = AssetRipper.Mining.PredefinedAssets.AudioClip;
using Behaviour = AssetRipper.Mining.PredefinedAssets.Behaviour;
using Component = AssetRipper.Mining.PredefinedAssets.Component;
using Cubemap = AssetRipper.Mining.PredefinedAssets.Cubemap;
using GameObject = AssetRipper.Mining.PredefinedAssets.GameObject;
using Material = AssetRipper.Mining.PredefinedAssets.Material;
using Mesh = AssetRipper.Mining.PredefinedAssets.Mesh;
using MonoBehaviour = AssetRipper.Mining.PredefinedAssets.MonoBehaviour;
using MonoScript = AssetRipper.Mining.PredefinedAssets.MonoScript;
using NamedObject = AssetRipper.Mining.PredefinedAssets.NamedObject;
using Object = AssetRipper.Mining.PredefinedAssets.Object;
using Shader = AssetRipper.Mining.PredefinedAssets.Shader;
using Sprite = AssetRipper.Mining.PredefinedAssets.Sprite;
using TextAsset = AssetRipper.Mining.PredefinedAssets.TextAsset;
using Texture2D = AssetRipper.Mining.PredefinedAssets.Texture2D;

namespace AssetRipper.Export.UnityProjects.EngineAssets;

public sealed class PredefinedAssetCache
{
	private readonly record struct AssetMetaPtr(long FileID, UnityGuid Guid, AssetType AssetType);
	private readonly record struct NamedObjectKey(Utf8String Name, ClassIDType ClassID)
	{
		public NamedObjectKey(Utf8String Name, int ClassID) : this(Name, (ClassIDType)ClassID)
		{
		}
	}
	private readonly record struct ComponentKey(Utf8String GameObjectName, ClassIDType ClassID)
	{
		public ComponentKey(Utf8String GameObjectName, int ClassID) : this(GameObjectName, (ClassIDType)ClassID)
		{
		}
	}
	private readonly record struct BehaviourKey(Utf8String GameObjectName, bool Enabled, ClassIDType ClassID)
	{
		public BehaviourKey(Utf8String GameObjectName, byte Enabled, int ClassID) : this(GameObjectName, Enabled != 0, (ClassIDType)ClassID)
		{
		}
	}
	private readonly record struct MonoBehaviourKey(Utf8String Name, bool Enabled, Utf8String AssemblyName, Utf8String Namespace, Utf8String ClassName, Utf8String? GameObjectName);
	private readonly record struct MonoScriptKey(Utf8String AssemblyName, Utf8String Namespace, Utf8String ClassName);
	private readonly record struct AudioClipKey(Utf8String Name, int Channels, int Frequency, float Length);
	private readonly record struct MeshKey(Utf8String Name, int VertexCount, int SubMeshCount, AxisAlignedBoundingBox localAABB);
	private readonly record struct CubeMapKey(Utf8String Name, int Width, int Height);
	private readonly record struct Texture2DKey(Utf8String Name, int Width, int Height);
	private readonly record struct ShaderKey(Utf8String Name, int PropertyNamesHash);
	private readonly record struct GameObjectKey(Utf8String Name, uint Layer, int ComponentCount);
	private readonly record struct MaterialKey(Utf8String Name, Utf8String? ShaderName);
	private readonly record struct SpriteKey(Utf8String Name, Utf8String? TextureName);

	private static UnityGuid DGUID { get; } = new UnityGuid(0x00000000, 0x00000000, 0x0000000D, 0x00000000);
	/// <summary>
	/// The predefined guid for unity default resources
	/// </summary>
	internal static UnityGuid EGUID { get; } = new UnityGuid(0x00000000, 0x00000000, 0x0000000E, 0x00000000);
	/// <summary>
	/// The predefined guid for unity builtin extra
	/// </summary>
	internal static UnityGuid FGUID { get; } = new UnityGuid(0x00000000, 0x00000000, 0x0000000F, 0x00000000);

	private readonly Dictionary<NamedObjectKey, AssetMetaPtr> namedObjectDictionary = new();
	private readonly Dictionary<ComponentKey, AssetMetaPtr> componentDictionary = new();
	private readonly Dictionary<BehaviourKey, AssetMetaPtr> behaviourDictionary = new();
	private readonly Dictionary<MonoBehaviourKey, AssetMetaPtr> monoBehaviourDictionary = new();
	private readonly Dictionary<MonoScriptKey, AssetMetaPtr> monoScriptDictionary = new();
	private readonly Dictionary<AudioClipKey, AssetMetaPtr> audioClipDictionary = new();
	private readonly Dictionary<MeshKey, AssetMetaPtr> meshDictionary = new();
	private readonly Dictionary<CubeMapKey, AssetMetaPtr> cubeMapDictionary = new();
	private readonly Dictionary<Texture2DKey, AssetMetaPtr> texture2DDictionary = new();
	private readonly Dictionary<ShaderKey, AssetMetaPtr> shaderDictionary = new();
	private readonly Dictionary<GameObjectKey, AssetMetaPtr> gameObjectDictionary = new();
	private readonly Dictionary<MaterialKey, AssetMetaPtr> materialDictionary = new();
	private readonly Dictionary<TextAsset, AssetMetaPtr> textAssetDictionary = new();
	private readonly Dictionary<SpriteKey, AssetMetaPtr> spriteDictionary = new();

	public PredefinedAssetCache()
	{
	}

	public PredefinedAssetCache(EngineResourceData resourceData)
	{
		foreach ((long id, Object @object) in resourceData.DefaultResources)
		{
			TryAdd(@object, id, EGUID, AssetType.Internal);
		}
		foreach ((long id, Object @object) in resourceData.ExtraResources)
		{
			TryAdd(@object, id, FGUID, AssetType.Internal);
		}
	}

	public bool Contains(IUnityObjectBase asset, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		return asset switch
		{
			INamedObject namedObject => Contains(namedObject, out fileID, out guid, out assetType),
			IGameObject gameObject => Contains(gameObject, out fileID, out guid, out assetType),
			IComponent component => Contains(component, out fileID, out guid, out assetType),
			_ => SetDefaultAndReturnFalse(out fileID, out guid, out assetType),
		};
	}

	public bool Contains(INamedObject namedObject, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		return namedObject switch
		{
			IMaterial material => Contains(material, out fileID, out guid, out assetType),
			ITexture2D texture2D => Contains(texture2D, out fileID, out guid, out assetType),
			IMesh mesh => Contains(mesh, out fileID, out guid, out assetType),
			ITextAsset textAsset => Contains(textAsset, out fileID, out guid, out assetType),
			IShader shader => Contains(shader, out fileID, out guid, out assetType),
			IAudioClip audioClip => Contains(audioClip, out fileID, out guid, out assetType),
			ISprite sprite => Contains(sprite, out fileID, out guid, out assetType),
			_ => Default(namedObject, out fileID, out guid, out assetType),
		};

		bool Default(INamedObject namedObject, out long fileID, out UnityGuid guid, out AssetType assetType)
		{
			if (namedObjectDictionary.TryGetValue(new NamedObjectKey(namedObject.Name, namedObject.ClassID), out AssetMetaPtr assetMetaPtr))
			{
				(fileID, guid, assetType) = assetMetaPtr;
				return true;
			}
			else
			{
				return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
			}
		}
	}

	public bool Contains(IComponent component, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		return component is IBehaviour behaviour
			? Contains(behaviour, out fileID, out guid, out assetType)
			: Default(component, out fileID, out guid, out assetType);

		bool Default(IComponent component, out long fileID, out UnityGuid guid, out AssetType assetType)
		{
			if (component.GameObject_C2.IsNull())
			{
				//Not enough information to determine the asset.
			}
			else if (component.GameObject_C2P is not { } gameObject)
			{
				//GameObject could not be resolved.
			}
			else
			{
				//Component index should be included in the future.
				if (componentDictionary.TryGetValue(new ComponentKey(gameObject.Name, component.ClassID), out AssetMetaPtr assetMetaPtr))
				{
					(fileID, guid, assetType) = assetMetaPtr;
					return true;
				}
			}
			return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
		}
	}

	public bool Contains(IBehaviour behaviour, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		return behaviour is IMonoBehaviour monoBehaviour
			? Contains(monoBehaviour, out fileID, out guid, out assetType)
			: Default(behaviour, out fileID, out guid, out assetType);

		bool Default(IBehaviour behaviour, out long fileID, out UnityGuid guid, out AssetType assetType)
		{
			if (behaviour.GameObject_C8.IsNull())
			{
				//Not enough information to determine the asset.
			}
			else if (behaviour.GameObject_C8P is not { } gameObject)
			{
				//GameObject could not be resolved.
			}
			else
			{
				//Component index should be included in the future.
				if (behaviourDictionary.TryGetValue(new BehaviourKey(gameObject.Name, behaviour.Enabled_C8, behaviour.ClassID), out AssetMetaPtr assetMetaPtr))
				{
					(fileID, guid, assetType) = assetMetaPtr;
					return true;
				}
			}
			return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
		}
	}

	public bool Contains(ITextAsset textAsset, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		return textAsset switch
		{
			IMonoScript monoScript => Contains(monoScript, out fileID, out guid, out assetType),
			IShader shader => Contains(shader, out fileID, out guid, out assetType),
			_ => Default(textAsset, out fileID, out guid, out assetType),
		};

		bool Default(ITextAsset textAsset, out long fileID, out UnityGuid guid, out AssetType assetType)
		{
			if (textAssetDictionary.TryGetValue(new TextAsset(textAsset.Name, textAsset.Script_C49.Data), out AssetMetaPtr assetMetaPtr))
			{
				(fileID, guid, assetType) = assetMetaPtr;
				return true;
			}
			else
			{
				return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
			}
		}
	}

	public bool Contains(ITexture2D texture2D, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		return texture2D is ICubemap cubemap
			? Contains(cubemap, out fileID, out guid, out assetType)
			: Default(texture2D, out fileID, out guid, out assetType);

		bool Default(ITexture2D texture2D, out long fileID, out UnityGuid guid, out AssetType assetType)
		{
			if (texture2DDictionary.TryGetValue(new Texture2DKey(texture2D.Name, texture2D.Width_C28, texture2D.Height_C28), out AssetMetaPtr assetMetaPtr))
			{
				(fileID, guid, assetType) = assetMetaPtr;
				return true;
			}
			else
			{
				return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
			}
		}
	}

	public bool Contains(ICubemap cubemap, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		if (cubeMapDictionary.TryGetValue(new CubeMapKey(cubemap.Name, cubemap.Width, cubemap.Height), out AssetMetaPtr assetMetaPtr))
		{
			(fileID, guid, assetType) = assetMetaPtr;
			return true;
		}
		else
		{
			return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
		}
	}

	public bool Contains(IMaterial material, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		IShader? shader = material.Shader_C21P;
		if (shader is null && !material.Shader_C21.IsNull())
		{
			//Shader could not be resolved.
		}
		else if (materialDictionary.TryGetValue(new MaterialKey(material.Name, shader?.Name), out AssetMetaPtr assetMetaPtr))
		{
			(fileID, guid, assetType) = assetMetaPtr;
			return true;
		}
		return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
	}

	public bool Contains(ISprite sprite, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		ITexture2D? texture = sprite.TryGetTexture();
		if (texture is null && !sprite.RD.Texture.IsNull())
		{
			//Texture could not be resolved.
		}
		else if (spriteDictionary.TryGetValue(new SpriteKey(sprite.Name, texture?.Name), out AssetMetaPtr assetMetaPtr))
		{
			(fileID, guid, assetType) = assetMetaPtr;
			return true;
		}
		return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
	}

	public bool Contains(IMesh mesh, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		if (meshDictionary.TryGetValue(new MeshKey(mesh.Name, (int)mesh.VertexData.VertexCount, mesh.SubMeshes.Count, new()
		{
			Center = mesh.LocalAABB.Center,
			Extent = mesh.LocalAABB.Extent
		}), out AssetMetaPtr assetMetaPtr))
		{
			(fileID, guid, assetType) = assetMetaPtr;
			return true;
		}
		else
		{
			return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
		}
	}

	public bool Contains(IAudioClip audioClip, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		if (audioClipDictionary.TryGetValue(new AudioClipKey(audioClip.Name, audioClip.Channels, audioClip.Frequency, audioClip.Length), out AssetMetaPtr assetMetaPtr))
		{
			(fileID, guid, assetType) = assetMetaPtr;
			return true;
		}
		else
		{
			return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
		}
	}

	public bool Contains(IShader shader, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		int hash = shader.ParsedForm is { } parsedForm
			? HashStrings(parsedForm.PropInfo.Props.Select(p => p.Name))
			: 0;

		if (shaderDictionary.TryGetValue(new ShaderKey(shader.Name, hash), out AssetMetaPtr assetMetaPtr))
		{
			(fileID, guid, assetType) = assetMetaPtr;
			return true;
		}
		else
		{
			return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
		}
	}

	public bool Contains(IMonoScript monoScript, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		if (monoScriptDictionary.TryGetValue(new MonoScriptKey(monoScript.GetAssemblyNameFixed(), monoScript.Namespace, monoScript.ClassName_R), out AssetMetaPtr assetMetaPtr))
		{
			(fileID, guid, assetType) = assetMetaPtr;
			return true;
		}
		else
		{
			return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
		}
	}

	public bool Contains(IGameObject gameObject, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		if (gameObjectDictionary.TryGetValue(new GameObjectKey(gameObject.Name, gameObject.Layer, gameObject.GetComponentCount()), out AssetMetaPtr assetMetaPtr))
		{
			(fileID, guid, assetType) = assetMetaPtr;
			return true;
		}
		else
		{
			return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
		}
	}

	public bool Contains(IMonoBehaviour monoBehaviour, out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		if (monoBehaviour.Script.IsNull())
		{
			//Not enough information to determine the asset.
		}
		else if (monoBehaviour.ScriptP is not { } script)
		{
			//MonoScript could not be resolved.
		}
		else if (monoBehaviour.GameObject.IsNull())
		{
			MonoBehaviourKey key = new()
			{
				Name = monoBehaviour.Name,
				Enabled = monoBehaviour.Enabled != 0,
				AssemblyName = script.AssemblyName,
				Namespace = script.Namespace,
				ClassName = script.ClassName_R,
				GameObjectName = null,
			};
			if (monoBehaviourDictionary.TryGetValue(key, out AssetMetaPtr assetMetaPtr))
			{
				(fileID, guid, assetType) = assetMetaPtr;
				return true;
			}
		}
		else if (monoBehaviour.GameObjectP is not { } gameObject)
		{
			//GameObject could not be resolved.
		}
		else
		{
			//Component index should be included in the future.
			MonoBehaviourKey key = new()
			{
				Name = monoBehaviour.Name,
				Enabled = monoBehaviour.Enabled != 0,
				AssemblyName = script.AssemblyName,
				Namespace = script.Namespace,
				ClassName = script.ClassName_R,
				GameObjectName = gameObject.Name,
			};
			if (monoBehaviourDictionary.TryGetValue(key, out AssetMetaPtr assetMetaPtr))
			{
				(fileID, guid, assetType) = assetMetaPtr;
				return true;
			}
		}
		return SetDefaultAndReturnFalse(out fileID, out guid, out assetType);
	}

	private static bool SetDefaultAndReturnFalse(out long fileID, out UnityGuid guid, out AssetType assetType)
	{
		fileID = default;
		guid = default;
		assetType = default;
		return false;
	}

	public bool TryAdd(Object @object, long fileID, UnityGuid guid, AssetType assetType)
	{
		return @object switch
		{
			NamedObject namedObject => TryAdd(namedObject, fileID, guid, assetType),
			GameObject gameObject => TryAdd(gameObject, fileID, guid, assetType),
			Component component => TryAdd(component, fileID, guid, assetType),
			MonoScript monoScript => TryAdd(monoScript, fileID, guid, assetType),
			_ => false,
		};
	}

	public bool TryAdd(NamedObject namedObject, long fileID, UnityGuid guid, AssetType assetType)
	{
		return namedObject switch
		{
			Material material => TryAdd(material, fileID, guid, assetType),
			Texture2D texture2D => TryAdd(texture2D, fileID, guid, assetType),
			Mesh mesh => TryAdd(mesh, fileID, guid, assetType),
			TextAsset textAsset => TryAdd(textAsset, fileID, guid, assetType),
			Shader shader => TryAdd(shader, fileID, guid, assetType),
			AudioClip audioClip => TryAdd(audioClip, fileID, guid, assetType),
			Sprite sprite => TryAdd(sprite, fileID, guid, assetType),
			_ => TryAddDefault(namedObject, fileID, guid, assetType),
		};

		bool TryAddDefault(NamedObject namedObject, long fileID, UnityGuid guid, AssetType assetType)
		{
			return namedObjectDictionary.TryAdd(new NamedObjectKey(namedObject.Name, namedObject.TypeID), new AssetMetaPtr(fileID, guid, assetType));
		}
	}

	public bool TryAdd(Component component, long fileID, UnityGuid guid, AssetType assetType)
	{
		return component is Behaviour behaviour
			? TryAdd(behaviour, fileID, guid, assetType)
			: TryAddDefault(component, fileID, guid, assetType);

		bool TryAddDefault(Component component, long fileID, UnityGuid guid, AssetType assetType)
		{
			if (component.GameObject is null)
			{
				//Not enough information to determine the asset.
				return false;
			}
			else
			{
				return componentDictionary.TryAdd(new ComponentKey(component.GameObject, component.TypeID), new AssetMetaPtr(fileID, guid, assetType));
			}
		}
	}

	public bool TryAdd(Behaviour behaviour, long fileID, UnityGuid guid, AssetType assetType)
	{
		return behaviour is MonoBehaviour monoBehaviour
			? TryAdd(monoBehaviour, fileID, guid, assetType)
			: TryAddDefault(behaviour, fileID, guid, assetType);

		bool TryAddDefault(Behaviour behaviour, long fileID, UnityGuid guid, AssetType assetType)
		{
			if (behaviour.GameObject is null)
			{
				//Not enough information to determine the asset.
				return false;
			}
			else
			{
				return behaviourDictionary.TryAdd(new BehaviourKey(behaviour.GameObject, behaviour.Enabled, (ClassIDType)behaviour.TypeID), new AssetMetaPtr(fileID, guid, assetType));
			}
		}
	}

	public bool TryAdd(MonoBehaviour monoBehaviour, long fileID, UnityGuid guid, AssetType assetType)
	{
		MonoBehaviourKey key = new()
		{
			Name = monoBehaviour.Name,
			Enabled = monoBehaviour.Enabled,
			AssemblyName = monoBehaviour.AssemblyName,
			Namespace = monoBehaviour.Namespace,
			ClassName = monoBehaviour.ClassName,
			GameObjectName = monoBehaviour.GameObject,
		};
		return monoBehaviourDictionary.TryAdd(key, new AssetMetaPtr(fileID, guid, assetType));
	}

	public bool TryAdd(TextAsset textAsset, long fileID, UnityGuid guid, AssetType assetType)
	{
		return textAssetDictionary.TryAdd(textAsset, new AssetMetaPtr(fileID, guid, assetType));
	}

	public bool TryAdd(Texture2D texture2D, long fileID, UnityGuid guid, AssetType assetType)
	{
		return texture2D is Cubemap cubemap
			? TryAdd(cubemap, fileID, guid, assetType)
			: texture2DDictionary.TryAdd(new Texture2DKey(texture2D.Name, texture2D.Width, texture2D.Height), new AssetMetaPtr(fileID, guid, assetType));
	}

	public bool TryAdd(Cubemap cubemap, long fileID, UnityGuid guid, AssetType assetType)
	{
		return cubeMapDictionary.TryAdd(new CubeMapKey(cubemap.Name, cubemap.Width, cubemap.Height), new AssetMetaPtr(fileID, guid, assetType));
	}

	public bool TryAdd(Mesh mesh, long fileID, UnityGuid guid, AssetType assetType)
	{
		return meshDictionary.TryAdd(new MeshKey(mesh.Name, mesh.VertexCount, mesh.SubMeshCount, new()
		{
			Center = mesh.LocalAABB.Center,
			Extent = mesh.LocalAABB.Extent
		}), new AssetMetaPtr(fileID, guid, assetType));
	}

	public bool TryAdd(AudioClip audioClip, long fileID, UnityGuid guid, AssetType assetType)
	{
		return audioClipDictionary.TryAdd(new AudioClipKey(audioClip.Name, audioClip.Channels, audioClip.Frequency, audioClip.Length), new AssetMetaPtr(fileID, guid, assetType));
	}

	public bool TryAdd(Shader shader, long fileID, UnityGuid guid, AssetType assetType)
	{
		return shaderDictionary.TryAdd(new ShaderKey(shader.Name, HashStrings(shader.PropertyNames)), new AssetMetaPtr(fileID, guid, assetType));
	}

	public bool TryAdd(MonoScript monoScript, long fileID, UnityGuid guid, AssetType assetType)
	{
		return monoScriptDictionary.TryAdd(new MonoScriptKey(monoScript.AssemblyName, monoScript.Namespace, monoScript.ClassName), new AssetMetaPtr(fileID, guid, assetType));
	}

	public bool TryAdd(GameObject gameObject, long fileID, UnityGuid guid, AssetType assetType)
	{
		return gameObjectDictionary.TryAdd(new GameObjectKey(gameObject.Name, gameObject.Layer, gameObject.Components.Length), new AssetMetaPtr(fileID, guid, assetType));
	}

	public bool TryAdd(Material material, long fileID, UnityGuid guid, AssetType assetType)
	{
		return materialDictionary.TryAdd(new MaterialKey(material.Name, material.Shader), new AssetMetaPtr(fileID, guid, assetType));
	}

	public bool TryAdd(Sprite sprite, long fileID, UnityGuid guid, AssetType assetType)
	{
		return spriteDictionary.TryAdd(new SpriteKey(sprite.Name, sprite.Texture), new AssetMetaPtr(fileID, guid, assetType));
	}

	private static int HashStrings(ReadOnlySpan<string> strings)
	{
		HashCode hashCode = new();
		foreach (string str in strings)
		{
			hashCode.AddBytes(Encoding.UTF8.GetBytes(str));
		}
		return hashCode.ToHashCode();
	}

	private static int HashStrings(IEnumerable<Utf8String> strings)
	{
		HashCode hashCode = new();
		foreach (Utf8String str in strings)
		{
			hashCode.AddBytes(str.Data);
		}
		return hashCode.ToHashCode();
	}
}
