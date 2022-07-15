namespace AssetRipper.Core.Classes.ParticleSystem.Shape
{
	/// <summary>
	/// The emission shape.
	/// </summary>
	public enum ParticleSystemShapeType
	{
		/// <summary>
		/// Emit from a sphere.
		/// </summary>
		Sphere = 0,
		/// <summary>
		/// Emit from the surface of a sphere.
		/// </summary>
		SphereShell = 1,
		/// <summary>
		/// Emit from a half-sphere.
		/// </summary>
		Hemisphere = 2,
		/// <summary>
		/// Emit from the surface of a half-sphere.
		/// </summary>
		HemisphereShell = 3,
		/// <summary>
		/// Emit from the base of a cone.
		/// </summary>
		Cone = 4,
		/// <summary>
		/// Emit from the volume of a box.
		/// </summary>
		Box = 5,
		/// <summary>
		/// Emit from a mesh.
		/// </summary>
		Mesh = 6,
		/// <summary>
		/// Emit from the base surface of a cone.
		/// </summary>
		ConeShell = 7,
		/// <summary>
		/// Emit from a cone.
		/// </summary>
		ConeVolume = 8,
		/// <summary>
		/// Emit from the surface of a cone.
		/// </summary>
		ConeVolumeShell = 9,
		/// <summary>
		/// Emit from a circle.
		/// </summary>
		Circle = 10,
		/// <summary>
		/// Emit from the edge of a circle.
		/// </summary>
		CircleEdge = 11,
		/// <summary>
		/// Emit from an edge.
		/// </summary>
		SingleSidedEdge = 12,
		/// <summary>
		/// Emit from a mesh renderer.
		/// </summary>
		MeshRenderer = 13,
		/// <summary>
		/// Emit from a skinned mesh renderer.
		/// </summary>
		SkinnedMeshRenderer = 14,
		/// <summary>
		/// Emit from the surface of a box.
		/// </summary>
		BoxShell = 15,
		/// <summary>
		/// Emit from the edges of a box.
		/// </summary>
		BoxEdge = 16,
		/// <summary>
		/// Emit from a Donut.
		/// </summary>
		Donut = 17
	}

	public static class ParticleSystemShapeTypeExtensions
	{
		public static bool IsBoxAny(this ParticleSystemShapeType _this)
		{
			switch (_this)
			{
				case ParticleSystemShapeType.Box:
				case ParticleSystemShapeType.BoxEdge:
				case ParticleSystemShapeType.BoxShell:
					return true;

				default:
					return false;
			}
		}
	}
}
