#include "HLSLccWrapper.h"
#include "hlslcc.h"

std::string ShaderTranslateFromFile(std::string filepath, GLLang language, WrappedGlExtensions extensions) {
	const char* _filename = filepath.c_str();
	unsigned int flags = HLSLCC_FLAG_UNIFORM_BUFFER_OBJECT;

	GlExtensions ext;
	ext.ARB_explicit_attrib_location = extensions.ARB_explicit_attrib_location;
	ext.ARB_explicit_uniform_location = extensions.ARB_explicit_uniform_location;
	ext.ARB_shading_language_420pack = extensions.ARB_shading_language_420pack;
	ext.EXT_shader_framebuffer_fetch = extensions.EXT_shader_framebuffer_fetch;
	ext.OVR_multiview = extensions.OVR_multiview;

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
	if (compiledOK) {
		return result.sourceCode;
	}
	return {};
}
std::string ShaderTranslateFromMem(unsigned char data[], GLLang lang, WrappedGlExtensions extensions) {
	const char* unmanagedData = (char*)&data;//<=========================== I'm concerned about that cast
	unsigned int flags = HLSLCC_FLAG_UNIFORM_BUFFER_OBJECT;
	GLLang language = (GLLang)lang;

	GlExtensions ext;
	ext.ARB_explicit_attrib_location = extensions.ARB_explicit_attrib_location;
	ext.ARB_explicit_uniform_location = extensions.ARB_explicit_uniform_location;
	ext.ARB_shading_language_420pack = extensions.ARB_shading_language_420pack;
	ext.EXT_shader_framebuffer_fetch = extensions.EXT_shader_framebuffer_fetch;
	ext.OVR_multiview = extensions.OVR_multiview;

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
	if (compiledOK) {
		return result.sourceCode;
	}
	return {};
}
