using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_320;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.ColorRGBAf;
using AssetRipper.SourceGenerated.Subclasses.FastPropertyName;
using AssetRipper.SourceGenerated.Subclasses.StaticBatchInfo;
using AssetRipper.SourceGenerated.Subclasses.UnityTexEnv;
using AssetRipper.Yaml;
using System.Globalization;

namespace AssetRipper.Tests.Traversal;

internal class DefaultYamlWalkerTests
{
	[TestCaseSource(nameof(GetObjectTypes))]
	public static void SerializedObjectIsConsistent(Type type, string yamlExpected)
	{
		UnityObjectBase asset = AssetCreator.CreateUnsafe(type);
		using (Assert.EnterMultipleScope())
		{
			string yamlActual = GenerateYaml(new DefaultYamlWalker(), asset);
			Assert.That(yamlActual, Is.EqualTo(yamlExpected));
		}
	}

	private static string GenerateYaml(YamlWalker yamlWalker, IUnityObjectBase asset)
	{
		return GenerateYaml(yamlWalker, [(asset, 1)]);
	}

	private static string GenerateYaml(YamlWalker yamlWalker, ReadOnlySpan<(IUnityObjectBase, long)> assets)
	{
		using StringWriter stringWriter = new(CultureInfo.InvariantCulture) { NewLine = "\n" };
		YamlWriter writer = new();
		writer.WriteHead(stringWriter);
		foreach ((IUnityObjectBase asset, long exportID) in assets)
		{
			YamlDocument document = yamlWalker.ExportYamlDocument(asset, exportID);
			writer.WriteDocument(document);
		}
		writer.WriteTail(stringWriter);
		return stringWriter.ToString();
	}

	private static object?[][] GetObjectTypes() =>
	[
		[typeof(ComponentListObject), ComponentListObject.Yaml],
		[typeof(DictionaryObject), DictionaryObject.Yaml],
		[typeof(GuidDictionaryObject), GuidDictionaryObject.Yaml],
		[typeof(ListObject), ListObject.Yaml],
		[typeof(PairListObject), PairListObject.Yaml],
		[typeof(PairObject), PairObject.Yaml],
		[typeof(ParentObject), ParentObject.Yaml],
		[typeof(PrimitiveListObject), PrimitiveListObject.Yaml],
		[typeof(SerializedVersionObject), SerializedVersionObject.Yaml],
		[typeof(SimpleObject), SimpleObject.Yaml],
		[typeof(StringDictionaryObject), StringDictionaryObject.Yaml],
		[typeof(SubclassObject), SubclassObject.Yaml],
		[typeof(StaticSquaredDictionaryObject), StaticSquaredDictionaryObject.Yaml],
	];

	private class DefaultYamlWalker : YamlWalker
	{
		public override YamlNode CreateYamlNodeForPPtr<TAsset>(PPtr<TAsset> pptr)
		{
			return new YamlMappingNode(MappingStyle.Flow)
			{
				{ "m_FileID", pptr.FileID },
				{ "m_PathID", pptr.PathID },
			};
		}
	}

	[Test]
	public void MonoBehaviourStructureSerializationTest()
	{
		const string yamlExpected = """
			%YAML 1.1
			%TAG !u! tag:unity3d.com,2011:
			--- !u!0 &1
			MonoBehaviour:
			  m_ObjectHideFlags: 0
			  m_PrefabParentObject: {m_FileID: 0, m_PathID: 0}
			  m_PrefabInternal: {m_FileID: 0, m_PathID: 0}
			  m_GameObject: {m_FileID: 0, m_PathID: 0}
			  m_Enabled: 0
			  m_EditorHideFlags: 0
			  m_Script: {m_FileID: 0, m_PathID: 0}
			  m_Name:
			  m_EditorClassIdentifier:
			  firstSubMesh: 0
			  subMeshCount: 0

			""";
		MonoBehaviour_2017_3 monoBehaviour = AssetCreator.CreateUnsafe<MonoBehaviour_2017_3>();
		monoBehaviour.Structure = new StaticBatchInfo();
		string yamlActual = GenerateYaml(new DefaultYamlWalker(), monoBehaviour);
		Assert.That(yamlActual, Is.EqualTo(yamlExpected));
	}

