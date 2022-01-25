using Mono.Cecil;
using System;
using System.Reflection;

namespace AssetRipper.Core.Structure.Assembly.Mono.Extensions
{
	public static class MetadataResolverExtensions
	{
		/*
		 * MetadataResolver.AreSame(TypeReference, TypeReference)
		 */

		static bool initialized;
		static MethodInfo areSameMethodInfo;

		private static void Initialize()
		{
			areSameMethodInfo = typeof(MetadataResolver).GetMethod(
				"AreSame",
				BindingFlags.NonPublic | BindingFlags.Static,
				null,
				new Type[] { typeof(TypeReference), typeof(TypeReference) },
				null);

			initialized = true;
		}

		public static bool AreSame(TypeReference a, TypeReference b)
		{
			if (!initialized) Initialize();
			return (bool)areSameMethodInfo.Invoke(null, new object[] { a, b });
		}
	}
}
