using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Tests.Scripts;

internal class TypeTreeScriptGeneratorTests
{
	[Test]
	public void GenerateProducesRecoverableFieldStubs()
	{
		TypeTreeNodeStruct rootNode = Node("MonoBehaviour", "Base",
			Node("int", "m_ObjectHideFlags"),
			Node("int", "rg_Internal"),
			Node("int", "<Secret>k__BackingField"),
			Node("ManagedReferencesRegistry", "references"),
			StringField("displayName"),
			ArrayField("numbers", "int"),
			MapField("lookup"),
			PPtrField("target"),
			Node("CustomStruct", "custom", Node("int", "x")),
			Node("int", "class"),
			Node("int", "field-name"),
			Node("int", "field name"));

		string script = TypeTreeScriptGenerator.Generate("My.Game", "MyType`2", rootNode);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(script, Does.Contain("namespace My.Game"));
			Assert.That(script, Does.Contain("public class MyType<T1, T2> : MonoBehaviour"));
			Assert.That(script, Does.Contain("public string displayName;"));
			Assert.That(script, Does.Contain("public List<object> numbers;"));
			Assert.That(script, Does.Contain("public Dictionary<object, object> lookup;"));
			Assert.That(script, Does.Contain("public UnityEngine.Object target;"));
			Assert.That(script, Does.Contain("public object custom;"));
			Assert.That(script, Does.Contain("public int @class;"));
			Assert.That(script, Does.Contain("public int field_name;"));
			Assert.That(script, Does.Contain("public int field_name_1;"));
			Assert.That(script, Does.Not.Contain("m_ObjectHideFlags"));
			Assert.That(script, Does.Not.Contain("rg_Internal"));
			Assert.That(script, Does.Not.Contain("<Secret>k__BackingField"));
			Assert.That(script, Does.Not.Contain("references;"));
		}
	}

	[Test]
	public void GenerateWritesFallbackCommentWhenNoRecoverableFields()
	{
		TypeTreeNodeStruct rootNode = Node("MonoBehaviour", "Base",
			Node("int", "m_Field"),
			Node("int", "rg_Field"),
			Node("int", "<Field>k__BackingField"),
			Node("ManagedReferencesRegistry", "references"));

		string script = TypeTreeScriptGenerator.Generate(null, "EmptyType", rootNode);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(script, Does.Not.Contain("namespace "));
			Assert.That(script, Does.Contain("public class EmptyType : MonoBehaviour"));
			Assert.That(script, Does.Contain("// No recoverable fields were found in the type tree."));
		}
	}

	private static TypeTreeNodeStruct StringField(string name)
	{
		return Node("string", name,
			Node("Array", "Array",
				Node("int", "size"),
				Node("char", "data")));
	}

	private static TypeTreeNodeStruct ArrayField(string name, string elementTypeName)
	{
		return Node("Array", name,
			Node("int", "size"),
			Node(elementTypeName, "data"));
	}

	private static TypeTreeNodeStruct MapField(string name)
	{
		return Node("map", name,
			Node("Array", "Array",
				Node("int", "size"),
				Node("pair", "data",
					Node("int", "first"),
					Node("int", "second"))));
	}

	private static TypeTreeNodeStruct PPtrField(string name)
	{
		return Node("PPtr<$GameObject>", name,
			Node("SInt32", "m_FileID"),
			Node("SInt64", "m_PathID"));
	}

	private static TypeTreeNodeStruct Node(string typeName, string name, params TypeTreeNodeStruct[] subNodes)
	{
		return new TypeTreeNodeStruct(typeName, name, 1, TransferMetaFlags.NoTransferFlags, subNodes);
	}
}
