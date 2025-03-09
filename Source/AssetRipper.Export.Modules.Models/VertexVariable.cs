using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using System.Numerics;

namespace AssetRipper.Export.Modules.Models;

/// <summary>
/// Defines a Vertex attribute with up to 1 color and up to 8 textures.
/// </summary>
internal struct VertexVariable : IVertexMaterial, IEquatable<VertexVariable>
{
	#region constructors

	public VertexVariable(
		int maxColors,
		int maxTextCoords)
	{
		MaxColors = int.Clamp(maxColors, 0, 1);
		MaxTextCoords = int.Clamp(maxTextCoords, 0, 8);
	}

	public VertexVariable(
		int maxColors,
		int maxTextCoords,
		Vector4 color = default,
		Vector2 texcoord0 = default,
		Vector2 texcoord1 = default,
		Vector2 texcoord2 = default,
		Vector2 texcoord3 = default,
		Vector2 texcoord4 = default,
		Vector2 texcoord5 = default,
		Vector2 texcoord6 = default,
		Vector2 texcoord7 = default)
	{
		Color = color;
		TexCoord0 = texcoord0;
		TexCoord1 = texcoord1;
		TexCoord2 = texcoord2;
		TexCoord3 = texcoord3;
		TexCoord4 = texcoord4;
		TexCoord5 = texcoord5;
		TexCoord6 = texcoord6;
		TexCoord7 = texcoord7;
		MaxColors = int.Clamp(maxColors, 0, 1);
		MaxTextCoords = int.Clamp(maxTextCoords, 0, 8);
	}

	public VertexVariable(
		int maxTextCoords,
		Vector2 texcoord0 = default,
		Vector2 texcoord1 = default,
		Vector2 texcoord2 = default,
		Vector2 texcoord3 = default,
		Vector2 texcoord4 = default,
		Vector2 texcoord5 = default,
		Vector2 texcoord6 = default,
		Vector2 texcoord7 = default) : this(0, maxTextCoords, default, texcoord0, texcoord1, texcoord2, texcoord3, texcoord4, texcoord5, texcoord6, texcoord7)
	{
	}

	public VertexVariable(IVertexMaterial src)
	{
		ArgumentNullException.ThrowIfNull(src);
		Color = 0 < src.MaxColors ? src.GetColor(0) : Vector4.One;
		TexCoord0 = 0 < src.MaxTextCoords ? src.GetTexCoord(0) : Vector2.Zero;
		TexCoord1 = 1 < src.MaxTextCoords ? src.GetTexCoord(1) : Vector2.Zero;
		TexCoord2 = 2 < src.MaxTextCoords ? src.GetTexCoord(2) : Vector2.Zero;
		TexCoord3 = 3 < src.MaxTextCoords ? src.GetTexCoord(3) : Vector2.Zero;
		TexCoord4 = 4 < src.MaxTextCoords ? src.GetTexCoord(4) : Vector2.Zero;
		TexCoord5 = 5 < src.MaxTextCoords ? src.GetTexCoord(5) : Vector2.Zero;
		TexCoord6 = 6 < src.MaxTextCoords ? src.GetTexCoord(6) : Vector2.Zero;
		TexCoord7 = 7 < src.MaxTextCoords ? src.GetTexCoord(7) : Vector2.Zero;
		MaxColors = int.Clamp(src.MaxColors, 0, 1);
		MaxTextCoords = int.Clamp(src.MaxTextCoords, 0, 8);
	}
	#endregion

	#region data

	public Vector4 Color;
	public Vector2 TexCoord0;
	public Vector2 TexCoord1;
	public Vector2 TexCoord2;
	public Vector2 TexCoord3;
	public Vector2 TexCoord4;
	public Vector2 TexCoord5;
	public Vector2 TexCoord6;
	public Vector2 TexCoord7;

	public readonly int MaxColors { get; }
	public readonly int MaxTextCoords { get; }

	readonly IEnumerable<KeyValuePair<string, AttributeFormat>> IVertexReflection.GetEncodingAttributes()
	{
		if (MaxColors == 1)
		{
			yield return new KeyValuePair<string, AttributeFormat>("COLOR_0", new AttributeFormat(DimensionType.VEC4, EncodingType.UNSIGNED_BYTE, true));
		}
		for (int i = 0; i < MaxTextCoords; i++)
		{
			yield return new KeyValuePair<string, AttributeFormat>($"TEXCOORD_{i}", new AttributeFormat(DimensionType.VEC2));
		}
	}

