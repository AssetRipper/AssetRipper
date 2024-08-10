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
		BundlePath path = new([0,0]);
		Assert.That(path.Parent.IsRoot, Is.False);
	}

	[Test]
	public void ParentHasCorrectPath()
	{
		BundlePath path = new([1, 2, 3]);
		BundlePath parent = path.Parent;
		Assert.That(parent.Path.ToArray(), Is.EquivalentTo((int[])[1, 2]));
	}
}
