using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.IO.Endian;
using AssetRipper.Primitives;
using AssetRipper.SerializationLogic;
using AssetRipper.SourceGenerated.Extensions;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.Tests;

public class ManagedReferenceRegistryTests
{
	[TestCase("Outer/Inner")]
	[TestCase("Outer+Inner")]
	public void NestedScriptNamesArePreserved(string name)
	{
		using TestManager manager = new();
		ModuleDefinition module = manager.GetAssemblies().Single().ManifestModule!;
		TypeDefinition outer = new("Tests", "Outer", TypeAttributes.Public);
		module.TopLevelTypes.Add(outer);
		TypeDefinition inner = new("", "Inner", TypeAttributes.NestedPublic);
		outer.NestedTypes.Add(inner);
		var id = manager.GetScriptID("Fixture", "Tests", name);
		Assert.That(id.Name, Is.EqualTo(name));
		Assert.That(manager.GetTypeDefinition(id), Is.SameAs(inner));
	}

	[Test]
	public void EngineFacadeForwardersResolveToModuleTypes()
	{
		using TestManager manager = new();
		AssemblyDefinition implementation = manager.GetAssemblies().Single();
		ModuleDefinition facadeModule = new("Facade");
		AssemblyDefinition facade = new("Facade", new Version(1, 0));
		facade.Modules.Add(facadeModule);
		AssemblyReference reference = new("Fixture", implementation.Version);
		facadeModule.AssemblyReferences.Add(reference);
		facadeModule.ExportedTypes.Add(new(reference, "Tests", "Example") { Attributes = TypeAttributes.Public | TypeAttributes.Forwarder });
		manager.Add(facade);
		var id = manager.GetScriptID("Facade", "Tests", "Example");
		Assert.That(id.IsDefault, Is.False);
		Assert.That(manager.GetTypeDefinition(id), Is.SameAs(implementation.ManifestModule!.TopLevelTypes.Single(t => t.Name == "Example")));
	}

	[Test]
	public void ClosedGenericReferenceTypesResolveFromAssemblies()
	{
		using TestManager manager = new();
		ModuleDefinition module = manager.GetAssemblies().Single().ManifestModule!;
		TypeDefinition box = new("Tests", "Box`1", TypeAttributes.Public | TypeAttributes.Serializable);
		module.TopLevelTypes.Add(box);
		box.GenericParameters.Add(new GenericParameter("T"));
		box.Fields.Add(new("item", FieldAttributes.Public, new GenericParameterSignature(module, GenericParameterType.Type, 0)));
		var id = new ScriptIdentifier("Fixture", "Tests", "Box`1[[Tests.Example, Fixture]]");
		Assert.That(manager.TryGetSerializableType(id, UnityVersion.Parse("6000.5.10f1"), out var type, out var reason), Is.True, reason);
		Assert.That(type!.Fields.Single().Type.Fields.Select(f => f.Name), Is.EqualTo(new[] { "value", "label" }));
	}

	[Test]
	public void EmptyReferenceArraysCanOmitTheTrailingRegistry()
	{
		var collection = AssetCreator.CreateCollection(UnityVersion.Parse("6000.5.10f1"));
		SerializableStructure host = new RegistryHostType(includeArray: true).CreateSerializableStructure();
		byte[] bytes = new byte[4];
		EndianSpanReader reader = new(bytes, collection.EndianType);
		host.Read(ref reader, collection.Version, collection.Flags, ITypeResolver.Null);
		Assert.That(reader.Position, Is.EqualTo(bytes.Length));
		Assert.That(host["references"].CValue, Is.Null);
		Assert.That(host.FetchDependencies(), Is.Empty);
		using MemoryStream output = new();
		using AssetWriter writer = new(output, collection);
		host.DeepClone(new PPtrConverter(collection, collection)).WriteRelease(writer);
		Assert.That(output.ToArray(), Is.EqualTo(bytes));
	}

	[Test]
	public void RegistryPreservesIdentityValuesAndBinaryLayout()
	{
		ProcessedAssetCollection collection = AssetCreator.CreateCollection(UnityVersion.Parse("6000.5.10f1"));
		using TestManager manager = new();
		byte[] bytes;
		using (MemoryStream stream = new())
		{
			using EndianWriter writer = new(stream, EndianType.LittleEndian);
			writer.Write(2);
			writer.Write(2);
			writer.Write(-2L);
			WriteString(writer, ""); WriteString(writer, ""); WriteString(writer, "");
			writer.Write(1000L);
			WriteString(writer, "Example"); WriteString(writer, "Tests"); WriteString(writer, "Fixture");
			writer.Write(73);
			WriteString(writer, "hello");
			bytes = stream.ToArray();
		}
		EndianSpanReader reader = new(bytes, EndianType.LittleEndian);
		SerializableStructure host = new RegistryHostType().CreateSerializableStructure();
		host.Read(ref reader, collection.Version, collection.Flags, manager);
		SerializableStructure registry = (SerializableStructure)host["references"].AsAsset;
		var entries = registry["RefIds"].AsAssetArray.Cast<SerializableStructure>().ToArray();
		Assert.That(reader.Position, Is.EqualTo(bytes.Length));
		Assert.That(entries[0]["rid"].AsInt64, Is.EqualTo(-2));
		Assert.That(((SerializableStructure)entries[0]["data"].AsAsset).Fields, Is.Empty);
		Assert.That(entries[1]["rid"].AsInt64, Is.EqualTo(1000));
		var data = (SerializableStructure)entries[1]["data"].AsAsset;
		Assert.That(data["value"].AsInt32, Is.EqualTo(73));
		Assert.That(data["label"].AsString, Is.EqualTo("hello"));
		using MemoryStream output = new();
		using AssetWriter assetWriter = new(output, collection);
		registry.WriteRelease(assetWriter);
		Assert.That(output.ToArray(), Is.EqualTo(bytes));
		SerializableStructure clone = registry.DeepClone(new PPtrConverter(collection, collection));
		var clonedEntry = (SerializableStructure)clone["RefIds"].AsAssetArray[1];
		var clonedData = (SerializableStructure)clonedEntry["data"].AsAsset;
		Assert.That(clonedData, Is.Not.SameAs(data));
		Assert.That(clonedData["value"].AsInt32, Is.EqualTo(73));
	}

