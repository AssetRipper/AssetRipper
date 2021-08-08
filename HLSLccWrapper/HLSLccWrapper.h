#pragma once
#include <vector>

using namespace System;

namespace HLSLccWrapper {
	public enum class WrappedGLLang { 
		LANG_DEFAULT,// Depends on the HLSL shader model.
		LANG_ES_100, LANG_ES_FIRST = LANG_ES_100,
		LANG_ES_300,
		LANG_ES_310, LANG_ES_LAST = LANG_ES_310,
		LANG_120, LANG_GL_FIRST = LANG_120,
		LANG_130,
		LANG_140,
		LANG_150,
		LANG_330,
		LANG_400,
		LANG_410,
		LANG_420,
		LANG_430,
		LANG_440, LANG_GL_LAST = LANG_440,
		LANG_METAL,
	};
	public ref class WrappedGlExtensions
	{
	public:
		uint32_t ARB_explicit_attrib_location;
		uint32_t ARB_explicit_uniform_location;
		uint32_t ARB_shading_language_420pack;
		uint32_t OVR_multiview;
		uint32_t EXT_shader_framebuffer_fetch;
	};
	public ref class Shader {
		public:
			String^ Text;
			int OK;
		static Shader^ TranslateFromFile(String^ filepath, WrappedGLLang lang, WrappedGlExtensions^ extensions);
		static Shader^ TranslateFromMem(array<unsigned char>^ data, WrappedGLLang lang, WrappedGlExtensions^ extensions);
	};
}
