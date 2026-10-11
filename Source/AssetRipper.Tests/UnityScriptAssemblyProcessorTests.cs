using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AssetRipper.Export.UnityProjects.Scripts;

namespace AssetRipper.Tests;

public class UnityScriptAssemblyProcessorTests
{
	[Test]
	public void OnlyTheTwoRegistryDataTypesBecomePublic()
	{
		ModuleDefinition module = new("Unity.Entities.dll");
		TypeDefinition manager = new("Unity.Entities", "TypeManager", TypeAttributes.Public);
		TypeDefinition registry = new("Unity.Entities", "TypeRegistry", TypeAttributes.Public);
		module.TopLevelTypes.Add(manager);
		module.TopLevelTypes.Add(registry);
		TypeDefinition systemInfo = new(null, "SystemTypeInfo", TypeAttributes.NestedAssembly | TypeAttributes.SequentialLayout);
		TypeDefinition systemAttribute = new(null, "SystemAttributeWithType", TypeAttributes.NestedAssembly);
		TypeDefinition unrelated = new(null, "Unrelated", TypeAttributes.NestedPrivate);
		manager.NestedTypes.Add(systemInfo);
		registry.NestedTypes.Add(systemAttribute);
		manager.NestedTypes.Add(unrelated);
		FieldDefinition field = new("Size", FieldAttributes.Assembly, module.CorLibTypeFactory.Int32);
		systemInfo.Fields.Add(field);

		UnityScriptAssemblyProcessor.MakeRegistryDataTypesAccessible(module);

		Assert.That(systemInfo.IsNestedPublic, Is.True);
		Assert.That(systemAttribute.IsNestedPublic, Is.True);
		Assert.That(systemInfo.IsSequentialLayout, Is.True);
		Assert.That(unrelated.IsNestedPrivate, Is.True);
		Assert.That(field.IsAssembly, Is.True);
	}
}
