using System.Collections.Generic;
using uTinyRipper.Classes;

namespace uTinyRipper
{
	public interface IDependencyContext
	{
		IEnumerable<Object> FetchDependencies<T>(T dependent, string name)
			   where T : IDependent;
		IEnumerable<Object> FetchDependencies<T>(T[] dependents, string name)
			where T : IDependent;
		IEnumerable<Object> FetchDependencies<T>(IReadOnlyList<T> dependents, string name)
			where T : IDependent;
		IEnumerable<Object> FetchDependencies<T>(IEnumerable<T> dependents, string name)
			where T : IDependent;
		IEnumerable<Object> FetchDependencies<T>(PPtr<T>[] pointers, string name)
			where T : Object;
		IEnumerable<Object> FetchDependencies<T>(IReadOnlyList<PPtr<T>> pointers, string name)
			where T : Object;
		IEnumerable<Object> FetchDependencies<T>(IEnumerable<PPtr<T>> pointers, string name)
			where T : Object;
		Object FetchDependency<T>(PPtr<T> pointer, string name)
			where T : Object;

		Version Version { get; }
		Platform Platform { get; }
		TransferInstructionFlags Flags { get; }

		bool IsLog { get; }
		IAssetContainer File { get; set; }
	}
}
