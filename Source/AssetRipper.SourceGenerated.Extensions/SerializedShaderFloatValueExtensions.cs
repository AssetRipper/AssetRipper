using AssetRipper.SourceGenerated.Subclasses.SerializedShaderFloatValue;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedShaderFloatValueExtensions
{
	extension(ISerializedShaderFloatValue floatValue)
	{
		public bool IsZero => floatValue.Value == 0.0f;
		public bool IsMax => floatValue.Value == 255.0f;
		public bool HasName => floatValue.Name.String is not "" and not "<noninit>";
		public bool IsZeroAndNameless => floatValue.IsZero && !floatValue.HasName;
		public bool IsMaxAndNameless => floatValue.IsMax && !floatValue.HasName;
		public Utf8String Name
		{
			get => floatValue.Has_Name_R_Utf8String() ? floatValue.Name_R_Utf8String : floatValue.Name_R_FastPropertyName.Name;
			set
			{
				if (floatValue.Has_Name_R_Utf8String())
				{
					floatValue.Name_R_Utf8String = value;
				}
				else
				{
					floatValue.Name_R_FastPropertyName.Name = value;
				}
			}
		}

		public bool IsDefault<T>(T defaultValue) where T : unmanaged, Enum
		{
			return EqualityComparer<T>.Default.Equals(floatValue.GetValue<T>(), defaultValue) && !floatValue.HasName;
		}

		public string GetNameOrFloatString()
		{
			if (floatValue.HasName)
			{
				return $"[{floatValue.Name}]";
			}
			return floatValue.Value.ToStringInvariant();
		}

		public string GetNameOrEnumString<T>() where T : unmanaged, Enum
		{
			if (floatValue.HasName)
			{
				return $"[{floatValue.Name}]";
			}
			return floatValue.GetValue<T>().ToString();
		}

		public T GetValue<T>() where T : unmanaged, Enum
		{
			if (Unsafe.SizeOf<T>() != sizeof(int))
			{
				Debug.Fail(null);
				return default;
			}
			return Unsafe.BitCast<int, T>((int)floatValue.Value);
		}
	}
}
