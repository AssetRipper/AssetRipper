using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Tests;

public class BundlePathTests
{
	[Test]
	public void DefaultBundlePathIsRoot()
	{
		BundlePath path = default;
		Assert.That(path.IsRoot);
	}

	[Test]
	public void BundlePathParentWithDepthOneIsRoot()
	{
		BundlePath path = new([0]);
		Assert.That(path.Parent.IsRoot);
	}

	[Test]
	public void BundlePathParentWithDepthTwoIsNotRoot()
	{
		BundlePath path = new([0, 0]);
		Assert.That(path.Parent.IsRoot, Is.False);
	}

	[Test]
	public void ParentHasCorrectPath()
	{
		BundlePath path = new([1, 2, 3]);
		BundlePath parent = path.Parent;
		Assert.That(parent.Path.ToArray(), Is.EquivalentTo((int[])[1, 2]));
	}

	[Test]
	public void BundlePathsAreSequenceEqual()
	{
		BundlePath path1 = new([0, 0]);
		BundlePath path2 = new([0, 0]);
		Assert.That(path1, Is.EqualTo(path2));
	}

	[Test]
	public void ToStringDoesNotThrow()
	{
		// At one point, ToString could cause a StackOverflowException because it was a record,
		// and the generated PrintMembers method was calling Parent.ToString.
		// This test prevents that from ever happening again.
		Assert.DoesNotThrow(() =>
		{
			BundlePath bundlePath = default;
			bundlePath.ToString();
		});
	}
}
