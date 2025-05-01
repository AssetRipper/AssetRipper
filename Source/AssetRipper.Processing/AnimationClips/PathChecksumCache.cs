using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Metadata;
using AssetRipper.Checksum;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.SerializationLogic;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_111;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_90;
using AssetRipper.SourceGenerated.Classes.ClassID_95;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing.AnimationClips;

/// <summary>
/// Attempts to recover field paths from <see cref="uint"/> hash values.
/// </summary>
/// <remarks>
/// Replicates Unity CRC32 checksum usage for field names and paths.
/// </remarks>
public readonly struct PathChecksumCache
{
	public PathChecksumCache(GameData gameData)
	{
		this.assemblyManager = gameData.AssemblyManager;
		BuildPathsCache(gameData.GameBundle);
	}

	private readonly Dictionary<string, uint> cachedPropertyNames = new() { { string.Empty, 0 } };
	private readonly Dictionary<uint, string> cachedChecksums = new() { { 0, string.Empty } };
	private readonly HashSet<AssetInfo> processedAssets = new();
	private readonly IAssemblyManager assemblyManager;
	
	private void AddAnimatorPathsToCache(IAnimator animator)
	{
		IAvatar? avatar = animator.AvatarP;
		if (avatar != null)
		{
			 AddAvatarTOS(avatar);
			 return;
		}
		
		if (animator.Has_HasTransformHierarchy() && !animator.HasTransformHierarchy)
		{
			return;
		}

		IGameObject? gameObject = animator.GameObjectP;
		if (gameObject is null)
		{
			return;
		}
		AddGameObjectPathsToCacheRecursive(gameObject, string.Empty);
	}

	private void AddGameObjectPathsToCacheRecursive(IGameObject parent, string parentPath)
	{
		ITransform transform = parent.GetTransform();

		foreach (ITransform? childTransform in transform.Children_C4P)
		{
			IGameObject? child = childTransform?.GameObject_C4P;
			if (child is null)
			{
				continue;
			}

			string path = string.IsNullOrEmpty(parentPath)
				? child.Name
				: $"{parentPath}/{child.Name}";

			uint pathHash = Crc32Algorithm.HashUTF8(path);
			AddKeys(pathHash, path);

			AddGameObjectPathsToCacheRecursive(child, path);
		}
	}
	
	private void AddAnimationPathsToCache(IAnimation animation)
	{
		IGameObject? go = animation.GameObjectP;
		if (go is null)
		{
			return;
		}
		AddGameObjectPathsToCacheRecursive(go, string.Empty);
	}
	
	private void AddAvatarTOS(IAvatar avatar)
	{
		foreach ((uint key, Utf8String value) in avatar.TOS)
		{
			AddKeys(key, value);
		}
	}

	private void BuildPathsCache(GameBundle bundle)
	{
		foreach (IUnityObjectBase asset in bundle.FetchAssets())
		{
			switch (asset)
			{
				case IAvatar avatar:
					AddAvatarTOS(avatar);
					break;
				case IAnimator animator:
					AddAnimatorPathsToCache(animator);
					break;
				case IAnimation animation:
					AddAnimationPathsToCache(animation);
					break;
			}
		}
	}
	
	public uint Add(string path)
	{
		if (cachedPropertyNames.TryGetValue(path, out uint value))
		{
			return value;
		}

		uint output = Crc32Algorithm.HashUTF8(path);

		AddKeys(output, path);
		return output;
	}

	public void Add(IMonoScript script)
	{
		if (!processedAssets.Add(script.AssetInfo))
		{
			return;
		}

		SerializableType? behaviour = script.GetBehaviourType(assemblyManager);

		if (behaviour is null)
		{
			return;
		}

		for (int f = 0; f < behaviour.Fields.Count; f++)
		{
			SerializableType.Field field = behaviour.Fields[f];
			AddFieldRecursively(field);
		}
	}

	private void AddFieldRecursively(SerializableType.Field field, string path = "", int depth = 0)
	{
		// Time out if we go too deeply to prevent infinite recursion
		if (depth > 10)
		{
			return;
		}
		
		if (field.Type.IsPrimitive())
		{
			// Only primitive fields can be animated.
			Add($"{path}{field.Name}");
		}
		else
		{
			string basePath = $"{path}{field.Name}.";
			for (int i = 0; i < field.Type.Fields.Count; i++)
			{
				AddFieldRecursively(field.Type.Fields[i], basePath, depth + 1);
			}
		}
	}

	public bool TryGetPath(uint identifier, [NotNullWhen(true)] out string? path)
	{
		return cachedChecksums.TryGetValue(identifier, out path);
	}

	public void Reset()
	{
		cachedPropertyNames.Clear();
		cachedChecksums.Clear();
		processedAssets.Clear();
	}

	private void AddKeys(uint checksum, string propertyName)
	{
		cachedPropertyNames[propertyName] = checksum;
		cachedChecksums[checksum] = propertyName;
	}
}
