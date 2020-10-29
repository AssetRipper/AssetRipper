using System;
using System.Collections.Generic;
using uTinyRipper.Classes;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper
{
	public class AssetFactory
	{
		public Object CreateAsset(AssetInfo assetInfo)
		{
			if (m_instantiators.TryGetValue(assetInfo.ClassID, out Func<AssetInfo, Object> instantiator))
			{
				return instantiator(assetInfo);
			}
			return DefaultInstantiator(assetInfo);
		}

		public void OverrideInstantiator(ClassIDType classType, Func<AssetInfo, Object> instantiator)
		{
			if (instantiator == null)
			{
				throw new ArgumentNullException(nameof(instantiator));
			}
			m_instantiators[classType] = instantiator;
		}

		private static Object DefaultInstantiator(AssetInfo assetInfo)
		{
			switch (assetInfo.ClassID)
			{
				case ClassIDType.GameObject:
					return new GameObject(assetInfo);
				case ClassIDType.Transform:
					return new Transform(assetInfo);
				case ClassIDType.TimeManager:
					return new TimeManager(assetInfo);
				case ClassIDType.AudioManager:
					return new AudioManager(assetInfo);
				case ClassIDType.InputManager:
					return new InputManager(assetInfo);
				case ClassIDType.Physics2DSettings:
					return new Physics2DSettings(assetInfo);
				case ClassIDType.Camera:
					return new Camera(assetInfo);
				case ClassIDType.Material:
					return new Material(assetInfo);
				case ClassIDType.MeshRenderer:
					return new MeshRenderer(assetInfo);
				case ClassIDType.Texture2D:
					return new Texture2D(assetInfo);
				case ClassIDType.OcclusionCullingSettings:
					return new OcclusionCullingSettings(assetInfo);
				case ClassIDType.GraphicsSettings:
					return new GraphicsSettings(assetInfo);
				case ClassIDType.MeshFilter:
					return new MeshFilter(assetInfo);
				case ClassIDType.OcclusionPortal:
					return new OcclusionPortal(assetInfo);
				case ClassIDType.Mesh:
					return new Mesh(assetInfo);
				case ClassIDType.Skybox:
					return new Skybox(assetInfo);
				case ClassIDType.QualitySettings:
					return new QualitySettings(assetInfo);
				case ClassIDType.Shader:
					return new Shader(assetInfo);
				case ClassIDType.TextAsset:
					return new TextAsset(assetInfo);
				case ClassIDType.Rigidbody2D:
					return new Rigidbody2D(assetInfo);
				case ClassIDType.Rigidbody:
					return new Rigidbody(assetInfo);
				case ClassIDType.PhysicsManager:
					return new PhysicsManager(assetInfo);
				case ClassIDType.CircleCollider2D:
					return new CircleCollider2D(assetInfo);
				case ClassIDType.PolygonCollider2D:
					return new PolygonCollider2D(assetInfo);
				case ClassIDType.BoxCollider2D:
					return new BoxCollider2D(assetInfo);
				case ClassIDType.PhysicsMaterial2D:
					return new PhysicsMaterial2D(assetInfo);
				case ClassIDType.MeshCollider:
					return new MeshCollider(assetInfo);
				case ClassIDType.BoxCollider:
					return new BoxCollider(assetInfo);
				case ClassIDType.CompositeCollider2D:
					return new CompositeCollider2D(assetInfo);
				case ClassIDType.EdgeCollider2D:
					return new EdgeCollider2D(assetInfo);
				case ClassIDType.CapsuleCollider2D:
					return new CapsuleCollider2D(assetInfo);
				case ClassIDType.AnimationClip:
					return new AnimationClip(assetInfo);
				case ClassIDType.TagManager:
					return new TagManager(assetInfo);
				case ClassIDType.AudioListener:
					return new AudioListener(assetInfo);
				case ClassIDType.AudioSource:
					return new AudioSource(assetInfo);
				case ClassIDType.AudioClip:
					return new AudioClip(assetInfo);
				case ClassIDType.RenderTexture:
					return new RenderTexture(assetInfo);
				case ClassIDType.Cubemap:
					return new Cubemap(assetInfo);
				case ClassIDType.Avatar:
					return new Avatar(assetInfo);
				case ClassIDType.AnimatorController:
					return new AnimatorController(assetInfo);
				case ClassIDType.GUILayer:
					return new GUILayer(assetInfo);
				case ClassIDType.Animator:
					return new Animator(assetInfo);
				case ClassIDType.TrailRenderer:
					return new TrailRenderer(assetInfo);
				case ClassIDType.TextMesh:
					return new TextMesh(assetInfo);
				case ClassIDType.RenderSettings:
					return new RenderSettings(assetInfo);
				case ClassIDType.Light:
					return new Light(assetInfo);
				case ClassIDType.Animation:
					return new Animation(assetInfo);
				case ClassIDType.MonoBehaviour:
					return new MonoBehaviour(assetInfo);
				case ClassIDType.MonoScript:
					return new MonoScript(assetInfo);
				case ClassIDType.MonoManager:
					return new MonoManager(assetInfo);
				case ClassIDType.Texture3D:
					return new Texture3D(assetInfo);
				case ClassIDType.NewAnimationTrack:
					return new NewAnimationTrack(assetInfo);
				case ClassIDType.FlareLayer:
					return new FlareLayer(assetInfo);
				case ClassIDType.NavMeshProjectSettings:
					return new NavMeshProjectSettings(assetInfo);
				case ClassIDType.Font:
					return new Font(assetInfo);
				case ClassIDType.GUITexture:
					return new GUITexture(assetInfo);
				case ClassIDType.GUIText:
					return new GUIText(assetInfo);
				case ClassIDType.PhysicMaterial:
					return new PhysicMaterial(assetInfo);
				case ClassIDType.SphereCollider:
					return new SphereCollider(assetInfo);
				case ClassIDType.CapsuleCollider:
					return new CapsuleCollider(assetInfo);
				case ClassIDType.SkinnedMeshRenderer:
					return new SkinnedMeshRenderer(assetInfo);
				case ClassIDType.BuildSettings:
					return new BuildSettings(assetInfo);
				case ClassIDType.CharacterController:
					return new CharacterController(assetInfo);
				case ClassIDType.AssetBundle:
					return new AssetBundle(assetInfo);
				case ClassIDType.WheelCollider:
					return new WheelCollider(assetInfo);
				case ClassIDType.ResourceManager:
					return new ResourceManager(assetInfo);
				case ClassIDType.NetworkManager:
					return new NetworkManager(assetInfo);
				case ClassIDType.PreloadData:
					return new PreloadData(assetInfo);
				case ClassIDType.MovieTexture:
					return new MovieTexture(assetInfo);
				case ClassIDType.TerrainCollider:
					return new TerrainCollider(assetInfo);
				case ClassIDType.TerrainData:
					return new TerrainData(assetInfo);
				case ClassIDType.LightmapSettings:
					return new LightmapSettings(assetInfo);
				case ClassIDType.EditorSettings:
					return new EditorSettings(assetInfo);
				case ClassIDType.AudioReverbZone:
					return new AudioReverbZone(assetInfo);
				case ClassIDType.OffMeshLink:
					return new OffMeshLink(assetInfo);
				case ClassIDType.OcclusionArea:
					return new OcclusionArea(assetInfo);
				case ClassIDType.NavMeshObsolete:
					return new NavMeshObsolete(assetInfo);
				case ClassIDType.NavMeshAgent:
					return new NavMeshAgent(assetInfo);
				case ClassIDType.NavMeshSettings:
					return new NavMeshSettings(assetInfo);
				case ClassIDType.ParticleSystem:
					return new ParticleSystem(assetInfo);
				case ClassIDType.ParticleSystemRenderer:
					return new ParticleSystemRenderer(assetInfo);
				case ClassIDType.ShaderVariantCollection:
					return new ShaderVariantCollection(assetInfo);
				case ClassIDType.LODGroup:
					return new LODGroup(assetInfo);
				case ClassIDType.NavMeshObstacle:
					return new NavMeshObstacle(assetInfo);
				case ClassIDType.SortingGroup:
					return new SortingGroup(assetInfo);
				case ClassIDType.SpriteRenderer:
					return new SpriteRenderer(assetInfo);
				case ClassIDType.Sprite:
					return new Sprite(assetInfo);
				case ClassIDType.ReflectionProbe:
					return new ReflectionProbe(assetInfo);
				case ClassIDType.Terrain:
					return new Terrain(assetInfo);
				case ClassIDType.AnimatorOverrideController:
					return new AnimatorOverrideController(assetInfo);
				case ClassIDType.CanvasRenderer:
					return new CanvasRenderer(assetInfo);
				case ClassIDType.Canvas:
					return new Canvas(assetInfo);
				case ClassIDType.RectTransform:
					return new RectTransform(assetInfo);
				case ClassIDType.CanvasGroup:
					return new CanvasGroup(assetInfo);
				case ClassIDType.ClusterInputManager:
					return new ClusterInputManager(assetInfo);
				case ClassIDType.NavMeshData:
					return new NavMeshData(assetInfo);
				case ClassIDType.UnityConnectSettings:
					return new UnityConnectSettings(assetInfo);
				case ClassIDType.ParticleSystemForceField:
					return new ParticleSystemForceField(assetInfo);
				case ClassIDType.OcclusionCullingData:
					return new OcclusionCullingData(assetInfo);

				case ClassIDType.PrefabInstance:
					return new PrefabInstance(assetInfo);
				case ClassIDType.TextureImporter:
					return new TextureImporter(assetInfo);
				case ClassIDType.AvatarMask:
				case ClassIDType.AvatarMaskOld:
					return new AvatarMask(assetInfo);
				case ClassIDType.DefaultAsset:
					return new DefaultAsset(assetInfo);
				case ClassIDType.DefaultImporter:
					return new DefaultImporter(assetInfo);
				case ClassIDType.SceneAsset:
					return new SceneAsset(assetInfo);
				case ClassIDType.NativeFormatImporter:
					return new NativeFormatImporter(assetInfo);
				case ClassIDType.MonoImporter:
					return new MonoImporter(assetInfo);
				case ClassIDType.DDSImporter:
					return new DDSImporter(assetInfo);
				case ClassIDType.PVRImporter:
					return new PVRImporter(assetInfo);
				case ClassIDType.ASTCImporter:
					return new ASTCImporter(assetInfo);
				case ClassIDType.KTXImporter:
					return new KTXImporter(assetInfo);
				case ClassIDType.IHVImageFormatImporter:
					return new IHVImageFormatImporter(assetInfo);
				case ClassIDType.LightmapParameters:
					return new LightmapParameters(assetInfo);
				case ClassIDType.LightingDataAsset:
					return new LightingDataAsset(assetInfo);

				case ClassIDType.SpriteAtlas:
					return new SpriteAtlas(assetInfo);
				case ClassIDType.TerrainLayer:
					return new TerrainLayer(assetInfo);
				default:
					return null;
			}
		}

		private readonly Dictionary<ClassIDType, Func<AssetInfo, Object>> m_instantiators = new Dictionary<ClassIDType, Func<AssetInfo, Object>>();
	}
}
