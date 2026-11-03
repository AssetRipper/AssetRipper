using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.Tests;

public class SmartStreamTests
{
	[Test]
	public async Task StreamsHaveTheCorrectStreamType()
	{

		{
			await Assert.That(SmartStream.CreateNull().StreamType).IsEqualTo(SmartStreamType.Null);
			await Assert.That(SmartStream.CreateTemp().StreamType).IsEqualTo(SmartStreamType.File);
			await Assert.That(SmartStream.CreateMemory().StreamType).IsEqualTo(SmartStreamType.Memory);
		}
	}

	[Test]
	public async Task MemoryStreamMaintainTheirLength()
	{
		const int Length = 64;
		SmartStream memoryStream = SmartStream.CreateMemory(new byte[Length]);
		await Assert.That(memoryStream.Length).IsEqualTo(Length);
	}

	[Test]
	public async Task RandomAccessPartialStreamSeeksRelativeToEnd()
	{
		using SmartStream baseStream = SmartStream.CreateTemp();
		baseStream.Write([1, 2, 3, 4]);
		using SmartStream partialStream = baseStream.CreatePartial(1, 2);

		await Assert.That(partialStream.Seek(-1, SeekOrigin.End)).IsEqualTo(1);
		await Assert.That(partialStream.ReadByte()).IsEqualTo(3);
		await Assert.That(partialStream.Seek(1, SeekOrigin.End)).IsEqualTo(3);
		await Assert.That(partialStream.Read(new byte[1])).IsZero();
	}

	[Test]
	public async Task ToArrayMakesAPerfectCopyForMemorySmartStreams()
	{
		const int Length = 87;
		byte[] randomData = new Random(42).NextBytes(Length);
		SmartStream memoryStream = SmartStream.CreateMemory(randomData);
		await Assert.That(memoryStream.ToArray().SequenceEqual(randomData)).IsTrue();
	}

	[Test]
	public async Task FreedStreamsMustBeNull()
	{
		SmartStream stream = SmartStream.CreateMemory();

		{
			await Assert.That(stream.RefCount).IsEqualTo(1);
			await Assert.That(stream.IsNull).IsFalse();
			await Assert.That(stream.StreamType).IsEqualTo(SmartStreamType.Memory);
		}
		stream.FreeReference();

		{
			await Assert.That(stream.RefCount).IsZero();
			await Assert.That(stream.IsNull).IsTrue();
			await Assert.That(stream.StreamType).IsEqualTo(SmartStreamType.Null);
		}
	}

	[Test]
	public async Task NullStreamIsNullAndHasRefCountZero()
	{

		{
			await Assert.That(SmartStream.CreateNull().RefCount).IsZero();
			await Assert.That(SmartStream.CreateNull().IsNull).IsTrue();
		}
	}

	[Test]
	public async Task DisposedStreamThrowsForManyMembers()
	{

		{
			await Assert.That(() => SmartStream.CreateNull().Flush()).ThrowsNothing();
			await Assert.That(() => _ = SmartStream.CreateNull().CanRead).ThrowsNothing();
			await Assert.That(() => _ = SmartStream.CreateNull().CanSeek).ThrowsNothing();
			await Assert.That(() => _ = SmartStream.CreateNull().CanWrite).ThrowsNothing();
			await Assert.That(() => _ = SmartStream.CreateNull().Position).ThrowsNothing();
			await Assert.That(() => _ = SmartStream.CreateNull().Length).ThrowsNothing();
			await Assert.That(() => _ = SmartStream.CreateNull().RefCount).ThrowsNothing();
			await Assert.That(() => _ = SmartStream.CreateNull().IsNull).ThrowsNothing();
			await Assert.That(() => _ = SmartStream.CreateNull().StreamType).ThrowsNothing();

			//Count should match up with the number of references to ThrowIfNull
			await Assert.ThrowsAsync<NullReferenceException>(() => Task.Run(() => SmartStream.CreateNull().ReadExactly(new byte[2], 1, 1)));
			await Assert.ThrowsAsync<NullReferenceException>(() => Task.Run(() => SmartStream.CreateNull().ReadExactly(new byte[2])));
			await Assert.ThrowsAsync<NullReferenceException>(() => Task.Run(() => SmartStream.CreateNull().ReadByte()));
			await Assert.ThrowsAsync<NullReferenceException>(() => Task.Run(() => SmartStream.CreateNull().Seek(0, SeekOrigin.Begin)));
			await Assert.ThrowsAsync<NullReferenceException>(() => Task.Run(() => SmartStream.CreateNull().SetLength(2)));
			await Assert.ThrowsAsync<NullReferenceException>(() => Task.Run(() => SmartStream.CreateNull().Write(new byte[2], 1, 1)));
			await Assert.ThrowsAsync<NullReferenceException>(() => Task.Run(() => SmartStream.CreateNull().Position = default));
			await Assert.ThrowsAsync<NullReferenceException>(() => Task.Run(() => SmartStream.CreateNull().ToArray()));
		}
	}
}
