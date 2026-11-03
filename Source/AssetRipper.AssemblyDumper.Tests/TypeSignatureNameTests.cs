using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using System.Text;

namespace AssetRipper.AssemblyDumper.Tests;

internal class TypeSignatureNameTests
{
	private readonly ModuleDefinition module;
	private readonly ReferenceImporter importer;

	public TypeSignatureNameTests()
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
	public async Task AsmByteArrayName()
	{
		TypeSignature type = module.CorLibTypeFactory.Byte.MakeSzArrayType();
		await Assert.That(type.Name).IsEqualTo("Byte[]");
	}

	[Test]
	public async Task AsmListTest()
	{
		TypeSignature list = importer.ImportTypeSignature(typeof(List<>));
		GenericInstanceTypeSignature stringList = list.MakeGenericInstanceType(module.RuntimeContext, [module.CorLibTypeFactory.String]);
		await Assert.That(stringList.Name).IsEqualTo("List`1<System.String>");
		await Assert.That(list.Name).IsEqualTo("List`1");
	}

	[Test]
	public async Task ByteArrayTest()
	{
		TypeSignature type = module.CorLibTypeFactory.Byte.MakeSzArrayType();
		await Assert.That(GetName(type)).IsEqualTo("Byte_Array");
	}

	[Test]
	public async Task DictionaryTest()
	{
		TypeSignature dictionary = importer.ImportTypeSignature(typeof(Dictionary<,>));
		await Assert.That(GetName(dictionary)).IsEqualTo("Dictionary");
	}

	[Test]
	public async Task DictionaryInstanceTest()
	{
		TypeSignature dictionary = importer.ImportTypeSignature(typeof(Dictionary<,>));
		GenericInstanceTypeSignature intStringDictionary = dictionary.MakeGenericInstanceType(module.RuntimeContext, [module.CorLibTypeFactory.Int32, module.CorLibTypeFactory.String]);
		await Assert.That(GetName(intStringDictionary)).IsEqualTo("Dictionary_Int32_String");
	}

	[Test]
	public async Task ListInstanceTest()
	{
		TypeSignature list = importer.ImportTypeSignature(typeof(List<>));
		GenericInstanceTypeSignature stringList = list.MakeGenericInstanceType(module.RuntimeContext, [module.CorLibTypeFactory.String]);
		await Assert.That(GetName(stringList)).IsEqualTo("List_String");
	}

	[Test]
	public async Task ListListInstanceTest()
	{
		TypeSignature list = importer.ImportTypeSignature(typeof(List<>));
		GenericInstanceTypeSignature stringList = list.MakeGenericInstanceType(module.RuntimeContext, [module.CorLibTypeFactory.String]);
		GenericInstanceTypeSignature stringListList = list.MakeGenericInstanceType(module.RuntimeContext, [stringList]);
		await Assert.That(GetName(stringListList)).IsEqualTo("List_List_String");
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
