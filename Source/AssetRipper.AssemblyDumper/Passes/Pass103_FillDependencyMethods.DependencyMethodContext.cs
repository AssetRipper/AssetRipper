using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.AssemblyDumper.Passes;

public static partial class Pass103_FillDependencyMethods
{
	private sealed class DependencyMethodContext
	{
		public DependencyMethodContext()
		{
			{
				GenericInstanceTypeSignature tupleSignature = SharedState.Instance.Importer.ImportType(typeof(ValueTuple<,>)).MakeGenericInstanceType(
					SharedState.Instance.Importer.ImportType(typeof(string)).ToTypeSignature(),
					SharedState.Instance.Importer.ImportType(typeof(PPtr)).ToTypeSignature());
				TupleConstructor = MethodUtils.MakeConstructorOnGenericType(SharedState.Instance.Importer, tupleSignature, 2);
			}
		}
		public required CilInstructionCollection Processor { get; init; }
		public required TypeDefinition Type { get; init; }
		public required IFieldDescriptor CurrentField { get; init; }
		public required IFieldDescriptor ThisField { get; init; }
		public CorLibTypeFactory CorLibTypeFactory => Type.DeclaringModule!.CorLibTypeFactory;
		public IMethodDescriptor TupleConstructor { get; }
		public void EmitReturnTrue()
		{
			Processor.Add(CilOpCodes.Ldc_I4_1);
			Processor.Add(CilOpCodes.Ret);
		}
	}
}
