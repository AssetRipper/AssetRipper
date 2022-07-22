﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Library.Exporters.Shaders
{
	public static class UnityShaderConstants
	{
		// TODO: there are more, but they aren't included by default
		public static readonly HashSet<string> BUILTIN_TEXTURE_NAMES = new HashSet<string>()
		{
			"unity_Lightmap",
			"unity_LightmapInd",
			"unity_ShadowMask",
			"unity_DynamicLightmap",
			"unity_DynamicDirectionality",
			"unity_DynamicNormal",
			"unity_SpecCube0",
			"unity_SpecCube1",
			"unity_ProbeVolumeSH",
			"_ShadowMapTexture"
		};

		// TODO: same here
		public static readonly HashSet<string> BUILTIN_CBUFFER_NAMES = new HashSet<string>()
		{
			"UnityPerCamera",
			"UnityPerCameraRare",
			"UnityLighting",
			"UnityLightingOld",
			"UnityShadows",
			"UnityPerDraw",
			"UnityStereoGlobals",
			"UnityStereoEyeIndices",
			"UnityStereoEyeIndex",
			"UnityPerDrawRare",
			"UnityPerFrame",
			"UnityFog",
			"UnityLightmaps",
			"UnityReflectionProbes",
			"UnityProbeVolume"
		};

		// these start showing up in $Globals in later versions (2020+?)
		// there's a lot so obviously incomplete
		public static readonly HashSet<string> INCLUDED_UNITY_PROP_NAMES = new HashSet<string>()
		{
			"unity_ObjectToWorld",
			"unity_WorldToObject",
			"unity_MatrixVP",
			"unity_MatrixV",
			"unity_MatrixInvV",
			"glstate_matrix_projection",
			"unity_Lightmap_HDR",
			"unity_DynamicLightmap_HDR",

			"_Time",
			"_SinTime",
			"_CosTime",
			"_ProjectionParams",
			"_ScreenParams",
			"_WorldSpaceCameraPos",
			"_WorldSpaceLightPos0",
			"_ZBufferParams",
			"_LightPositionRange"
		};

		// not in cgincludes but needed
		public static readonly HashSet<string> NONINCLUDED_UNITY_PROP_NAMES = new HashSet<string>()
		{
			"unity_LightData"
		};
	}
}
