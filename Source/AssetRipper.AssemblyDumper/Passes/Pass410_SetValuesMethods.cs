using AssetRipper.AssemblyDumper.Types;
using System.Collections;
using System.Text;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass410_SetValuesMethods
{
	private const string SetValuesName = "SetValues";

	public static void DoPass()
	{
		foreach (SubclassGroup group in SharedState.Instance.SubclassGroups.Values)
		{
			ProcessGroup(group);
		}
	}

	private static void ProcessGroup(SubclassGroup group)
	{
		if (group.InterfaceProperties.All(prop => prop.HasSetAccessor))
		{
			group.ImplementSetValuesMethod();
		}
	}

	private static void ImplementSetValuesMethod(this SubclassGroup group)
	{
		MethodDefinition interfaceMethod = group.Interface.AddMethod(SetValuesName, InterfaceUtils.InterfaceMethodDeclaration, SharedState.Instance.Importer.Void);
		foreach (PropertyDefinition property in group.GetInterfacePropertiesInOrder())
		{
			interfaceMethod.AddParameter(property.Signature!.ReturnType, GetParameterName(property.Name));
		}

		foreach (GeneratedClassInstance instance in group.Instances)
		{
			MethodDefinition method = instance.Type.AddMethod(SetValuesName, InterfaceUtils.InterfaceMethodImplementation, SharedState.Instance.Importer.Void);
			CilInstructionCollection instructions = method.GetInstructions();
			IEnumerable<PropertyDefinition> properties = group.IsVector4()
				? new Vector4PropertyEnumerable_Instance(instance)
				: group.IsColorRGBAf()
					? new ColorPropertyEnumerable_Instance(instance)
					: instance.Properties.Select(c => c.Definition);
			foreach (PropertyDefinition property in properties)
			{
				Parameter parameter = method.AddParameter(property.Signature!.ReturnType, GetParameterName(property.Name));
				instructions.Add(CilOpCodes.Ldarg_0);
				instructions.Add(CilOpCodes.Ldarg, parameter);
				instructions.Add(CilOpCodes.Call, property.SetMethod ?? throw new Exception("Set method can't be null"));
			}
			instructions.Add(CilOpCodes.Ret);
			instructions.OptimizeMacros();
		}
	}

	private static IEnumerable<PropertyDefinition> GetInterfacePropertiesInOrder(this SubclassGroup group)
	{
		return group.IsVector4()
			? new Vector4PropertyEnumerable_Group(group)
			: group.IsColorRGBAf()
				? new ColorPropertyEnumerable_Group(group)
				: group.InterfaceProperties.Select(i => i.Definition);
	}

	private static string GetParameterName(string? propertyName)
	{
		if (string.IsNullOrEmpty(propertyName))
		{
			throw new Exception("Property name cannot be null or empty");
		}

		StringBuilder sb = new StringBuilder(propertyName.Length);
		sb.Append(char.ToLowerInvariant(propertyName[0]));
		if (propertyName.Length > 1)
		{
			sb.Append(propertyName.AsSpan(1));
		}
		return sb.ToString();
	}

	private static bool IsVector4(this SubclassGroup group) => group.Name is "Vector4f" or "Vector4Float" or "Quaternionf";
	private abstract class Vector4PropertyEnumerableBase : IEnumerable<PropertyDefinition>
	{
		public IEnumerator<PropertyDefinition> GetEnumerator()
		{
			yield return GetProperty("X");
			yield return GetProperty("Y");
			yield return GetProperty("Z");
			yield return GetProperty("W");
		}
		protected abstract PropertyDefinition GetProperty(string propertyName);
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	private sealed class Vector4PropertyEnumerable_Instance : Vector4PropertyEnumerableBase
	{
		private readonly GeneratedClassInstance instance;
		public Vector4PropertyEnumerable_Instance(GeneratedClassInstance instance) => this.instance = instance;
		protected override PropertyDefinition GetProperty(string propertyName)
		{
			return instance.Properties.Select(c => c.Definition).Single(property => property.Name == propertyName);
		}
	}
	private sealed class Vector4PropertyEnumerable_Group : Vector4PropertyEnumerableBase
	{
		private readonly SubclassGroup group;
		public Vector4PropertyEnumerable_Group(SubclassGroup group) => this.group = group;
		protected override PropertyDefinition GetProperty(string propertyName)
		{
			return group.InterfaceProperties.Select(i => i.Definition).Single(property => property.Name == propertyName);
		}
	}

	private static bool IsColorRGBAf(this SubclassGroup group) => group.Name is "ColorRGBAf";
	private abstract class ColorPropertyEnumerableBase : IEnumerable<PropertyDefinition>
	{
		public IEnumerator<PropertyDefinition> GetEnumerator()
		{
			yield return GetProperty("R");
			yield return GetProperty("G");
			yield return GetProperty("B");
			yield return GetProperty("A");
		}
		protected abstract PropertyDefinition GetProperty(string propertyName);
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	private sealed class ColorPropertyEnumerable_Instance : ColorPropertyEnumerableBase
	{
		private readonly GeneratedClassInstance instance;
		public ColorPropertyEnumerable_Instance(GeneratedClassInstance instance) => this.instance = instance;
		protected override PropertyDefinition GetProperty(string propertyName)
		{
			return instance.Properties.Select(c => c.Definition).Single(property => property.Name == propertyName);
		}
	}
	private sealed class ColorPropertyEnumerable_Group : ColorPropertyEnumerableBase
	{
		private readonly SubclassGroup group;
		public ColorPropertyEnumerable_Group(SubclassGroup group) => this.group = group;
		protected override PropertyDefinition GetProperty(string propertyName)
		{
			return group.InterfaceProperties.Select(i => i.Definition).Single(property => property.Name == propertyName);
		}
	}
}