	public readonly override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Color);
		hash.Add(TexCoord0);
		hash.Add(TexCoord1);
		hash.Add(TexCoord2);
		hash.Add(TexCoord3);
		hash.Add(TexCoord4);
		hash.Add(TexCoord5);
		hash.Add(TexCoord6);
		hash.Add(TexCoord7);
		hash.Add(MaxColors);
		hash.Add(MaxTextCoords);
		return hash.ToHashCode();
	}

	public readonly override bool Equals(object? obj) { return obj is VertexVariable other ? Equals(other) : false; }

	public readonly bool Equals(VertexVariable other) { return AreEqual(this, other); }

	public static bool operator ==(in VertexVariable a, in VertexVariable b) { return AreEqual(a, b); }

	public static bool operator !=(in VertexVariable a, in VertexVariable b) { return !AreEqual(a, b); }

	public static bool AreEqual(in VertexVariable a, in VertexVariable b)
	{
		return a.Color == b.Color
			&& a.TexCoord0 == b.TexCoord0
			&& a.TexCoord1 == b.TexCoord1
			&& a.TexCoord2 == b.TexCoord2
			&& a.TexCoord3 == b.TexCoord3
			&& a.TexCoord4 == b.TexCoord4
			&& a.TexCoord5 == b.TexCoord5
			&& a.TexCoord6 == b.TexCoord6
			&& a.TexCoord7 == b.TexCoord7
			&& a.MaxColors == b.MaxColors
			&& a.MaxTextCoords == b.MaxTextCoords;
	}

	#endregion

	#region API

	public readonly VertexMaterialDelta Subtract(IVertexMaterial baseValue)
	{
		if (MaxTextCoords <= 4)
		{
			return new()
			{
				Color0Delta = baseValue.MaxColors > 0 ? Color - baseValue.GetColor(0) : default,
				TexCoord0Delta = baseValue.MaxTextCoords > 0 ? TexCoord0 - baseValue.GetTexCoord(0) : default,
				TexCoord1Delta = baseValue.MaxTextCoords > 1 ? TexCoord1 - baseValue.GetTexCoord(1) : default,
				TexCoord2Delta = baseValue.MaxTextCoords > 2 ? TexCoord2 - baseValue.GetTexCoord(2) : default,
				TexCoord3Delta = baseValue.MaxTextCoords > 3 ? TexCoord3 - baseValue.GetTexCoord(3) : default,
			};
		}
		else
		{
			throw new NotSupportedException();
		}
	}
	public void Add(in VertexMaterialDelta delta)
	{
		Color += delta.Color0Delta;
		TexCoord0 += delta.TexCoord0Delta;
		TexCoord1 += delta.TexCoord1Delta;
		TexCoord2 += delta.TexCoord2Delta;
		TexCoord3 += delta.TexCoord3Delta;
	}
	void IVertexMaterial.SetColor(int index, Vector4 color)
	{
		ArgumentOutOfRangeException.ThrowIfNotEqual(index, 0);
		Color = color;
	}
	void IVertexMaterial.SetTexCoord(int index, Vector2 coord)
	{
		switch (index)
		{
			case 0: TexCoord0 = coord; break;
			case 1: TexCoord1 = coord; break;
			case 2: TexCoord2 = coord; break;
			case 3: TexCoord3 = coord; break;
			case 4: TexCoord4 = coord; break;
			case 5: TexCoord5 = coord; break;
			case 6: TexCoord6 = coord; break;
			case 7: TexCoord7 = coord; break;
		}
	}
	public readonly Vector4 GetColor(int index)
	{
		ArgumentOutOfRangeException.ThrowIfNotEqual(index, 0);
		return Color;
	}
	public readonly Vector2 GetTexCoord(int index)
	{
		return index switch
		{
			0 => TexCoord0,
			1 => TexCoord1,
			2 => TexCoord2,
			3 => TexCoord3,
			4 => TexCoord4,
			5 => TexCoord5,
			6 => TexCoord6,
			7 => TexCoord7,
			_ => throw new ArgumentOutOfRangeException(nameof(index)),
		};
	}
	#endregion

}
