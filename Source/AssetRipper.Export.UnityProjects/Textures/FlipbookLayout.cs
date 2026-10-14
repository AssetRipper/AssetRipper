namespace AssetRipper.Export.UnityProjects.Textures;

/// <summary>
/// How the layers of a texture array or 3D texture are arranged in a single image, which Unity's texture importer reads as a flipbook.
/// </summary>
/// <param name="Columns">The number of layers in each row of the image.</param>
/// <param name="Rows">The number of rows of layers.</param>
public readonly record struct FlipbookLayout(int Columns, int Rows)
{
	/// <summary>
	/// The largest width or height of a texture that Unity can import.
	/// </summary>
	public const int MaxImageSize = 16384;

	/// <summary>
	/// Choose a layout for <paramref name="depth"/> layers of size <paramref name="width"/> by <paramref name="height"/>.
	/// </summary>
	/// <remarks>
	/// A single column is used when it fits, since it matches the layer order in memory and has no unused cells.
	/// Otherwise the layers are arranged in a grid that is as close to square as possible, preferring a grid without unused cells,
	/// because Unity imports every cell as a layer.
	/// </remarks>
	/// <returns>False if the layers can't fit in an image that Unity can import.</returns>
	public static bool TryCreate(int width, int height, int depth, out FlipbookLayout layout)
	{
		layout = default;
		if (width <= 0 || height <= 0 || depth <= 0 || width > MaxImageSize || height > MaxImageSize)
		{
			return false;
		}

		int maxColumns = MaxImageSize / width;
		int maxRows = MaxImageSize / height;
		if (1L * maxColumns * maxRows < depth)
		{
			return false;
		}
		if (depth <= maxRows)
		{
			layout = new FlipbookLayout(1, depth);
			return true;
		}

		// The fewest columns that keep the rows within the limit, and the number of columns that makes the image square.
		int minColumns = (depth + maxRows - 1) / maxRows;
		int squareColumns = int.Clamp((int)Math.Ceiling(Math.Sqrt((double)depth * height / width)), minColumns, maxColumns);

		// Prefer an exact grid near the square one, so that no empty layers are added.
		for (int distance = 0; distance <= maxColumns; distance++)
		{
			foreach (int columns in (ReadOnlySpan<int>)[squareColumns + distance, squareColumns - distance])
			{
				if (columns >= minColumns && columns <= maxColumns && depth % columns == 0)
				{
					layout = new FlipbookLayout(columns, depth / columns);
					return true;
				}
			}
		}

		layout = new FlipbookLayout(squareColumns, (depth + squareColumns - 1) / squareColumns);
		return true;
	}

	public int LayerCount => Columns * Rows;
}
