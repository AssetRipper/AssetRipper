using System.Reflection;
using System.Reflection.Emit;

namespace AssetRipper.Decompilation.CSharp.SourceGenerator;

internal static class Program
{
	static void Main(string[] args)
	{
		List<OpCode> opCodes = GetOpCodes().OrderBy(c => c.Value).ToList();
		Console.WriteLine("Done!");
	}

	/// <summary>
	/// Gets all opcodes from <see cref="OpCodes"/> class.
	/// </summary>
	/// <remarks>
	/// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes" /><br />
	/// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcode" />
	/// </remarks>
	/// <returns>An enumeration of all the <see cref="OpCode"/>s.</returns>
	static IEnumerable<OpCode> GetOpCodes()
	{
		foreach (FieldInfo field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
		{
			if (field.FieldType == typeof(OpCode))
			{
				object fieldValue = field.GetValue(null) ?? throw new NullReferenceException();
				yield return (OpCode)fieldValue;
			}
		}
	}
}
