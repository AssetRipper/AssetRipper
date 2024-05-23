using AssetRipper.Export.UnityProjects;
using AssetRipper.HashAlgorithms;
using AssetRipper.IO.Files;
using AssetRipper.Primitives;
using System.Buffers.Binary;
using System.Diagnostics;

namespace AssetRipper.Tests;

internal class PathIDCalculationTests
{
	[Test]
	public void BinocularsOverlay()
	{
		const long ExpectedPathID = -3447896943880403800;
		UnityGuid guid = UnityGuid.Parse("01e291bf376af4b4994f5015f73d2fe0");
		AssetType type = AssetType.Meta;
		long fileID = ExportIdHandler.GetMainExportID(28);
		Assert.That(Compute64BitHash(guid, type, fileID), Is.EqualTo(ExpectedPathID));
	}

	private static long Compute64BitHash(UnityGuid guid, AssetType type, long fileID)
	{
		const int GuidStringLength = 32;
		Span<byte> input = stackalloc byte[GuidStringLength + sizeof(AssetType) + sizeof(long)];
		string guidString = guid.ToString();
		Debug.Assert(guidString.Length == GuidStringLength);
		for (int i = 0; i < GuidStringLength; i++)
		{
			input[i] = (byte)guidString[i];
		}
		BinaryPrimitives.WriteInt32LittleEndian(input.Slice(GuidStringLength), (int)type);
		BinaryPrimitives.WriteInt64LittleEndian(input.Slice(GuidStringLength + sizeof(AssetType)), fileID);

		Span<byte> output = stackalloc byte[16];
		MD4.HashData(input, output);

		return BinaryPrimitives.ReadInt64LittleEndian(output);
	}
}
