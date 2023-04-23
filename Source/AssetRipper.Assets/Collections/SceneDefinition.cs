using AssetRipper.IO.Files;

namespace AssetRipper.Assets.Collections;

public sealed class SceneDefinition
{
	private readonly List<AssetCollection> collections = new();

	private SceneDefinition()
	{
	}

	/// <summary>
	/// Creates a new <see cref="SceneDefinition"/> from the given name and guid.
	/// </summary>
	/// <param name="name">The name of the scene.</param>
	/// <param name="guid">The predefined <see cref="UnityGUID"/> for the scene. If default, a random one is assigned.</param>
	/// <returns></returns>
	public static SceneDefinition FromName(string name, UnityGUID guid = default)
	{
		return new()
		{
			Name = name,
			Path = $"Assets/Scenes/{name}",
			GUID = guid.IsZero ? UnityGUID.NewGuid() : guid,
		};
	}

	/// <summary>
	/// Creates a new <see cref="SceneDefinition"/> from the given path and guid.
	/// </summary>
	/// <param name="path">The relative path to the scene.</param>
	/// <param name="guid">The predefined <see cref="UnityGUID"/> for the scene. If default, a random one is assigned.</param>
	/// <returns></returns>
	public static SceneDefinition FromPath(string path, UnityGUID guid = default)
	{
		return new()
		{
			Name = System.IO.Path.GetFileName(path),
			Path = path,
			GUID = guid.IsZero ? UnityGUID.NewGuid() : guid,
		};
	}

	/// <summary>
	/// The name of the scene, without any file extension.
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// The scene path without any file extension, relative to the project root directory.
	/// </summary>
	public required string Path { get; init; }

	/// <summary>
	/// The GUID of this scene. It gets used in the scene's meta file. This will not be <see cref="UnityGUID.Zero"/>.
	/// </summary>
	public required UnityGUID GUID { get; init; }

	/// <summary>
	/// All the <see cref="AssetCollection"/>s that make up this scene.
	/// </summary>
	public IReadOnlyList<AssetCollection> Collections => collections;

	/// <summary>
	/// Adds an <see cref="AssetCollection"/> to this <see cref="SceneDefinition"/> and sets its <see cref="AssetCollection.Scene"/> property.
	/// </summary>
	/// <param name="collection">The collection to be added.</param>
	public void AddCollection(AssetCollection collection)
	{
		ThrowIfAlreadyPartOfAScene(collection);
		collections.Add(collection);
		collection.Scene = this;
	}

	private static void ThrowIfAlreadyPartOfAScene(AssetCollection collection)
	{
		if (collection.Scene is not null)
		{
			throw new InvalidOperationException($"{collection} is already part of a scene.");
		}
	}

	public override string ToString() => Name;
}
