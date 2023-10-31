using System.Runtime.CompilerServices;

namespace SpirV;

public static class EnumExtensions
{
	public static uint AsUInt32<T>(this T value) where T : unmanaged, Enum
	{
		return As<T, uint>(value);
	}

	public static T AsEnum<T>(this uint value) where T : unmanaged, Enum
	{
		return As<uint, T>(value);
	}

	private static TTo As<TFrom, TTo>(TFrom source) where TFrom : unmanaged where TTo : unmanaged
	{
		if (Unsafe.SizeOf<TFrom>() == Unsafe.SizeOf<TTo>())
		{
			return Unsafe.As<TFrom, TTo>(ref source);
		}
		else
		{
			return ThrowOrReturnDefault<TTo>();
		}
	}

#if DEBUG
	[DoesNotReturn]
#endif
	private static T ThrowOrReturnDefault<T>() where T : struct
	{
#if DEBUG
		throw new InvalidCastException();
#else
		return default;//Exceptions prevent inlining.
#endif
	}
}
