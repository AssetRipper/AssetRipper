using AssetRipper.IO.Endian;
using AssetRipper.Numerics;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions;

internal static class EndianWriterExtensions
{
	public static void Write(this EndianWriter writer, Vector2 value)
	{
		writer.Write(value.X);
		writer.Write(value.Y);
	}
	public static void Write(this EndianWriter writer, Vector3 value)
	{
		writer.Write(value.X);
		writer.Write(value.Y);
		writer.Write(value.Z);
	}
	public static void Write(this EndianWriter writer, Vector4 value)
	{
		writer.Write(value.X);
		writer.Write(value.Y);
		writer.Write(value.Z);
		writer.Write(value.W);
	}
	public static void Write(this EndianWriter writer, Color32 value)
	{
		writer.Write(value.R);
		writer.Write(value.G);
		writer.Write(value.B);
		writer.Write(value.A);
	}
}