	[Test]
	public void MultipleMonoBehaviourStructureSerializationTest()
	{
		const string yamlExpected = """
			%YAML 1.1
			%TAG !u! tag:unity3d.com,2011:
			--- !u!0 &1
			MonoBehaviour:
			  m_ObjectHideFlags: 0
			  m_PrefabParentObject: {m_FileID: 0, m_PathID: 0}
			  m_PrefabInternal: {m_FileID: 0, m_PathID: 0}
			  m_GameObject: {m_FileID: 0, m_PathID: 0}
			  m_Enabled: 0
			  m_EditorHideFlags: 0
			  m_Script: {m_FileID: 0, m_PathID: 0}
			  m_Name:
			  m_EditorClassIdentifier:
			  firstSubMesh: 0
			  subMeshCount: 0
			--- !u!0 &2
			MonoBehaviour:
			  m_ObjectHideFlags: 0
			  m_PrefabParentObject: {m_FileID: 0, m_PathID: 0}
			  m_PrefabInternal: {m_FileID: 0, m_PathID: 0}
			  m_GameObject: {m_FileID: 0, m_PathID: 0}
			  m_Enabled: 0
			  m_EditorHideFlags: 0
			  m_Script: {m_FileID: 0, m_PathID: 0}
			  m_Name:
			  m_EditorClassIdentifier:
			  firstSubMesh: 0
			  subMeshCount: 0

			""";
		MonoBehaviour_2017_3 monoBehaviour = AssetCreator.CreateUnsafe<MonoBehaviour_2017_3>();
		monoBehaviour.Structure = new StaticBatchInfo();
		string yamlActual = GenerateYaml(new DefaultYamlWalker(), [(monoBehaviour, 1), (monoBehaviour, 2)]);
		Assert.That(yamlActual, Is.EqualTo(yamlExpected));
	}

	[Test]
	public void MaterialSerializationTest_3_5()
	{
		const string yamlExpected = """
			%YAML 1.1
			%TAG !u! tag:unity3d.com,2011:
			--- !u!0 &1
			Material:
			  serializedVersion: 3
			  m_ObjectHideFlags: 0
			  m_PrefabParentObject: {m_FileID: 0, m_PathID: 0}
			  m_PrefabInternal: {m_FileID: 0, m_PathID: 0}
			  m_Name:
			  m_Shader: {m_FileID: 0, m_PathID: 0}
			  m_SavedProperties:
			    serializedVersion: 2
			    m_TexEnvs:
			      data:
			        first:
			          name: _MainTex
			        second:
			          m_Texture: {m_FileID: 0, m_PathID: 0}
			          m_Scale: {x: 1, y: 1}
			          m_Offset: {x: 0, y: 0}
			    m_Floats: {}
			    m_Colors:
			      data:
			        first:
			          name: _Color
			        second: {r: 1, g: 1, b: 1, a: 1}

			""";
		Material_3_5 material = AssetCreator.CreateUnsafe<Material_3_5>();

		// Texture
		{
			(FastPropertyName name, UnityTexEnv_3_5 textureEnv) = material.SavedProperties_C21.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_5!.AddNew();
			name.Name = "_MainTex";
			textureEnv.Scale.X = 1f;
			textureEnv.Scale.Y = 1f;
		}

		// Color
		{
			(FastPropertyName name, ColorRGBAf color) = material.SavedProperties_C21.Colors_AssetDictionary_FastPropertyName_ColorRGBAf!.AddNew();
			name.Name = "_Color";
			color.SetAsWhite();
		}

		string yamlActual = GenerateYaml(new DefaultYamlWalker().WithUnityVersion(new UnityVersion(3, 5, 6)), material);
		Assert.That(yamlActual, Is.EqualTo(yamlExpected));
	}

	[Test]
	public void MaterialSerializationTest_5_3_8()
	{
		const string yamlExpected = """
			%YAML 1.1
			%TAG !u! tag:unity3d.com,2011:
			--- !u!0 &1
			Material:
			  serializedVersion: 6
			  m_ObjectHideFlags: 0
			  m_PrefabParentObject: {m_FileID: 0, m_PathID: 0}
			  m_PrefabInternal: {m_FileID: 0, m_PathID: 0}
			  m_Name:
			  m_Shader: {m_FileID: 0, m_PathID: 0}
			  m_ShaderKeywords:
			  m_LightmapFlags: 0
			  m_CustomRenderQueue: 0
			  stringTagMap: {}
			  m_SavedProperties:
			    serializedVersion: 2
			    m_TexEnvs:
			      data:
			        first:
			          name: _MainTex
			        second:
			          m_Texture: {m_FileID: 0, m_PathID: 0}
			          m_Scale: {x: 1, y: 1}
			          m_Offset: {x: 0, y: 0}
			    m_Floats: {}
			    m_Colors:
			      data:
			        first:
			          name: _Color
			        second: {r: 1, g: 1, b: 1, a: 1}

			""";
		Material_5_1 material = AssetCreator.CreateUnsafe<Material_5_1>();

		// Texture
		{
			(FastPropertyName name, UnityTexEnv_5 textureEnv) = material.SavedProperties_C21.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_5!.AddNew();
			name.Name = "_MainTex";
			textureEnv.Scale.X = 1f;
			textureEnv.Scale.Y = 1f;
		}

		// Color
		{
			(FastPropertyName name, ColorRGBAf color) = material.SavedProperties_C21.Colors_AssetDictionary_FastPropertyName_ColorRGBAf!.AddNew();
			name.Name = "_Color";
			color.SetAsWhite();
		}

		string yamlActual = GenerateYaml(new DefaultYamlWalker().WithUnityVersion(new UnityVersion(5, 3, 8)), material);
		Assert.That(yamlActual, Is.EqualTo(yamlExpected));
	}