	[TestCase(false)]
	[TestCase(true)]
	public void ResetClearsTheManagedReferenceArrayAndRegistry(bool copyFromNull)
	{
		var collection = AssetCreator.CreateCollection(UnityVersion.Parse("6000.5.10f1"));
		byte[] bytes;
		using (MemoryStream stream = new())
		{
			using EndianWriter writer = new(stream, collection.EndianType);
			writer.Write(1);
			writer.Write(-2L);
			writer.Write(2);
			writer.Write(1);
			writer.Write(-2L);
			WriteString(writer, ""); WriteString(writer, ""); WriteString(writer, "");
			bytes = stream.ToArray();
		}
		SerializableStructure host = new RegistryHostType(includeArray: true).CreateSerializableStructure();
		EndianSpanReader reader = new(bytes, collection.EndianType);
		host.Read(ref reader, collection.Version, collection.Flags, ITypeResolver.Null);
		Assert.That(reader.Position, Is.EqualTo(bytes.Length));
		Assert.That(host["items"].AsAssetArray, Has.Length.EqualTo(1));
		Assert.That(((SerializableStructure)host["references"].AsAsset)["RefIds"].AsAssetArray, Has.Length.EqualTo(1));
		if (copyFromNull)
		{
			host.CopyValues(null, new PPtrConverter(collection, collection));
		}
		else
		{
			host.Reset();
		}
		Assert.That(host["items"].AsAssetArray, Is.Empty);
		Assert.That(host["references"].CValue, Is.Null);
		Assert.That(host.FetchDependencies(), Is.Empty);
		using MemoryStream output = new();
		using AssetWriter assetWriter = new(output, collection);
		host.WriteRelease(assetWriter);
		Assert.That(output.ToArray(), Is.EqualTo(new byte[4]));
	}

	[TestCase(-1, typeof(InvalidDataException))]
	[TestCase(int.MaxValue, typeof(EndOfStreamException))]
	public void InvalidReferenceCountsAreRejected(int count, Type exceptionType)
	{
		byte[] bytes = new byte[8];
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes, 2);
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), count);
		var collection = AssetCreator.CreateCollection(UnityVersion.Parse("6000.5.10f1"));
		Assert.That(() =>
		{
			EndianSpanReader reader = new(bytes, EndianType.LittleEndian);
			SerializableStructure host = new RegistryHostType().CreateSerializableStructure();
			host.Read(ref reader, collection.Version, collection.Flags, ITypeResolver.Null);
		}, Throws.TypeOf(exceptionType));
	}

	private static void WriteString(EndianWriter writer, string value)
	{
		writer.Write((Utf8String)value);
		writer.AlignStream();
	}

	private sealed class RegistryHostType : SerializableType
	{
		public RegistryHostType(bool includeArray = false) : base(null, PrimitiveType.Complex, "Host")
		{
			Fields = includeArray
				? [new(ManagedReferenceTypes.ManagedReference, 1, "items", true), ManagedReferenceTypes.RegistryField]
				: [ManagedReferenceTypes.RegistryField];
			MaxDepth = ManagedReferenceTypes.Registry.MaxDepth + 1;
		}
	}

	private sealed class TestManager : BaseManager
	{
		public TestManager() : base(_ => { })
		{
			ModuleDefinition module = new("Fixture");
			AssemblyDefinition assembly = new("Fixture", new Version(1, 0));
			assembly.Modules.Add(module);
			RuntimeContext context = new(DotNetRuntimeInfo.NetCoreApp(10, 0), (bool?)null, (AssemblyReference)module.CorLibTypeFactory.CorLibScope);
			context.AddAssembly(assembly);
			TypeDefinition type = new("Tests", "Example", TypeAttributes.Public | TypeAttributes.Serializable);
			module.TopLevelTypes.Add(type);
			type.Fields.Add(new("value", FieldAttributes.Public, module.CorLibTypeFactory.Int32));
			type.Fields.Add(new("label", FieldAttributes.Public, module.CorLibTypeFactory.String));
			Add(assembly);
		}
		public override ScriptingBackend ScriptingBackend => ScriptingBackend.Mono;
	}
}
