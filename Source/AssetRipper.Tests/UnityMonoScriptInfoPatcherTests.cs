using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AssetRipper.Export.UnityProjects.Scripts;

namespace AssetRipper.Tests;

internal class UnityMonoScriptInfoPatcherTests
{
	private const string GeneratedTypeName = "UnitySourceGeneratedAssemblyMonoScriptTypes_v1";
	private const string GeneratedDataName = "MonoScriptData";

	[Test]
	public async Task IncompleteGeneratedTypeIsRemoved()
	{
		AssemblyDefinition assembly = CreateAssembly(out ModuleDefinition module);
		module.TopLevelTypes.Add(new TypeDefinition(null, GeneratedTypeName, TypeAttributes.NotPublic));

		bool removed = UnityMonoScriptInfoPatcher.TryRemoveIncompleteGeneratedType(assembly);

		await Assert.That(removed).IsTrue();
		await Assert.That(module.TopLevelTypes.Any(type => type.Name == GeneratedTypeName)).IsFalse();
	}

	[Test]
	public async Task GeneratedTypeWithGetMethodIsKept()
	{
		AssemblyDefinition assembly = CreateAssembly(out ModuleDefinition module);
		TypeDefinition type = new(null, GeneratedTypeName, TypeAttributes.NotPublic);
		module.TopLevelTypes.Add(type);
		TypeDefinition data = new(null, GeneratedDataName, TypeAttributes.NestedPrivate | TypeAttributes.Sealed);
		type.NestedTypes.Add(data);
		type.Methods.Add(new MethodDefinition("Get", MethodAttributes.Public | MethodAttributes.Static, MethodSignature.CreateStatic(data.ToTypeSignature())));

		bool removed = UnityMonoScriptInfoPatcher.TryRemoveIncompleteGeneratedType(assembly);

		await Assert.That(removed).IsFalse();
		await Assert.That(module.TopLevelTypes.Any(t => t.Name == GeneratedTypeName)).IsTrue();
	}

	private static AssemblyDefinition CreateAssembly(out ModuleDefinition module)
	{
		AssemblyDefinition assembly = new("TestAssembly", new Version(1, 0, 0, 0));
		module = new ModuleDefinition("TestAssembly.dll");
		assembly.Modules.Add(module);
		return assembly;
	}
}
