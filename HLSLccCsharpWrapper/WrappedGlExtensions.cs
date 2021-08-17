using System.Runtime.InteropServices;

namespace HLSLccCsharpWrapper
{
	[StructLayout(LayoutKind.Sequential)]
	public struct WrappedGlExtensions
	{
		//Needs to be wrapped because the original is a bitfield
		public uint ARB_explicit_attrib_location;
		public uint ARB_explicit_uniform_location;
		public uint ARB_shading_language_420pack;
		public uint OVR_multiview;
		public uint EXT_shader_framebuffer_fetch;
	}
}