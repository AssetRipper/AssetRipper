using System;
using System.Collections.Generic;
using System.Text;

namespace HLSLccCsharpWrapper
{
	public enum GLLang 
	{
		/// <summary>
		/// Depends on the HLSL shader model.
		/// </summary>
		LANG_DEFAULT,
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
	}
}
