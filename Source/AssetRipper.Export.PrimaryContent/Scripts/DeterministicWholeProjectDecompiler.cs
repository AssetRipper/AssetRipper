using AssetRipper.Assets;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.Metadata;

namespace AssetRipper.Export.PrimaryContent.Scripts;

public sealed class DeterministicWholeProjectDecompiler(DecompilerSettings settings, IAssemblyResolver assemblyResolver)
	: WholeProjectDecompiler(settings, RandomGuid.Next(), assemblyResolver, null, null, null);
