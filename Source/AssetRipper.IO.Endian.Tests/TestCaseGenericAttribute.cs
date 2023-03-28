using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

//https://stackoverflow.com/questions/2364929/nunit-testcase-with-generics

namespace AssetRipper.IO.Endian.Tests;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestCaseGenericAttribute : TestCaseAttribute, ITestBuilder
{
	public TestCaseGenericAttribute(params object[] arguments) : base(arguments)
	{
	}

	public Type[]? TypeArguments { get; set; }

	IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test? suite)
	{
		if (!method.IsGenericMethodDefinition)
			return base.BuildFrom(method, suite);

		if (TypeArguments == null || TypeArguments.Length != method.GetGenericArguments().Length)
		{
			TestCaseParameters parms = new TestCaseParameters { RunState = RunState.NotRunnable };
			parms.Properties.Set(PropertyNames.SkipReason, $"{nameof(TypeArguments)} should have {method.GetGenericArguments().Length} elements");
			return new[] { new NUnitTestCaseBuilder().BuildTestMethod(method, suite, parms) };
		}

		IMethodInfo genMethod = method.MakeGenericMethod(TypeArguments);
		return base.BuildFrom(genMethod, suite);
	}
}
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestCaseSourceGenericAttribute : TestCaseSourceAttribute, ITestBuilder
{
	public TestCaseSourceGenericAttribute(string sourceName) : base(sourceName)
	{
	}

	public Type[]? TypeArguments { get; set; }

	IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test? suite)
	{
		if (!method.IsGenericMethodDefinition)
			return base.BuildFrom(method, suite);

		if (TypeArguments == null || TypeArguments.Length != method.GetGenericArguments().Length)
		{
			TestCaseParameters parms = new TestCaseParameters { RunState = RunState.NotRunnable };
			parms.Properties.Set(PropertyNames.SkipReason, $"{nameof(TypeArguments)} should have {method.GetGenericArguments().Length} elements");
			return new[] { new NUnitTestCaseBuilder().BuildTestMethod(method, suite, parms) };
		}

		IMethodInfo genMethod = method.MakeGenericMethod(TypeArguments);
		return base.BuildFrom(genMethod, suite);
	}
}
/// <summary>
/// For exactly one type argument. See the base implementation.
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestCaseAttribute<T> : TestCaseGenericAttribute
{
	public TestCaseAttribute(params object[] arguments) : base(arguments) => TypeArguments = new[] { typeof(T) };
}
/// <summary>
/// For exactly two type arguments. See the base implementation.
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestCaseAttribute<T1, T2> : TestCaseGenericAttribute
{
	public TestCaseAttribute(params object[] arguments) : base(arguments) => TypeArguments = new[] { typeof(T1), typeof(T2) };
}
