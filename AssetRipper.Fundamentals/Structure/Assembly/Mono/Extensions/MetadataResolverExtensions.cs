using Mono.Cecil;
using System.Reflection;

namespace AssetRipper.Core.Structure.Assembly.Mono.Extensions
{
	public static class MetadataResolverExtensions
	{
		/*
		 * MetadataResolver.AreSame(TypeReference, TypeReference)
		 */

		static MethodInfo areSameMethodInfo = typeof(MetadataResolver).GetMethod(
				"AreSame",
				BindingFlags.NonPublic | BindingFlags.Static,
				null,
				new Type[] { typeof(TypeReference), typeof(TypeReference) },
				null)
			?? throw new Exception();

		public static bool AreSame(TypeReference a, TypeReference b)
		{
			return (bool)(areSameMethodInfo.Invoke(null, new object[] { a, b }) ?? throw new Exception());
		}
	}
}
