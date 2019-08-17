#include "HLSLccWrapper.h"
#include "HLSLcc.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace HLSLccWrapper {
	Shader^ Shader::TranslateFromFile(String^ filepath, WrappedGLLang lang, WrappedGlExtensions^ extensions) {
		Shader^ shader = gcnew Shader();
		
		IntPtr p = Marshal::StringToHGlobalAnsi(filepath);
		const char* _filename = static_cast<char*>(p.ToPointer());
		unsigned int flags = HLSLCC_FLAG_UNIFORM_BUFFER_OBJECT;
		GLLang language = GLLang::LANG_DEFAULT;
		GlExtensions ext;
		ext.ARB_explicit_attrib_location = extensions->ARB_explicit_attrib_location;
		ext.ARB_explicit_uniform_location = extensions->ARB_explicit_uniform_location;
		ext.ARB_shading_language_420pack = extensions->ARB_shading_language_420pack;
		ext.OVR_multiview = extensions->OVR_multiview;
		ext.EXT_shader_framebuffer_fetch = extensions->EXT_shader_framebuffer_fetch;

		HLSLccSamplerPrecisionInfo samplerPrecisions;
		HLSLccReflection reflectionCallbacks;
		GLSLCrossDependencyData dependencies;
		GLSLShader result;
		int compiledOK = TranslateHLSLFromFile(
			_filename,
			flags,
			language,
			&ext,
			nullptr,
			samplerPrecisions,
			reflectionCallbacks,
			&result
		);
		shader->OK = compiledOK;
		if (compiledOK) {
			shader->Text = gcnew String(result.sourceCode.c_str());
		}
		Marshal::FreeHGlobal(p);
		return shader;
	}
	Shader^ Shader::TranslateFromMem(array<unsigned char>^ data, WrappedGLLang lang, WrappedGlExtensions^ extensions) {
		Shader^ shader = gcnew Shader();

		IntPtr p = Marshal::AllocHGlobal(data->Length);
		Marshal::Copy(data, 0, p, data->Length);
		const char* unmanagedData = static_cast<char*>(p.ToPointer());
		unsigned int flags = HLSLCC_FLAG_UNIFORM_BUFFER_OBJECT;
		GLLang language = GLLang::LANG_DEFAULT;
		GlExtensions ext;
		ext.ARB_explicit_attrib_location = extensions->ARB_explicit_attrib_location;
		ext.ARB_explicit_uniform_location = extensions->ARB_explicit_uniform_location;
		ext.ARB_shading_language_420pack = extensions->ARB_shading_language_420pack;
		ext.OVR_multiview = extensions->OVR_multiview;
		ext.EXT_shader_framebuffer_fetch = extensions->EXT_shader_framebuffer_fetch;

		HLSLccSamplerPrecisionInfo samplerPrecisions;
		HLSLccReflection reflectionCallbacks;
		GLSLCrossDependencyData dependencies;
		GLSLShader result;
		int compiledOK = TranslateHLSLFromMem(
			unmanagedData,
			flags,
			language,
			&ext,
			nullptr,
			samplerPrecisions,
			reflectionCallbacks,
			&result
		);
		shader->OK = compiledOK;
		if (compiledOK) {
			shader->Text = gcnew String(result.sourceCode.c_str());
		}
		Marshal::FreeHGlobal(p);
		return shader;
	}
}