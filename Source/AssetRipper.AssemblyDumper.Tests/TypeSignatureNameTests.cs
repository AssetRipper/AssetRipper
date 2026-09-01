using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using System.Text;

namespace AssetRipper.AssemblyDumper.Tests;

internal class TypeSignatureNameTests
{
	private ModuleDefinition module;
	private ReferenceImporter importer;

	[SetUp]
	public void Setup()
	{
		AssemblyReference corlib = KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0;
		AssemblyDefinition assembly = new AssemblyDefinition("test", new Version(1, 0, 0, 0));
		module = new ModuleDefinition("test", corlib);
		assembly.Modules.Add(module);
		RuntimeContext runtimeContext = new(DotNetRuntimeInfo.NetCoreApp(corlib.Version), (bool?)null, corlib);
		runtimeContext.AddAssembly(assembly);
		importer = module.DefaultImporter;
	}

	[Test]
	public void AsmByteArrayName()
	{
		TypeSignature type = module.CorLibTypeFactory.Byte.MakeSzArrayType();
		Assert.That(type.Name, Is.EqualTo("Byte[]"));
	}

	[Test]
	public void AsmListTest()
	{
		TypeSignature list = importer.ImportTypeSignature(typeof(List<>));
		GenericInstanceTypeSignature stringList = list.MakeGenericInstanceType(module.RuntimeContext, module.CorLibTypeFactory.String);
		Assert.That(stringList.Name, Is.EqualTo("List`1<System.String>"));
		Assert.That(list.Name, Is.EqualTo("List`1"));
	}

	[Test]
	public void ByteArrayTest()
	{
		TypeSignature type = module.CorLibTypeFactory.Byte.MakeSzArrayType();
		Assert.That(GetName(type), Is.EqualTo("Byte_Array"));
	}

	[Test]
	public void DictionaryTest()
	{
		TypeSignature dictionary = importer.ImportTypeSignature(typeof(Dictionary<,>));
		Assert.That(GetName(dictionary), Is.EqualTo("Dictionary"));
	}

	[Test]
	public void DictionaryInstanceTest()
	{
		TypeSignature dictionary = importer.ImportTypeSignature(typeof(Dictionary<,>));
		GenericInstanceTypeSignature intStringDictionary = dictionary.MakeGenericInstanceType(module.RuntimeContext, module.CorLibTypeFactory.Int32, module.CorLibTypeFactory.String);
		Assert.That(GetName(intStringDictionary), Is.EqualTo("Dictionary_Int32_String"));
	}

	[Test]
	public void ListInstanceTest()
	{
		TypeSignature list = importer.ImportTypeSignature(typeof(List<>));
		GenericInstanceTypeSignature stringList = list.MakeGenericInstanceType(module.RuntimeContext, module.CorLibTypeFactory.String);
		Assert.That(GetName(stringList), Is.EqualTo("List_String"));
	}

	[Test]
	public void ListListInstanceTest()
	{
		TypeSignature list = importer.ImportTypeSignature(typeof(List<>));
		GenericInstanceTypeSignature stringList = list.MakeGenericInstanceType(module.RuntimeContext, module.CorLibTypeFactory.String);
		GenericInstanceTypeSignature stringListList = list.MakeGenericInstanceType(module.RuntimeContext, stringList);
		Assert.That(GetName(stringListList), Is.EqualTo("List_List_String"));
	}

	private string GetName(TypeSignature type)
	{
		if (type is CorLibTypeSignature)
		{
			return type.Name ?? "";
		}
		else if (type is TypeDefOrRefSignature normalType)
		{
			string asmName = normalType.Name;
			int index = asmName.IndexOf('`');
			return index > -1 ? asmName.Substring(0, index) : asmName;
		}
		else if (type is SzArrayTypeSignature arrayType)
		{
			return $"{GetName(arrayType.BaseType)}_Array";
		}
		else if (type is GenericInstanceTypeSignature genericInstanceType)
		{
			string baseTypeName = GetName(genericInstanceType.GenericType.ToTypeSignature(module.RuntimeContext));
			StringBuilder sb = new StringBuilder();
			sb.Append(baseTypeName);
			foreach (TypeSignature typeArgument in genericInstanceType.TypeArguments)
			{
				sb.Append('_');
				sb.Append(GetName(typeArgument));
			}
			return sb.ToString();
		}
		else
		{
			throw new NotSupportedException($"GetName not support for {type.FullName} of type {type.GetType()}");
		}
	}
}
