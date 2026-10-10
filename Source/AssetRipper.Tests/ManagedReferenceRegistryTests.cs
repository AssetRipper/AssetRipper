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
	public void RegistryPreservesIdentityValuesAndBinaryLayout()
	{
		ProcessedAssetCollection collection = AssetCreator.CreateCollection(UnityVersion.Parse("6000.5.10f1"));
		TestManager manager = new();
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
		ManagedReferenceRegistry registry = ManagedReferenceRegistry.Read(ref reader, collection, manager);
		Assert.That(reader.Position, Is.EqualTo(bytes.Length));
		Assert.That(registry.Entries[0].Rid, Is.EqualTo(-2));
		Assert.That(registry.Entries[0].Data, Is.Null);
		Assert.That(registry.Entries[1].Rid, Is.EqualTo(1000));
		Assert.That(registry.Entries[1].Data!["value"].AsInt32, Is.EqualTo(73));
		Assert.That(registry.Entries[1].Data!["label"].AsString, Is.EqualTo("hello"));
		using MemoryStream output = new();
		using AssetWriter assetWriter = new(output, collection);
		registry.WriteRelease(assetWriter);
		Assert.That(output.ToArray(), Is.EqualTo(bytes));
		ManagedReferenceRegistry clone = registry.DeepClone(new PPtrConverter(collection, collection));
		Assert.That(clone.Entries[1].Data, Is.Not.SameAs(registry.Entries[1].Data));
		Assert.That(clone.Entries[1].Data!["value"].AsInt32, Is.EqualTo(73));
	}

	[TestCase(-1)]
	[TestCase(int.MaxValue)]
	public void InvalidReferenceCountsAreRejected(int count)
	{
		byte[] bytes = new byte[8];
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes, 2);
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), count);
		var collection = AssetCreator.CreateCollection(UnityVersion.Parse("6000.5.10f1"));
		Assert.Throws<InvalidDataException>(() =>
		{
			EndianSpanReader reader = new(bytes, EndianType.LittleEndian);
			ManagedReferenceRegistry.Read(ref reader, collection, null);
		});
	}

	private static void WriteString(EndianWriter writer, string value)
	{
		writer.Write((Utf8String)value);
		writer.AlignStream();
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
