using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Tests;

public class BundlePathTests
{
	[Test]
	public async Task DefaultBundlePathIsRoot()
	{
		BundlePath path = default;
		await Assert.That(path.IsRoot).IsTrue();
	}

	[Test]
	public async Task BundlePathParentWithDepthOneIsRoot()
	{
		BundlePath path = new([0]);
		await Assert.That(path.Parent.IsRoot).IsTrue();
	}

	[Test]
	public async Task BundlePathParentWithDepthTwoIsNotRoot()
	{
		BundlePath path = new([0, 0]);
		await Assert.That(path.Parent.IsRoot).IsFalse();
	}

	[Test]
	public async Task ParentHasCorrectPath()
	{
		BundlePath path = new([1, 2, 3]);
		BundlePath parent = path.Parent;
		await Assert.That(parent.Path.Span.SequenceEqual([1, 2])).IsTrue();
	}

	[Test]
	public async Task BundlePathsAreSequenceEqual()
	{
		BundlePath path1 = new([0, 0]);
		BundlePath path2 = new([0, 0]);
		await Assert.That(path1).IsEqualTo(path2);
	}

	[Test]
	public async Task ToStringDoesNotThrow()
	{
		// At one point, ToString could cause a StackOverflowException because it was a record,
		// and the generated PrintMembers method was calling Parent.ToString.
		// This test prevents that from ever happening again.
		await Assert.That(() =>
		{
			BundlePath bundlePath = default;
			bundlePath.ToString();
		}).ThrowsNothing();
	}
}
