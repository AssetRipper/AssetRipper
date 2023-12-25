using AssetRipper.Assets.Traversal;
using AssetRipper.Primitives;
using System.Reflection;

namespace AssetRipper.Tests.Traversal;

partial class CustomInjectedObjectBase
{
	private static class PrimitiveHelper
	{
		private static readonly Dictionary<Type, MethodInfo> walkCache = new();

		public static void VisitPrimitive(AssetWalker walker, Type type, object? value)
		{
			if (!walkCache.TryGetValue(type, out MethodInfo? method))
			{
				method = typeof(AssetWalker).GetMethod(nameof(AssetWalker.VisitPrimitive), BindingFlags.Public | BindingFlags.Instance)
					!.MakeGenericMethod(type);
				walkCache.Add(type, method);
			}
			method.Invoke(walker, [value]);
		}

		public static bool IsPrimitive(Type type) => type.IsPrimitive || type == typeof(string) || type == typeof(Utf8String);
	}
}
