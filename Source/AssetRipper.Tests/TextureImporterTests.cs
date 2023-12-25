using AssetRipper.Primitives;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1006;
using AssetRipper.SourceGenerated.Extensions;
using NUnit.Framework.Internal;
using Swizzle = AssetRipper.SourceGenerated.Enums.TextureImporterSwizzle;

namespace AssetRipper.Tests;

internal class TextureImporterTests
{
	[Test]
	public void DefaultSwizzleIsZero()
	{
		Assert.Multiple(() =>
		{
			ITextureImporter importer = CreateImporterWithSwizzleField();
			Assert.That(importer.Has_Swizzle(), Is.True);
			Assert.That(importer.Swizzle, Is.EqualTo(0));
		});
		Assert.Multiple(() =>
		{
			ITextureImporter importer = CreateImporterWithoutSwizzleField();
			Assert.That(importer.Has_Swizzle(), Is.False);
			Assert.That(importer.Swizzle, Is.EqualTo(0));
		});
	}

	[Test]
	public void DefaultFieldSwizzleIsRRRR()
	{
		Assert.Multiple(() =>
		{
			ITextureImporter importer = CreateImporterWithSwizzleField();
			importer.GetSwizzle(out Swizzle channel0, out Swizzle channel1, out Swizzle channel2, out Swizzle channel3);
			Assert.That(channel0, Is.EqualTo(Swizzle.R));
			Assert.That(channel1, Is.EqualTo(Swizzle.R));
			Assert.That(channel2, Is.EqualTo(Swizzle.R));
			Assert.That(channel3, Is.EqualTo(Swizzle.R));
			Assert.That(importer.Swizzle, Is.EqualTo(0));//Check that GetSwizzle hasn't changed the value.
		});
	}

	[Test]
	public void SwizzleHasCorrectRGBA()
	{
		Assert.Multiple(() =>
		{
			ITextureImporter importer = CreateImporterWithSwizzleField();
			importer.SetSwizzle(Swizzle.R, Swizzle.G, Swizzle.B, Swizzle.A);
			Assert.That(importer.Swizzle, Is.EqualTo(0x_03_02_01_00));
		});
	}

	[Test]
	public void FieldlessSwizzleIsAlwaysRGBA()
	{
		Assert.Multiple(() =>
		{
			ITextureImporter importer = CreateImporterWithoutSwizzleField();
			importer.Swizzle = 534232;//arbitrary
			Assert.That(importer.Swizzle, Is.EqualTo(0));
			importer.SetSwizzle(Swizzle.One, Swizzle.OneMinusR, Swizzle.G, Swizzle.OneMinusA);//arbitrary
			importer.GetSwizzle(out Swizzle channel0, out Swizzle channel1, out Swizzle channel2, out Swizzle channel3);
			Assert.That(channel0, Is.EqualTo(Swizzle.R));
			Assert.That(channel1, Is.EqualTo(Swizzle.G));
			Assert.That(channel2, Is.EqualTo(Swizzle.B));
			Assert.That(channel3, Is.EqualTo(Swizzle.A));
		});
	}

	[TestCaseSource(nameof(GetRandomSwizzles), new object[] { 10 })]
	public void GetSwizzleMatchesSetSwizzle(Swizzle s0, Swizzle s1, Swizzle s2, Swizzle s3)
	{
		Assert.Multiple(() =>
		{
			ITextureImporter importer = CreateImporterWithSwizzleField();
			importer.SetSwizzle(s0, s1, s2, s3);
			importer.GetSwizzle(out Swizzle channel0, out Swizzle channel1, out Swizzle channel2, out Swizzle channel3);
			Assert.That(channel0, Is.EqualTo(s0));
			Assert.That(channel1, Is.EqualTo(s1));
			Assert.That(channel2, Is.EqualTo(s2));
			Assert.That(channel3, Is.EqualTo(s3));
		});
	}

	private static IEnumerable<Swizzle[]> GetRandomSwizzles(int count)
	{
		for (int i = 0; i < count; i++)
		{
			yield return new Swizzle[] { NextSwizzle(), NextSwizzle(), NextSwizzle(), NextSwizzle() };
		}

		static Swizzle NextSwizzle() => TestContext.CurrentContext.Random.NextEnum<Swizzle>();
	}

	private static ITextureImporter CreateImporterWithSwizzleField()
	{
		return AssetCreator.Create<ITextureImporter>(ClassIDType.TextureImporter, new UnityVersion(2022, 2));
	}

	private static ITextureImporter CreateImporterWithoutSwizzleField()
	{
		return AssetCreator.Create<ITextureImporter>(ClassIDType.TextureImporter, new UnityVersion(2020, 1));
	}
}
