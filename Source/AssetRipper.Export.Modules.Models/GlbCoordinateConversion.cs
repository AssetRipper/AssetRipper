using System.Numerics;

namespace AssetRipper.Export.Modules.Models;

internal static class GlbCoordinateConversion
{
	/// <summary>
	/// Define the transformation between Unity coordinate space and glTF.
	/// glTF is a right-handed coordinate system, where the 'right' direction is -X relative to
	/// Unity's coordinate system.
	/// glTF matrix: column vectors, column-major storage, +Y up, +Z forward, -X right, right-handed
	/// unity matrix: column vectors, column-major storage, +Y up, +Z forward, +X right, left-handed
	/// multiply by a negative X scale to convert handedness
	/// </summary>
	public static readonly Vector3 CoordinateSpaceConversionScale = new Vector3(-1, 1, 1);

	public static readonly Vector4 TangentSpaceConversionScale = new Vector4(-1, 1, 1, -1);

	/// <summary>
	/// Define whether the coordinate space scale conversion above means we have a change in handedness.
	/// This is used when determining the conventional direction of rotation - the right-hand rule states
	/// that rotations are clockwise in left-handed systems and counter-clockwise in right-handed systems.
	/// Reversing the direction of one or three axes of reverses the handedness.
	/// </summary>
	private const float axisFlipScale = -1;

	/// <summary>
	/// Convert unity quaternion to a gltf quaternion
	/// </summary>
	/// <param name="unityQuaternion">unity quaternion</param>
	/// <returns>gltf quaternion</returns>
	public static Quaternion ToGltfQuaternionConvert(Quaternion unityQuaternion)
	{
		Vector3 fromAxisOfRotation = new Vector3(unityQuaternion.X, unityQuaternion.Y, unityQuaternion.Z);
		Vector3 toAxisOfRotation = axisFlipScale * fromAxisOfRotation * CoordinateSpaceConversionScale;
		return new Quaternion(toAxisOfRotation.X, toAxisOfRotation.Y, toAxisOfRotation.Z, unityQuaternion.W);
	}

	public static Vector3 ToGltfVector3Convert(Vector3 unityVector)
	{
		return unityVector * CoordinateSpaceConversionScale;
	}

	public static Vector4 ToGltfTangentConvert(Vector4 unityVector)
	{
		return unityVector * TangentSpaceConversionScale;
	}
}
