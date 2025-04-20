using Cpp2IL.Core.Api;
using Cpp2IL.Core.Model.Contexts;
using System.Diagnostics;

namespace AssetRipper.Import.Structure.Assembly.Managers;

/// <summary>
/// Method names do not always match the names of the methods they override (eg because of obfuscation). This class fixes that.
/// </summary>
internal sealed class MethodOverrideNameFixer : Cpp2IlProcessingLayer
{
	public override string Name => nameof(MethodOverrideNameFixer);

	public override string Id => Name;

	public override void Process(ApplicationAnalysisContext appContext, Action<int, int>? progressCallback = null)
	{
		HashSet<MethodAnalysisContext> methodsAnalyzed = new();
		foreach (MethodAnalysisContext method in appContext.AllTypes.SelectMany(t => t.Methods))
		{
			AnalyzeMethod(method, methodsAnalyzed);
		}
	}

	private static void AnalyzeMethod(MethodAnalysisContext method, HashSet<MethodAnalysisContext> methodsAnalyzed)
	{
		Debug.Assert(method is not ConcreteGenericMethodAnalysisContext);

		if (methodsAnalyzed.Contains(method))
			return;

		MethodAnalysisContext? baseMethod = method.BaseMethod;
		if (baseMethod is null)
			return;

		if (baseMethod is ConcreteGenericMethodAnalysisContext concreteGenericMethod)
		{
			AnalyzeMethod(concreteGenericMethod.BaseMethodContext, methodsAnalyzed);
		}
		else
		{
			AnalyzeMethod(baseMethod, methodsAnalyzed);
		}

		if (baseMethod.Name != method.Name)
		{
			method.OverrideName = baseMethod.Name;
		}
	}
}
