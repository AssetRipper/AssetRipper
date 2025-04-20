using AssetRipper.HashAlgorithms;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using System.Buffers.Binary;
using System.Text;

namespace AssetRipper.Export.UnityProjects.Scripts;

public static class ScriptHashing
{
	/// <summary>
	/// Compute the FileID of a script inside a compiled assembly.
	/// </summary>
	/// <remarks>
	/// This replicates a Unity algorithm.
	/// </remarks>
	/// <param name="namespace">The namespace of the script.</param>
	/// <param name="name">The name of the script.</param>
	public static int CalculateScriptFileID(string @namespace, string name)
	{
		Span<byte> source = stackalloc byte[4 + Encoding.UTF8.GetByteCount(@namespace) + Encoding.UTF8.GetByteCount(name)];

		source[0] = (byte)'s';
		source[1] = 0;
		source[2] = 0;
		source[3] = 0;

		int namespaceLength = Encoding.UTF8.GetBytes(@namespace, source[4..]);

		int nameLength = Encoding.UTF8.GetBytes(name, source[(4 + namespaceLength)..]);

		Span<byte> destination = stackalloc byte[16];
		MD4.HashData(source[..(4 + namespaceLength + nameLength)], destination);

		return BinaryPrimitives.ReadInt32LittleEndian(destination);
	}

	/// <summary>
	/// Compute the FileID of a script inside a compiled assembly.
	/// </summary>
	/// <remarks>
	/// This replicates a Unity algorithm.
	/// </remarks>
	/// <param name="namespace">The namespace of the script encoded as UTF8.</param>
	/// <param name="name">The name of the script encoded as UTF8.</param>
	public static int CalculateScriptFileID(ReadOnlySpan<byte> @namespace, ReadOnlySpan<byte> name)
	{
		Span<byte> source = stackalloc byte[4 + @namespace.Length + name.Length];

		source[0] = (byte)'s';
		source[1] = 0;
		source[2] = 0;
		source[3] = 0;

		@namespace.CopyTo(source[4..]);
		name.CopyTo(source[(4 + @namespace.Length)..]);

		Span<byte> destination = stackalloc byte[16];
		MD4.HashData(source, destination);

		return BinaryPrimitives.ReadInt32LittleEndian(destination);
	}

	/// <summary>
	/// Compute the FileID of a script inside a compiled assembly.
	/// </summary>
	/// <remarks>
	/// This replicates a Unity algorithm.
	/// </remarks>
	public static int CalculateScriptFileID(IMonoScript script)
	{
		return CalculateScriptFileID(script.Namespace.Data, script.ClassName_R.Data);
	}

	/// <summary>
	/// Compute a unique hash of a script and use that as the Guid for the script.
	/// </summary>
	/// <remarks>
	/// This is for consistency. Script guid's are random when created in Unity.
	/// </remarks>
	/// <param name="assemblyName">The name of the assembly (without any file extension) encoded as UTF8.</param>
	/// <param name="namespace">The namespace of the script encoded as UTF8.</param>
	/// <param name="className">The name of the script encoded as UTF8.</param>
	public static UnityGuid CalculateScriptGuid(ReadOnlySpan<byte> assemblyName, ReadOnlySpan<byte> @namespace, ReadOnlySpan<byte> className)
	{
		return UnityGuid.Md5Hash(assemblyName, @namespace, className);
	}

	/// <summary>
	/// Compute a unique hash of a script and use that as the Guid for the script.
	/// </summary>
	/// <remarks>
	/// This is for consistency. Script guid's are random when created in Unity.
	/// </remarks>
	public static UnityGuid CalculateScriptGuid(IMonoScript script)
	{
		//The assembly file name without any extension.
		ReadOnlySpan<byte> assemblyName = Encoding.UTF8.GetBytes(script.GetAssemblyNameFixed());
		return CalculateScriptGuid(assemblyName, script.Namespace.Data, script.ClassName_R.Data);
	}

	/// <summary>
	/// Compute a unique hash of an assembly name and use that as the Guid for the assembly.
	/// </summary>
	/// <remarks>
	/// This is for consistency. Assembly guid's are random when created in Unity.
	/// </remarks>
	public static UnityGuid CalculateAssemblyGuid(string assemblyName)
	{
		return CalculateAssemblyGuidInternal(SpecialFileNames.RemoveAssemblyFileExtension(assemblyName));
	}

	/// <summary>
	/// Compute a unique hash of an assembly name and use that as the Guid for the assembly.
	/// </summary>
	/// <remarks>
	/// This is for consistency. Assembly guid's are random when created in Unity.
	/// </remarks>
	public static UnityGuid CalculateAssemblyGuid(IMonoScript script)
	{
		return CalculateAssemblyGuidInternal(script.GetAssemblyNameFixed());
	}

	private static UnityGuid CalculateAssemblyGuidInternal(string assemblyName)
	{
		return UnityGuid.Md5Hash(assemblyName);
	}
}
