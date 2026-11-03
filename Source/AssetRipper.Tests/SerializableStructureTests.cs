using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.IO.Endian;
using AssetRipper.Primitives;
using AssetRipper.SerializationLogic;

namespace AssetRipper.Tests;

internal class SerializableStructureTests
{
	[Test]
	public async Task SphericalHarmonicsL2CanBeRead()
	{
		// https://github.com/AssetRipper/AssetRipper/issues/1970
		SerializableType serializableType = new ParentType();
		await Assert.That(serializableType.Fields).Count().IsEqualTo(1);
		SerializableStructure structure = serializableType.CreateSerializableStructure();
		byte[] data = new byte[27 * sizeof(float)];
		EndianSpanReader reader = new(data, EndianType.LittleEndian);
		structure.Read(ref reader, new UnityVersion(6000), default, ITypeResolver.Null);
		await Assert.That(reader.Position).IsEqualTo(data.Length);
	}

	private class ParentType : SerializableType
	{
		public ParentType() : base("Namespace", PrimitiveType.Complex, "Name")
		{
			Fields = [new Field(new SphericalHarmonicsType(), 0, "field", false)];
			MaxDepth = 1;
		}
	}

	private class SphericalHarmonicsType : SerializableType
	{
		public SphericalHarmonicsType() : base("UnityEngine.Rendering", PrimitiveType.Complex, "SphericalHarmonicsL2")
		{
			// Even though its fields are serialized, they don't show up in our algorithm as they are private and have no SerializeField attribute.
			MaxDepth = 0;
		}
	}
}