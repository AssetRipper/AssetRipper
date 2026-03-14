using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.Yaml;
using System.Globalization;

namespace AssetRipper.Tests.Traversal;

internal class YamlWalkerManagedReferenceTests
{
	[Test]
	public void MonoBehaviourManagedReferencesAreAppendedAndSorted()
	{
		MonoBehaviour_2017_3 monoBehaviour = AssetCreator.CreateUnsafe<MonoBehaviour_2017_3>();
		SerializableStructure structure = CreateMainStructure();
		structure["value"].AsInt32 = 7;

		SerializableStructure data20 = CreateDataStructure();
		data20["innerValue"].AsInt32 = 84;
		structure.ManagedReferences[20] = data20;
		structure.ManagedReferenceTypes[20] = new SerializableStructure.ManagedReferenceTypeInfo("SecondType", "Game.Data", "Assembly-CSharp");

		SerializableStructure data10 = CreateDataStructure();
		data10["innerValue"].AsInt32 = 42;
		structure.ManagedReferences[10] = data10;
		structure.ManagedReferenceTypes[10] = new SerializableStructure.ManagedReferenceTypeInfo("FirstType", "Game.Data", "Assembly-CSharp");

		monoBehaviour.Structure = structure;

		string yaml = GenerateYaml(monoBehaviour);
		int firstRidIndex = yaml.IndexOf("rid: 10", StringComparison.Ordinal);
		int secondRidIndex = yaml.IndexOf("rid: 20", StringComparison.Ordinal);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(yaml, Does.Contain("value: 7"));
			Assert.That(yaml, Does.Contain("references:"));
			Assert.That(yaml, Does.Contain("type: {class: FirstType, ns: Game.Data, asm: Assembly-CSharp}"));
			Assert.That(yaml, Does.Contain("type: {class: SecondType, ns: Game.Data, asm: Assembly-CSharp}"));
			Assert.That(yaml, Does.Contain("innerValue: 42"));
			Assert.That(yaml, Does.Contain("innerValue: 84"));
			Assert.That(firstRidIndex, Is.GreaterThanOrEqualTo(0));
			Assert.That(secondRidIndex, Is.GreaterThan(firstRidIndex));
		}
	}

	private static SerializableStructure CreateMainStructure()
	{
		TypeTreeNodeStruct rootNode = Node("MonoBehaviour", "Base",
			Node("int", "value"),
			Node("ManagedReferencesRegistry", "references"));
		return SerializableTreeType.FromRootNode(rootNode, true).CreateSerializableStructure();
	}

	private static SerializableStructure CreateDataStructure()
	{
		TypeTreeNodeStruct rootNode = Node("ReferencedObjectData", "data",
			Node("int", "innerValue"));
		return SerializableTreeType.FromRootNode(rootNode).CreateSerializableStructure();
	}

	private static TypeTreeNodeStruct Node(string typeName, string name, params TypeTreeNodeStruct[] subNodes)
	{
		return new TypeTreeNodeStruct(typeName, name, 1, TransferMetaFlags.NoTransferFlags, subNodes);
	}

	private static string GenerateYaml(IUnityObjectBase asset)
	{
		using StringWriter stringWriter = new(CultureInfo.InvariantCulture) { NewLine = "\n" };
		YamlWriter writer = new();
		writer.WriteHead(stringWriter);
		YamlDocument document = new YamlWalker().ExportYamlDocument(asset, 1);
		writer.WriteDocument(document);
		writer.WriteTail(stringWriter);
		return stringWriter.ToString();
	}
}
