using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.Primitives;

namespace AssetRipper.AssemblyDumper.Tests;

internal class VersionedListTests
{
	[Test]
	public void DivisionTest()
	{
		VersionedList<int> integers = new()
		{
			{ new UnityVersion(3), 3 },
			{ new UnityVersion(4), 4 },
			{ new UnityVersion(5), 5 },
			{ new UnityVersion(2017), 7 },
		};
		Assert.That(integers.Count, Is.EqualTo(4));
		Assert.That(integers[2].Key, Is.EqualTo(new UnityVersion(5)));
		Assert.That(integers[3].Key, Is.EqualTo(new UnityVersion(2017)));

		integers.Divide(new UnityVersion(5));
		Assert.That(integers.Count, Is.EqualTo(4));
		Assert.That(integers[2].Key, Is.EqualTo(new UnityVersion(5)));
		Assert.That(integers[3].Key, Is.EqualTo(new UnityVersion(2017)));

		integers.Divide(new UnityVersion(2017));
		Assert.That(integers.Count, Is.EqualTo(4));
		Assert.That(integers[2].Key, Is.EqualTo(new UnityVersion(5)));
		Assert.That(integers[3].Key, Is.EqualTo(new UnityVersion(2017)));

		integers.Divide(new UnityVersion(6));
		Assert.That(integers.Count, Is.EqualTo(5));
		Assert.That(integers[2].Key, Is.EqualTo(new UnityVersion(5)));
		Assert.That(integers[3].Key, Is.EqualTo(new UnityVersion(6)));
		Assert.That(integers[4].Key, Is.EqualTo(new UnityVersion(2017)));
	}

	[Test]
	public void MergeTest1()
	{
		VersionedList<string> list1 = new()
		{
			{ new UnityVersion(3), "" },
			{ new UnityVersion(4), null },
			{ new UnityVersion(5), "Five" },
			{ new UnityVersion(2017), "Seven" },
		};
		VersionedList<string> list2 = new()
		{
			{ new UnityVersion(3, 5), "" },
			{ new UnityVersion(4), null },
			{ new UnityVersion(4, 5), "" },
			{ new UnityVersion(4, 7), null },
			{ new UnityVersion(2017), "Seven" },
		};

		VersionedList<string> merged = VersionedList.Merge(list1, list2);

		Assert.That(merged.Count, Is.EqualTo(6));
		Assert.That(merged[0].Key, Is.EqualTo(new UnityVersion(3)));
		Assert.That(merged[0].Value, Is.EqualTo(""));
		Assert.That(merged[1].Key, Is.EqualTo(new UnityVersion(4)));
		Assert.That(merged[1].Value, Is.EqualTo(null));
		Assert.That(merged[2].Key, Is.EqualTo(new UnityVersion(4, 5)));
		Assert.That(merged[2].Value, Is.EqualTo(""));
		Assert.That(merged[3].Key, Is.EqualTo(new UnityVersion(4, 7)));
		Assert.That(merged[3].Value, Is.EqualTo(null));
		Assert.That(merged[4].Key, Is.EqualTo(new UnityVersion(5)));
		Assert.That(merged[4].Value, Is.EqualTo("Five"));
		Assert.That(merged[5].Key, Is.EqualTo(new UnityVersion(2017)));
		Assert.That(merged[5].Value, Is.EqualTo("Seven"));
	}
}
