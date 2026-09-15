using AssetRipper.Processing.Extended.PathOverrides;
using System.Text.Json;

namespace AssetRipper.Processing.Extended.Tests;

public class PathOverrideDataTests
{
	private const string DocumentedJson = """
		{
			"Files": {
				"cab-bcaf22789432bda1e5d0eea9d2521ddd": {
					"4476349470337976665": "Assets/AssetRenamed.txt"
				},
				"level1.assets": {
					"1": "Assets/Prefabs/Prefab1.prefab",
					"12": "Assets/Images/MyTexture.png"
				}
			}
		}
		""";

	[Test]
	public void DeserializesTheDocumentedFormat()
	{
		PathOverrideData data = JsonSerializer.Deserialize(DocumentedJson, PathOverrideDataContext.Default.PathOverrideData)!;

		Assert.That(data.Files, Has.Count.EqualTo(2));
		Assert.Multiple(() =>
		{
			Assert.That(data.Files["cab-bcaf22789432bda1e5d0eea9d2521ddd"][4476349470337976665L], Is.EqualTo("Assets/AssetRenamed.txt"));
			Assert.That(data.Files["level1.assets"][1L], Is.EqualTo("Assets/Prefabs/Prefab1.prefab"));
			Assert.That(data.Files["level1.assets"][12L], Is.EqualTo("Assets/Images/MyTexture.png"));
		});
	}

	[Test]
	public void RoundTripsThroughSerialization()
	{
		PathOverrideData original = new();
		original.Files.Add("level0", new Dictionary<long, string> { { 7L, "Assets/Seven.asset" } });

		string json = JsonSerializer.Serialize(original, PathOverrideDataContext.Default.PathOverrideData);
		PathOverrideData restored = JsonSerializer.Deserialize(json, PathOverrideDataContext.Default.PathOverrideData)!;

		Assert.That(restored.Files["level0"][7L], Is.EqualTo("Assets/Seven.asset"));
	}

	[Test]
	public void EmptyJsonObjectYieldsNoOverrides()
	{
		PathOverrideData data = JsonSerializer.Deserialize("{}", PathOverrideDataContext.Default.PathOverrideData)!;

		Assert.That(data.Files, Is.Empty);
	}
}
