using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.Primitives;

namespace AssetRipper.AssemblyDumper.Tests;

internal class VersionedListTests
{
	[Test]
	public async Task DivisionTest()
	{
		VersionedList<int> integers = new()
		{
			{ new UnityVersion(3), 3 },
			{ new UnityVersion(4), 4 },
			{ new UnityVersion(5), 5 },
			{ new UnityVersion(2017), 7 },
		};
		await Assert.That(integers.Count).IsEqualTo(4);
		await Assert.That(integers[2].Key).IsEqualTo(new UnityVersion(5));
		await Assert.That(integers[3].Key).IsEqualTo(new UnityVersion(2017));

		integers.Divide(new UnityVersion(5));
		await Assert.That(integers.Count).IsEqualTo(4);
		await Assert.That(integers[2].Key).IsEqualTo(new UnityVersion(5));
		await Assert.That(integers[3].Key).IsEqualTo(new UnityVersion(2017));

		integers.Divide(new UnityVersion(2017));
		await Assert.That(integers.Count).IsEqualTo(4);
		await Assert.That(integers[2].Key).IsEqualTo(new UnityVersion(5));
		await Assert.That(integers[3].Key).IsEqualTo(new UnityVersion(2017));

		integers.Divide(new UnityVersion(6));
		await Assert.That(integers.Count).IsEqualTo(5);
		await Assert.That(integers[2].Key).IsEqualTo(new UnityVersion(5));
		await Assert.That(integers[3].Key).IsEqualTo(new UnityVersion(6));
		await Assert.That(integers[4].Key).IsEqualTo(new UnityVersion(2017));
	}

	[Test]
	public async Task DividingAtMaxVersionDoesNotAddAnInvalidRange()
	{
		VersionedList<int> integers = new()
		{
			{ new UnityVersion(1), 1 },
		};

		integers.Divide(UnityVersion.MaxVersion);

		await Assert.That(integers).Count().IsEqualTo(1);
		await Assert.That(integers.GetRange(0).End).IsEqualTo(UnityVersion.MaxVersion);
	}

	[Test]
	public async Task MergeTest1()
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

		await Assert.That(merged.Count).IsEqualTo(6);
		await Assert.That(merged[0].Key).IsEqualTo(new UnityVersion(3));
		await Assert.That(merged[0].Value).IsEqualTo("");
		await Assert.That(merged[1].Key).IsEqualTo(new UnityVersion(4));
		await Assert.That(merged[1].Value).IsNull();
		await Assert.That(merged[2].Key).IsEqualTo(new UnityVersion(4, 5));
		await Assert.That(merged[2].Value).IsEqualTo("");
		await Assert.That(merged[3].Key).IsEqualTo(new UnityVersion(4, 7));
		await Assert.That(merged[3].Value).IsNull();
		await Assert.That(merged[4].Key).IsEqualTo(new UnityVersion(5));
		await Assert.That(merged[4].Value).IsEqualTo("Five");
		await Assert.That(merged[5].Key).IsEqualTo(new UnityVersion(2017));
		await Assert.That(merged[5].Value).IsEqualTo("Seven");
	}
}
