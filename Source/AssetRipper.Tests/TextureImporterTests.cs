using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Classes.ClassID_1006;
using AssetRipper.SourceGenerated.Extensions;
using Swizzle = AssetRipper.SourceGenerated.Enums.TextureImporterSwizzle;

namespace AssetRipper.Tests;

internal class TextureImporterTests
{
	[Test]
	public async Task DefaultSwizzleIsZero()
	{

		{
			ITextureImporter importer = CreateImporterWithSwizzleField();
			await Assert.That(importer.Has_Swizzle()).IsTrue();
			await Assert.That(importer.Swizzle).IsZero();
		}

		{
			ITextureImporter importer = CreateImporterWithoutSwizzleField();
			await Assert.That(importer.Has_Swizzle()).IsFalse();
			await Assert.That(importer.Swizzle).IsZero();
		}
	}

	[Test]
	public async Task DefaultFieldSwizzleIsRRRR()
	{

		{
			ITextureImporter importer = CreateImporterWithSwizzleField();
			importer.GetSwizzle(out Swizzle channel0, out Swizzle channel1, out Swizzle channel2, out Swizzle channel3);
			await Assert.That(channel0).IsEqualTo(Swizzle.R);
			await Assert.That(channel1).IsEqualTo(Swizzle.R);
			await Assert.That(channel2).IsEqualTo(Swizzle.R);
			await Assert.That(channel3).IsEqualTo(Swizzle.R);
			await Assert.That(importer.Swizzle).IsZero();//Check that GetSwizzle hasn't changed the value.
		}
	}

	[Test]
	public async Task SwizzleHasCorrectRGBA()
	{

		{
			ITextureImporter importer = CreateImporterWithSwizzleField();
			importer.SetSwizzle(Swizzle.R, Swizzle.G, Swizzle.B, Swizzle.A);
			await Assert.That(importer.Swizzle).IsEqualTo(0x_03_02_01_00);
		}
	}

	[Test]
	public async Task FieldlessSwizzleIsAlwaysRGBA()
	{

		{
			ITextureImporter importer = CreateImporterWithoutSwizzleField();
			importer.Swizzle = 534232;//arbitrary
			await Assert.That(importer.Swizzle).IsZero();
			importer.SetSwizzle(Swizzle.One, Swizzle.OneMinusR, Swizzle.G, Swizzle.OneMinusA);//arbitrary
			importer.GetSwizzle(out Swizzle channel0, out Swizzle channel1, out Swizzle channel2, out Swizzle channel3);
			await Assert.That(channel0).IsEqualTo(Swizzle.R);
			await Assert.That(channel1).IsEqualTo(Swizzle.G);
			await Assert.That(channel2).IsEqualTo(Swizzle.B);
			await Assert.That(channel3).IsEqualTo(Swizzle.A);
		}
	}

	[Test]
	[MethodDataSource(nameof(GetRandomSwizzles))]
	public async Task GetSwizzleMatchesSetSwizzle(Swizzle s0, Swizzle s1, Swizzle s2, Swizzle s3)
	{

		{
			ITextureImporter importer = CreateImporterWithSwizzleField();
			importer.SetSwizzle(s0, s1, s2, s3);
			importer.GetSwizzle(out Swizzle channel0, out Swizzle channel1, out Swizzle channel2, out Swizzle channel3);
			await Assert.That(channel0).IsEqualTo(s0);
			await Assert.That(channel1).IsEqualTo(s1);
			await Assert.That(channel2).IsEqualTo(s2);
			await Assert.That(channel3).IsEqualTo(s3);
		}
	}

	public static IEnumerable<(Swizzle S0, Swizzle S1, Swizzle S2, Swizzle S3)> GetRandomSwizzles()
	{
		const int count = 100;
		Random random = new(42);
		for (int i = 0; i < count; i++)
		{
			yield return (NextSwizzle(random), NextSwizzle(random), NextSwizzle(random), NextSwizzle(random));
		}

		static Swizzle NextSwizzle(Random random)
		{
			Swizzle[] values = Enum.GetValues<Swizzle>();
			return values[random.Next(values.Length)];
		}
	}

	private static ITextureImporter CreateImporterWithSwizzleField()
	{
		return AssetCreator.CreateTextureImporter(new UnityVersion(2022, 2));
	}

	private static ITextureImporter CreateImporterWithoutSwizzleField()
	{
		return AssetCreator.CreateTextureImporter(new UnityVersion(2020, 1));
	}
}