	[Test]
	public void MaterialSerializationTest_5_4()
	{
		const string yamlExpected = """
			%YAML 1.1
			%TAG !u! tag:unity3d.com,2011:
			--- !u!0 &1
			Material:
			  serializedVersion: 6
			  m_ObjectHideFlags: 0
			  m_PrefabParentObject: {m_FileID: 0, m_PathID: 0}
			  m_PrefabInternal: {m_FileID: 0, m_PathID: 0}
			  m_Name:
			  m_Shader: {m_FileID: 0, m_PathID: 0}
			  m_ShaderKeywords:
			  m_LightmapFlags: 0
			  m_CustomRenderQueue: 0
			  stringTagMap: {}
			  m_SavedProperties:
			    serializedVersion: 2
			    m_TexEnvs:
			    - first:
			        name: _MainTex
			      second:
			        m_Texture: {m_FileID: 0, m_PathID: 0}
			        m_Scale: {x: 1, y: 1}
			        m_Offset: {x: 0, y: 0}
			    m_Floats: {}
			    m_Colors:
			    - first:
			        name: _Color
			      second: {r: 1, g: 1, b: 1, a: 1}

			""";
		Material_5_1 material = AssetCreator.CreateUnsafe<Material_5_1>();

		// Texture
		{
			(FastPropertyName name, UnityTexEnv_5 textureEnv) = material.SavedProperties_C21.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_5!.AddNew();
			name.Name = "_MainTex";
			textureEnv.Scale.X = 1f;
			textureEnv.Scale.Y = 1f;
		}

		// Color
		{
			(FastPropertyName name, ColorRGBAf color) = material.SavedProperties_C21.Colors_AssetDictionary_FastPropertyName_ColorRGBAf!.AddNew();
			name.Name = "_Color";
			color.SetAsWhite();
		}

		string yamlActual = GenerateYaml(new DefaultYamlWalker().WithUnityVersion(new UnityVersion(5, 4)), material);
		Assert.That(yamlActual, Is.EqualTo(yamlExpected));
	}

	[Test]
	public void PlayableDirectorSerializationTest()
	{
		// Based on:
		// https://github.com/Unity-Technologies/Timeline-MessageMarker/blob/711db46387de66c746e9027090c2de786fe99855/Assets/TestScene.unity#L210-L231
		const string yamlExpected = """
			%YAML 1.1
			%TAG !u! tag:unity3d.com,2011:
			--- !u!0 &1
			PlayableDirector:
			  serializedVersion: 3
			  m_ObjectHideFlags: 0
			  m_CorrespondingSourceObject: {m_FileID: 0, m_PathID: 0}
			  m_PrefabInstance: {m_FileID: 0, m_PathID: 0}
			  m_PrefabAsset: {m_FileID: 0, m_PathID: 0}
			  m_GameObject: {m_FileID: 0, m_PathID: 0}
			  m_Enabled: 0
			  m_PlayableAsset: {m_FileID: 0, m_PathID: 0}
			  m_InitialState: 0
			  m_WrapMode: 0
			  m_DirectorUpdateMode: 0
			  m_InitialTime: 0
			  m_SceneBindings:
			  - key: {m_FileID: 0, m_PathID: 0}
			    value: {m_FileID: 0, m_PathID: 0}
			  - key: {m_FileID: 0, m_PathID: 0}
			    value: {m_FileID: 0, m_PathID: 0}
			  m_ExposedReferences:
			    m_References:
			    - fc1441eaed6bd5f45a945cc3d2579dd6: {m_FileID: 0, m_PathID: 0}
			    - ec546beecf692a4419584c0b9cc42a29: {m_FileID: 0, m_PathID: 0}

			""";
		PlayableDirector_2019 director = AssetCreator.CreateUnsafe<PlayableDirector_2019>();

		director.SceneBindings_C320.AddNew();
		director.SceneBindings_C320.AddNew();
		director.ExposedReferences_C320.References_Editor.AddNew().Key = "fc1441eaed6bd5f45a945cc3d2579dd6";
		director.ExposedReferences_C320.References_Editor.AddNew().Key = "ec546beecf692a4419584c0b9cc42a29";

		string yamlActual = GenerateYaml(new DefaultYamlWalker().WithUnityVersion(new UnityVersion(2019)), director);
		Assert.That(yamlActual, Is.EqualTo(yamlExpected));
	}
}
