using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.Export.UnityProjects.Project;

public record class PackageManifest([property: JsonPropertyName("dependencies")] Dictionary<string, string> Dependencies)
{
	public PackageManifest() : this(new Dictionary<string, string>())
	{
	}

	public void Save(Stream stream)
	{
		JsonSerializer.Serialize(stream, this, PackageManifestSerializerContext.Default.PackageManifest);
	}

	public static PackageManifest CreateDefault(UnityVersion version)
	{
		PackageManifest manifest = new();
		manifest.AddDefaultDependencies(version);
		return manifest;
	}

	public void AddDefaultDependencies(UnityVersion version)
	{
		// This should be accurate to at least 2023

		Dependencies.TryAdd("com.unity.modules.ai", "1.0.0");
		if (version.GreaterThanOrEquals(2019, 2))
		{
			Dependencies.TryAdd("com.unity.modules.androidjni", "1.0.0");
		}
		Dependencies.TryAdd("com.unity.modules.animation", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.assetbundle", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.audio", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.cloth", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.director", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.imageconversion", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.imgui", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.jsonserialize", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.particlesystem", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.physics", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.physics2d", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.screencapture", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.terrain", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.terrainphysics", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.tilemap", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.ui", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.uielements", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.umbra", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.unityanalytics", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.unitywebrequest", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.unitywebrequestassetbundle", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.unitywebrequestaudio", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.unitywebrequesttexture", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.unitywebrequestwww", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.vehicles", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.video", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.vr", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.wind", "1.0.0");
		Dependencies.TryAdd("com.unity.modules.xr", "1.0.0");
	}
}
