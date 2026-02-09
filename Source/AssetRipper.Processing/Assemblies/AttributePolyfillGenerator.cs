using AsmResolver.DotNet;
using AsmResolver.DotNet.Cloning;
using AsmResolver.DotNet.Signatures;
using AssetRipper.CIL;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// Generates polyfills for missing attributes using the Roslyn compiler.
/// During compilation, these attributes can be stripped because they're not "custom" attributes.
/// They're special and get encoded in the metadata directly.
/// </summary>
/// <remarks>
/// <see href="https://github.com/icsharpcode/ILSpy/blob/master/ICSharpCode.Decompiler/TypeSystem/Implementation/KnownAttributes.cs"/>
/// </remarks>
public sealed class AttributePolyfillGenerator : IAssetProcessor
{
	/// <summary>
	/// Injects generated attribute polyfills into the loaded game data.
	/// </summary>
	/// <param name="gameData">The loaded game data.</param>
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);

	/// <summary>
	/// Generates attribute polyfills for the given assembly manager.
	/// </summary>
	/// <param name="manager">The assembly manager to modify.</param>
	private static void Process(IAssemblyManager manager)
	{
		ModuleDefinition? mscorlib = manager.Mscorlib?.ManifestModule;
		if (mscorlib is null)
		{
			return;
		}

		manager.ClearStreamCache();

		ModuleDefinition polyfillModule = EmbeddedAssembly.Load();

		MemberCloner cloner = new(mscorlib, (context) => new CustomCloneReferenceImporter(context));
		cloner.Include(polyfillModule.TopLevelTypes.Where(t => !mscorlib.HasTopLevelType(t.Namespace, t.Name)));

		MemberCloneResult cloneResult = cloner.Clone();
		foreach (TypeDefinition type in cloneResult.ClonedTopLevelTypes)
		{
			mscorlib.TopLevelTypes.Add(type);
		}
	}

	/// <summary>
	/// Custom reference importer for cloning. It handles redirects references from the source module to the target module.
	/// </summary>
	private sealed class CustomCloneReferenceImporter : CloneContextAwareReferenceImporter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CustomCloneReferenceImporter"/> class.
		/// </summary>
		/// <param name="context">The cloning context.</param>
		public CustomCloneReferenceImporter(MemberCloneContext context) : base(context)
		{
		}

		/// <inheritdoc/>
		protected override ITypeDefOrRef ImportType(TypeDefinition type)
		{
			if (type.DeclaringType is null && TargetModule.TryGetTopLevelType(type.Namespace, type.Name, out TypeDefinition? typeDefinition))
			{
				return typeDefinition;
			}
			return base.ImportType(type);
		}

		/// <inheritdoc/>
		protected override ITypeDefOrRef ImportType(TypeReference type)
		{
			if (SignatureComparer.Default.Equals(type.Scope, TargetModule) &&
				type.DeclaringType is null &&
				TargetModule.TryGetTopLevelType(type.Namespace, type.Name, out TypeDefinition? typeDefinition))
			{
				return typeDefinition;
			}
			return base.ImportType(type);
		}
	}
}
