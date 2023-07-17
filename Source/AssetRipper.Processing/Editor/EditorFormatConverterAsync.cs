using AssetRipper.Assets;
using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Extensions;
using System.Numerics;

namespace AssetRipper.Processing.Editor;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Rules for the methods in this class:
/// <list type="bullet">
/// <item>All methods must be static.</item>
/// <item>All public methods must return void and be named Convert.</item>
/// <item>Each public method must only take one parameter, and that parameter's type must inherit from <see cref="IUnityObjectBase"/>.</item>
/// <item>They must not resolve any PPtrs.</item>
/// </list>
/// </remarks>
internal static class EditorFormatConverterAsync
{
	public static void Convert(ITransform transform)
	{
		if (transform.Has_RootOrder_C4())
		{
			transform.RootOrder_C4 = transform.CalculateRootOrder();
		}
		if (transform.Has_LocalEulerAnglesHint_C4())
		{
			Vector3 eulerHints = new Quaternion(
				transform.LocalRotation_C4.X,
				transform.LocalRotation_C4.Y,
				transform.LocalRotation_C4.Z,
				transform.LocalRotation_C4.W).ToEulerAngle(true);
			transform.LocalEulerAnglesHint_C4.SetValues(eulerHints.X, eulerHints.Y, eulerHints.Z);
		}
	}
}
