using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.Tests;

public class SmartStreamTests
{
	[Test]
	public void StreamsHaveTheCorrectStreamType()
	{
		Assert.Multiple(() =>
		{
			Assert.That(SmartStream.CreateNull().StreamType, Is.EqualTo(SmartStreamType.Null));
			Assert.That(SmartStream.CreateTemp().StreamType, Is.EqualTo(SmartStreamType.File));
			Assert.That(SmartStream.CreateMemory().StreamType, Is.EqualTo(SmartStreamType.Memory));
		});
	}

	[Test]
	public void MemoryStreamMaintainTheirLength()
	{
		const int Length = 64;
		SmartStream memoryStream = SmartStream.CreateMemory(new byte[Length]);
		Assert.That(memoryStream.Length, Is.EqualTo(Length));
	}

	[Test]
	public void ToArrayMakesAPerfectCopyForMemorySmartStreams()
	{
		const int Length = 87;
		byte[] randomData = new byte[Length];
		new Random(32).NextBytes(randomData);
		SmartStream memoryStream = SmartStream.CreateMemory(randomData);
		Assert.That(memoryStream.ToArray(), Is.EqualTo(randomData));
	}

	[Test]
	public void FreedStreamsMustBeNull()
	{
		SmartStream stream = SmartStream.CreateMemory();
		Assert.Multiple(() =>
		{
			Assert.That(stream.RefCount, Is.EqualTo(1));
			Assert.That(stream.IsNull, Is.False);
			Assert.That(stream.StreamType, Is.EqualTo(SmartStreamType.Memory));
		});
		stream.FreeReference();
		Assert.Multiple(() =>
		{
			Assert.That(stream.RefCount, Is.EqualTo(0));
			Assert.That(stream.IsNull, Is.True);
			Assert.That(stream.StreamType, Is.EqualTo(SmartStreamType.Null));
		});
	}

	[Test]
	public void NullStreamIsNullAndHasRefCountZero()
	{
		Assert.Multiple(() =>
		{
			Assert.That(SmartStream.CreateNull().RefCount, Is.EqualTo(0));
			Assert.That(SmartStream.CreateNull().IsNull);
		});
	}

	[Test]
	public void DisposedStreamThrowsForManyMembers()
	{
		Assert.Multiple(() =>
		{
			Assert.DoesNotThrow(() => SmartStream.CreateNull().Flush());
			Assert.DoesNotThrow(() => _ = SmartStream.CreateNull().CanRead);
			Assert.DoesNotThrow(() => _ = SmartStream.CreateNull().CanSeek);
			Assert.DoesNotThrow(() => _ = SmartStream.CreateNull().CanWrite);
			Assert.DoesNotThrow(() => _ = SmartStream.CreateNull().Position);
			Assert.DoesNotThrow(() => _ = SmartStream.CreateNull().Length);
			Assert.DoesNotThrow(() => _ = SmartStream.CreateNull().RefCount);
			Assert.DoesNotThrow(() => _ = SmartStream.CreateNull().IsNull);
			Assert.DoesNotThrow(() => _ = SmartStream.CreateNull().StreamType);

			//Count should match up with the number of references to ThrowIfNull
			Assert.Throws<NullReferenceException>(() => SmartStream.CreateNull().Read(new byte[2], 1, 1));
			Assert.Throws<NullReferenceException>(() => SmartStream.CreateNull().Read(new byte[2]));
			Assert.Throws<NullReferenceException>(() => SmartStream.CreateNull().ReadByte());
			Assert.Throws<NullReferenceException>(() => SmartStream.CreateNull().Seek(0, SeekOrigin.Begin));
			Assert.Throws<NullReferenceException>(() => SmartStream.CreateNull().SetLength(2));
			Assert.Throws<NullReferenceException>(() => SmartStream.CreateNull().Write(new byte[2], 1, 1));
			Assert.Throws<NullReferenceException>(() => SmartStream.CreateNull().Position = default);
			Assert.Throws<NullReferenceException>(() => SmartStream.CreateNull().ToArray());
		});
	}
}
