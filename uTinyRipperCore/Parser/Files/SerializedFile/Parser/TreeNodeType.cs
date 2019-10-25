using System;
using System.Collections.Generic;
using System.Reflection;

namespace uTinyRipper.SerializedFiles
{
	/// <summary>
	/// Predefines types
	/// Value equals to string offset (in common buffer)
	/// </summary>
	public enum TreeNodeType
	{
		AABB						= 0,
		AnimationClip				= 5,
		AnimationCurve				= 19,
		AnimationState				= 34,
		Array						= 49,
		Base						= 55,
		BitField					= 60,
		bitset						= 69,
		@bool						= 76,
		@char						= 81,
		ColorRGBA					= 86,
		Component					= 96,
		data						= 106,
		deque						= 111,
		@double						= 117,
		dynamic_array				= 124,
		FastPropertyName			= 138,
		first						= 155,
		@float						= 161,
		Font						= 167,
		GameObject					= 172,
		[EnumName("Generic Mono")]
		GenericMono					= 183,
		GradientNEW					= 196,
		GUID						= 208,
		GUIStyle					= 213,
		@int						= 222,
		list						= 226,
		[EnumName("long long")]
		longlong					= 231,
		map							= 241,
		Matrix4x4f					= 245,
		MdFour						= 256,
		MonoBehaviour				= 263,
		MonoScript					= 277,
		m_ByteSize					= 288,
		m_Curve						= 299,
		m_EditorClassIdentifier		= 307,
		m_EditorHideFlags			= 331,
		m_Enabled					= 349,
		m_ExtensionPtr				= 359,
		m_GameObject				= 374,
		m_Index						= 387,
		m_IsArray					= 395,
		m_IsStatic					= 405,
		m_MetaFlag					= 416,
		m_Name						= 427,
		m_ObjectHideFlags			= 434,
		m_PrefabInternal			= 452,
		m_PrefabParentObject		= 469,
		m_Script					= 490,
		m_StaticEditorFlags			= 499,
		m_Type						= 519,
		m_Version					= 526,
		Object						= 536,
		pair						= 543,
		[EnumName("PPtr<Component>")]
		PPtrComponent				= 548,
		[EnumName("PPtr<GameObject>")]
		PPtrGameObject				= 564,
		[EnumName("PPtr<Material>")]
		PPtrMaterial				= 581,
		[EnumName("PPtr<MonoBehaviour>")]
		PPtrMonoBehaviour			= 596,
		[EnumName("PPtr<MonoScript>")]
		PPtrMonoScript				= 616,
		[EnumName("PPtr<Object>")]
		PPtrObject					= 633,
		[EnumName("PPtr<Prefab>")]
		PPtrPrefab					= 646,
		[EnumName("PPtr<Sprite>")]
		PPtrSprite					= 659,
		[EnumName("PPtr<TextAsset>")]
		PPtrTextAsset				= 672,
		[EnumName("PPtr<Texture>")]
		PPtrTexture					= 688,
		[EnumName("PPtr<Texture2D>")]
		PPtrTexture2D				= 702,
		[EnumName("PPtr<Transform>")]
		PPtrTransform				= 718,
		Prefab						= 734,
		Quaternionf					= 741,
		Rectf						= 753,
		RectInt						= 759,
		RectOffset					= 767,
		second						= 778,
		set							= 785,
		@short						= 789,
		size						= 795,
		SInt16						= 800,
		SInt32						= 807,
		SInt64						= 814,
		SInt8						= 821,
		staticvector				= 827,
		@string						= 840,
		TextAsset					= 847,
		TextMesh					= 857,
		Texture						= 866,
		Texture2D					= 874,
		Transform					= 884,
		TypelessData				= 894,
		UInt16						= 907,
		UInt32						= 914,
		UInt64						= 921,
		UInt8						= 928,
		[EnumName("unsigned int")]
		unsignedint					= 934,
		[EnumName("unsigned long long")]
		unsignedlonglong			= 947,
		[EnumName("unsigned short")]
		unsignedshort				= 966,
		vector						= 981,
		Vector2f					= 988,
		Vector3f					= 997,
		Vector4f					= 1006,
		m_ScriptingClassIdentifier	= 1015,
		Gradient					= 1042,
		[EnumName("Type*")]
		TypeStar					= 1051,
		int2_storage				= 1057,
		int3_storage				= 1070,
		BoundsInt					= 1083,
		m_CorrespondingSourceObject	= 1093,
		m_PrefabInstance			= 1121,
		m_PrefabAsset				= 1138,
	}

	public static class TreeNodeTypeExtentions
	{
		static TreeNodeTypeExtentions()
		{
			int index = 0;
			Type type = typeof(TreeNodeType);
			TreeNodeType[] values = (TreeNodeType[])Enum.GetValues(type);
			IEnumerable<FieldInfo> fields = type.GetRuntimeFields();
			Dictionary<TreeNodeType, string> typeNames = new Dictionary<TreeNodeType, string>(values.Length);
			foreach (FieldInfo field in fields)
			{
				if(index > 0)
				{
					EnumNameAttribute name = field.GetCustomAttribute<EnumNameAttribute>();
					typeNames.Add(values[index - 1], name == null ? values[index - 1].ToString() : name.Name);
				}
				index++;
			}
			s_typeNames = typeNames;
		}

		public static string ToTypeString(this TreeNodeType _this)
		{
			return s_typeNames[_this];
		}

		private static readonly IReadOnlyDictionary<TreeNodeType, string> s_typeNames;
	}
}
