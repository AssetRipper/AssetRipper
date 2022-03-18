using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Animation;
using AssetRipper.Core.Classes.AnimationClip;
using AssetRipper.Core.Classes.Animator;
using AssetRipper.Core.Classes.AnimatorController;
using AssetRipper.Core.Classes.AnimatorOverrideController;
using AssetRipper.Core.Classes.AssetBundle;
using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Classes.AudioEchoFilter;
using AssetRipper.Core.Classes.AudioHighPassFilter;
using AssetRipper.Core.Classes.AudioChorusFilter;
using AssetRipper.Core.Classes.AudioDistortionFilter;
using AssetRipper.Core.Classes.AudioManager;
using AssetRipper.Core.Classes.AudioSource;
using AssetRipper.Core.Classes.Avatar;
using AssetRipper.Core.Classes.AvatarMask;
using AssetRipper.Core.Classes.BoxCollider2D;
using AssetRipper.Core.Classes.Camera;
using AssetRipper.Core.Classes.CapsuleCollider2D;
using AssetRipper.Core.Classes.ClusterInputManager;
using AssetRipper.Core.Classes.CompositeCollider2D;
using AssetRipper.Core.Classes.ComputeShader;
using AssetRipper.Core.Classes.ConstantForce;
using AssetRipper.Core.Classes.EditorBuildSettings;
using AssetRipper.Core.Classes.EditorSettings;
using AssetRipper.Core.Classes.Flare;
using AssetRipper.Core.Classes.Font;
using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.GraphicsSettings;
using AssetRipper.Core.Classes.GUIText;
using AssetRipper.Core.Classes.InputManager;
using AssetRipper.Core.Classes.Light;
using AssetRipper.Core.Classes.LightingDataAsset;
using AssetRipper.Core.Classes.LightmapSettings;
using AssetRipper.Core.Classes.LightProbes;
using AssetRipper.Core.Classes.LineRenderer;
using AssetRipper.Core.Classes.LODGroup;
using AssetRipper.Core.Classes.Material;
using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.MeshCollider;
using AssetRipper.Core.Classes.Meta.Importers;
using AssetRipper.Core.Classes.Meta.Importers.Texture;
using AssetRipper.Core.Classes.NavMeshAgent;
using AssetRipper.Core.Classes.NavMeshData;
using AssetRipper.Core.Classes.NavMeshObstacle;
using AssetRipper.Core.Classes.NavMeshProjectSettings;
using AssetRipper.Core.Classes.NavMeshSettings;
using AssetRipper.Core.Classes.NewAnimationTrack;
using AssetRipper.Core.Classes.OcclusionCullingData;
using AssetRipper.Core.Classes.OcclusionCullingSettings;
using AssetRipper.Core.Classes.ParticleSystem;
using AssetRipper.Core.Classes.ParticleSystemForceField;
using AssetRipper.Core.Classes.ParticleSystemRenderer;
using AssetRipper.Core.Classes.PhysicMaterial;
using AssetRipper.Core.Classes.Physics2DSettings;
using AssetRipper.Core.Classes.PhysicsManager;
using AssetRipper.Core.Classes.PrefabInstance;
using AssetRipper.Core.Classes.QualitySettings;
using AssetRipper.Core.Classes.ReflectionProbe;
using AssetRipper.Core.Classes.RenderSettings;
using AssetRipper.Core.Classes.RenderTexture;
using AssetRipper.Core.Classes.ResourceManager;
using AssetRipper.Core.Classes.Rigidbody;
using AssetRipper.Core.Classes.Rigidbody2D;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.ShaderVariantCollection;
using AssetRipper.Core.Classes.Sprite;
using AssetRipper.Core.Classes.SpriteAtlas;
using AssetRipper.Core.Classes.SpriteRenderer;
using AssetRipper.Core.Classes.StreamingController;
using AssetRipper.Core.Classes.TagManager;
using AssetRipper.Core.Classes.Terrain;
using AssetRipper.Core.Classes.TerrainData;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Classes.TrailRenderer;
using AssetRipper.Core.Classes.UI;
using AssetRipper.Core.Classes.UI.Canvas;
using AssetRipper.Core.Classes.UnityConnectSettings;
using AssetRipper.Core.Classes.WheelCollider;
using AssetRipper.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Parser.Asset
{
	public class AssetFactory : AssetFactoryBase
	{
		public override IUnityObjectBase CreateAsset(AssetInfo assetInfo)
		{
			if (m_instantiators.TryGetValue(assetInfo.ClassID, out Func<AssetInfo, IUnityObjectBase> instantiator))
			{
				return instantiator(assetInfo);
			}
			return DefaultInstantiator(assetInfo);
		}

		public void OverrideInstantiator(int classType, Func<AssetInfo, IUnityObjectBase> instantiator)
		{
			if (instantiator == null)
			{
				throw new ArgumentNullException(nameof(instantiator));
			}
			m_instantiators[(ClassIDType)classType] = instantiator;
		}

		private static IUnityObjectBase DefaultInstantiator(AssetInfo assetInfo)
		{
			return assetInfo.ClassID switch
			{
				ClassIDType.GameObject => new GameObject(assetInfo),
				ClassIDType.Transform => new Transform(assetInfo),
				ClassIDType.TimeManager => new TimeManager(assetInfo),
				ClassIDType.AudioManager => new AudioManager(assetInfo),
				ClassIDType.InputManager => new InputManager(assetInfo),
				ClassIDType.Physics2DSettings => new Physics2DSettings(assetInfo),
				ClassIDType.Camera => new Camera(assetInfo),
				ClassIDType.Material => new Material(assetInfo),
				ClassIDType.MeshRenderer => new MeshRenderer(assetInfo),
				ClassIDType.Texture2D => new Texture2D(assetInfo),
				ClassIDType.OcclusionCullingSettings => new OcclusionCullingSettings(assetInfo),
				ClassIDType.GraphicsSettings => new GraphicsSettings(assetInfo),
				ClassIDType.MeshFilter => new MeshFilter(assetInfo),
				ClassIDType.OcclusionPortal => new OcclusionPortal(assetInfo),
				ClassIDType.Mesh => new Mesh(assetInfo),
				ClassIDType.Skybox => new Skybox(assetInfo),
				ClassIDType.QualitySettings => new QualitySettings(assetInfo),
				ClassIDType.Shader => new Shader(assetInfo),
				ClassIDType.TextAsset => new TextAsset(assetInfo),
				ClassIDType.Rigidbody2D => new Rigidbody2D(assetInfo),
				ClassIDType.Rigidbody => new Rigidbody(assetInfo),
				ClassIDType.PhysicsManager => new PhysicsManager(assetInfo),
				ClassIDType.CircleCollider2D => new CircleCollider2D(assetInfo),
				ClassIDType.PolygonCollider2D => new PolygonCollider2D(assetInfo),
				ClassIDType.BoxCollider2D => new BoxCollider2D(assetInfo),
				ClassIDType.PhysicsMaterial2D => new PhysicsMaterial2D(assetInfo),
				ClassIDType.MeshCollider => new MeshCollider(assetInfo),
				ClassIDType.BoxCollider => new BoxCollider(assetInfo),
				ClassIDType.CompositeCollider2D => new CompositeCollider2D(assetInfo),
				ClassIDType.EdgeCollider2D => new EdgeCollider2D(assetInfo),
				ClassIDType.CapsuleCollider2D => new CapsuleCollider2D(assetInfo),
				ClassIDType.ComputeShader => new ComputeShader(assetInfo),
				ClassIDType.AnimationClip => new AnimationClip(assetInfo),
				ClassIDType.ConstantForce => new ConstantForce(assetInfo),
				ClassIDType.TagManager => new TagManager(assetInfo),
				ClassIDType.AudioListener => new AudioListener(assetInfo),
				ClassIDType.AudioSource => new AudioSource(assetInfo),
				ClassIDType.AudioClip => new AudioClip(assetInfo),
				ClassIDType.RenderTexture => new RenderTexture(assetInfo),
				ClassIDType.Cubemap => new Cubemap(assetInfo),
				ClassIDType.Avatar => new Avatar(assetInfo),
				ClassIDType.AnimatorController => new AnimatorController(assetInfo),
				ClassIDType.GUILayer => new GUILayer(assetInfo),
				ClassIDType.Animator => new Animator(assetInfo),
				ClassIDType.TrailRenderer => new TrailRenderer(assetInfo),
				ClassIDType.TextMesh => new TextMesh(assetInfo),
				ClassIDType.RenderSettings => new RenderSettings(assetInfo),
				ClassIDType.Light => new Light(assetInfo),
				ClassIDType.Animation => new Animation(assetInfo),
				ClassIDType.MonoBehaviour => new MonoBehaviour(assetInfo),
				ClassIDType.MonoScript => new MonoScript(assetInfo),
				ClassIDType.MonoManager => new MonoManager(assetInfo),
				ClassIDType.Texture3D => new Texture3D(assetInfo),
				ClassIDType.NewAnimationTrack => new NewAnimationTrack(assetInfo),
				ClassIDType.LineRenderer => new LineRenderer(assetInfo),
				ClassIDType.Flare => new Flare(assetInfo),
				ClassIDType.Halo => new Halo(assetInfo),
				ClassIDType.LensFlare => new LensFlare(assetInfo),
				ClassIDType.FlareLayer => new FlareLayer(assetInfo),
				ClassIDType.HaloLayer => new HaloLayer(assetInfo),
				ClassIDType.NavMeshProjectSettings => new NavMeshProjectSettings(assetInfo),
				ClassIDType.Font => new Font(assetInfo),
				ClassIDType.GUITexture => new GUITexture(assetInfo),
				ClassIDType.GUIText => new GUIText(assetInfo),
				ClassIDType.PhysicMaterial => new PhysicMaterial(assetInfo),
				ClassIDType.SphereCollider => new SphereCollider(assetInfo),
				ClassIDType.CapsuleCollider => new CapsuleCollider(assetInfo),
				ClassIDType.SkinnedMeshRenderer => new SkinnedMeshRenderer(assetInfo),
				ClassIDType.BuildSettings => new BuildSettings(assetInfo),
				ClassIDType.CharacterController => new CharacterController(assetInfo),
				ClassIDType.AssetBundle => new AssetBundle(assetInfo),
				ClassIDType.WheelCollider => new WheelCollider(assetInfo),
				ClassIDType.ResourceManager => new ResourceManager(assetInfo),
				ClassIDType.NetworkManager => new NetworkManager(assetInfo),
				ClassIDType.PreloadData => new PreloadData(assetInfo),
				ClassIDType.MovieTexture => new MovieTexture(assetInfo),
				ClassIDType.TerrainCollider => new TerrainCollider(assetInfo),
				ClassIDType.TerrainData => new TerrainData(assetInfo),
				ClassIDType.LightmapSettings => new LightmapSettings(assetInfo),
				ClassIDType.EditorSettings => new EditorSettings(assetInfo),
				ClassIDType.AudioHighPassFilter => new AudioHighPassFilter(assetInfo),
				ClassIDType.AudioChorusFilter => new AudioChorusFilter(assetInfo),
				ClassIDType.AudioReverbZone => new AudioReverbZone(assetInfo),
				ClassIDType.AudioEchoFilter => new AudioEchoFilter(assetInfo),
				ClassIDType.AudioDistortionFilter => new AudioDistortionFilter(assetInfo),
				ClassIDType.WindZone => new WindZone(assetInfo),
				ClassIDType.OffMeshLink => new OffMeshLink(assetInfo),
				ClassIDType.OcclusionArea => new OcclusionArea(assetInfo),
				ClassIDType.NavMeshObsolete => new NavMeshObsolete(assetInfo),
				ClassIDType.NavMeshAgent => new NavMeshAgent(assetInfo),
				ClassIDType.NavMeshSettings => new NavMeshSettings(assetInfo),
				ClassIDType.ParticleSystem => new ParticleSystem(assetInfo),
				ClassIDType.ParticleSystemRenderer => new ParticleSystemRenderer(assetInfo),
				ClassIDType.ShaderVariantCollection => new ShaderVariantCollection(assetInfo),
				ClassIDType.LODGroup => new LODGroup(assetInfo),
				ClassIDType.NavMeshObstacle => new NavMeshObstacle(assetInfo),
				ClassIDType.SortingGroup => new SortingGroup(assetInfo),
				ClassIDType.SpriteRenderer => new SpriteRenderer(assetInfo),
				ClassIDType.Sprite => new Sprite(assetInfo),
				ClassIDType.ReflectionProbe => new ReflectionProbe(assetInfo),
				ClassIDType.Terrain => new Terrain(assetInfo),
				ClassIDType.LightProbeGroup => new LightProbeGroup(assetInfo),
				ClassIDType.AnimatorOverrideController => new AnimatorOverrideController(assetInfo),
				ClassIDType.CanvasRenderer => new CanvasRenderer(assetInfo),
				ClassIDType.Canvas => new Canvas(assetInfo),
				ClassIDType.RectTransform => new RectTransform(assetInfo),
				ClassIDType.CanvasGroup => new CanvasGroup(assetInfo),
				ClassIDType.ClusterInputManager => new ClusterInputManager(assetInfo),
				ClassIDType.NavMeshData => new NavMeshData(assetInfo),
				ClassIDType.ConstantForce2D => new ConstantForce2D(assetInfo),
				ClassIDType.LightProbes => new LightProbes(assetInfo),
				ClassIDType.UnityConnectSettings => new UnityConnectSettings(assetInfo),
				ClassIDType.ParticleSystemForceField => new ParticleSystemForceField(assetInfo),
				ClassIDType.OcclusionCullingData => new OcclusionCullingData(assetInfo),
				ClassIDType.PrefabInstance => new PrefabInstance(assetInfo),
				ClassIDType.TextureImporter => new TextureImporter(assetInfo),
				ClassIDType.AvatarMask or ClassIDType.AvatarMaskOld => new AvatarMask(assetInfo),
				ClassIDType.DefaultAsset => new DefaultAsset(assetInfo),
				ClassIDType.DefaultImporter => new DefaultImporter(assetInfo),
				ClassIDType.SceneAsset => new SceneAsset(assetInfo),
				ClassIDType.NativeFormatImporter => new NativeFormatImporter(assetInfo),
				ClassIDType.MonoImporter => new MonoImporter(assetInfo),
				ClassIDType.EditorBuildSettings => new EditorBuildSettings(assetInfo),
				ClassIDType.DDSImporter => new DDSImporter(assetInfo),
				ClassIDType.PVRImporter => new PVRImporter(assetInfo),
				ClassIDType.ASTCImporter => new ASTCImporter(assetInfo),
				ClassIDType.KTXImporter => new KTXImporter(assetInfo),
				ClassIDType.IHVImageFormatImporter => new IHVImageFormatImporter(assetInfo),
				ClassIDType.LightmapParameters => new LightmapParameters(assetInfo),
				ClassIDType.LightingDataAsset => new LightingDataAsset(assetInfo),
				ClassIDType.LightingSettings => new LightingSettings(assetInfo),
				ClassIDType.SpriteAtlas => new SpriteAtlas(assetInfo),
				ClassIDType.StreamingController => new StreamingController(assetInfo),
				ClassIDType.TerrainLayer => new TerrainLayer(assetInfo),
				_ => null,
			};
		}

		private readonly Dictionary<ClassIDType, Func<AssetInfo, IUnityObjectBase>> m_instantiators = new Dictionary<ClassIDType, Func<AssetInfo, IUnityObjectBase>>();
	}
}
