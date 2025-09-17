namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL;

// Should be renamed UConstants probably?
public static class USILConstants
{
	public static readonly int[] XYZW_MASK = [0, 1, 2, 3];
	public static readonly char[] MASK_CHARS = ['x', 'y', 'z', 'w'];

	public static readonly string[] MATRIX_MASK_CHARS = [
		"_m00", "_m01", "_m02", "_m03",
		"_m10", "_m11", "_m12", "_m13",
		"_m20", "_m21", "_m22", "_m23",
		"_m30", "_m31", "_m32", "_m33"
	];
	public static readonly string[] TMATRIX_MASK_CHARS = [
		"_m00", "_m10", "_m20", "_m30",
		"_m01", "_m11", "_m21", "_m31",
		"_m02", "_m12", "_m22", "_m32",
		"_m03", "_m13", "_m23", "_m33"
	];

	public static readonly char[] ITER_CHARS = ['i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u'];

	// should be functions
	public const string VERT_INPUT_NAME = "v";
	public const string VERT_OUTPUT_LOCAL_NAME = "o";
	public const string VERT_TO_FRAG_STRUCT_NAME = "v2f";

	public const string FRAG_INPUT_NAME = "inp";
	public const string FRAG_OUTPUT_LOCAL_NAME = "o";
	public const string FRAG_OUTPUT_STRUCT_NAME = "fout";
}
