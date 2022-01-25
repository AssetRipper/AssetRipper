
namespace AssetRipper.Core.Classes.ShaderVariantCollection
{
	/// <summary>
	/// Shader pass type for Unity's lighting pipeline.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum PassType
	{
		/// <summary>
		/// Regular shader pass that does not interact with lighting.
		/// </summary>
		Normal = 0,
		/// <summary>
		/// Legacy vertex-lit shader pass.
		/// </summary>
		Vertex = 1,
		/// <summary>
		/// Legacy vertex-lit shader pass, with mobile lightmaps.
		/// </summary>
		VertexLM = 2,
		/// <summary>
		/// Legacy vertex-lit shader pass, with desktop (RGBM) lightmaps.
		/// </summary>
		VertexLMRGBM = 3,
		/// <summary>
		/// Forward rendering base pass.
		/// </summary>
		ForwardBase = 4,
		/// <summary>
		/// Forward rendering additive pixel light pass.
		/// </summary>
		ForwardAdd = 5,
		/// <summary>
		/// Legacy deferred lighting (light pre-pass) base pass.
		/// </summary>
		LightPrePassBase = 6,
		/// <summary>
		/// Legacy deferred lighting (light pre-pass) final pass.
		/// </summary>
		LightPrePassFinal = 7,
		/// <summary>
		/// Shadow caster and depth texure shader pass.
		/// </summary>
		ShadowCaster = 8,
		/// <summary>
		/// Not needed starting with 5.0
		/// </summary>
		ShadowCollector = 9,
		/// <summary>
		/// Deferred Shading shader pass.
		/// </summary>
		Deferred = 10,
		/// <summary>
		/// Shader pass used to generate the albedo and emissive values used as input to lightmapping.
		/// </summary>
		Meta = 11,
		/// <summary>
		/// Motion vector render pass.
		/// </summary>
		MotionVectors = 12,
		/// <summary>
		/// Custom scriptable pipeline.
		/// </summary>
		ScriptableRenderPipeline = 13,
		/// <summary>
		/// Custom scriptable pipeline when lightmode is set to default unlit or no light mode is set.
		/// </summary>
		ScriptableRenderPipelineDefaultUnlit = 14,
	}
}
