///////////////////////////////////////////////////////////////////////////////
//                                                                           //
// D3DCompiler.cs                                                            //
// Copyright (C) Microsoft Corporation. All rights reserved.                 //
// This file is distributed under the University of Illinois Open Source     //
// License. See LICENSE.TXT for details.                                     //
//                                                                           //
///////////////////////////////////////////////////////////////////////////////

namespace D3DCompiler
{
    using DotNetDxc;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct D3D_SHADER_MACRO
    {
        [MarshalAs(UnmanagedType.LPStr)] string Name;
        [MarshalAs(UnmanagedType.LPStr)] string Definition;
    }

    internal static class D3DCompiler
    {
        [DllImport("d3dcompiler_47", CallingConvention = CallingConvention.Winapi, SetLastError = false, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public extern static Int32 D3DDisassemble(
            IntPtr ptr, uint ptrSize, uint flags,
            [MarshalAs(UnmanagedType.LPStr)] string szComments,
            out IDxcBlob disassembly);
	}
}
