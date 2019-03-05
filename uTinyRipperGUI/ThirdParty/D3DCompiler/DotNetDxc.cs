///////////////////////////////////////////////////////////////////////////////
//                                                                           //
// DotNetDxc.cs                                                              //
// Copyright (C) Microsoft Corporation. All rights reserved.                 //
// This file is distributed under the University of Illinois Open Source     //
// License. See LICENSE.TXT for details.                                     //
//                                                                           //
// Provides P/Invoke declarations for dxcompiler HLSL support.               //
//                                                                           //
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace DotNetDxc
{
    [ComImport]
    [Guid("8BA5FB08-5195-40e2-AC58-0D989C3A0102")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDxcBlob
    {
        [PreserveSig]
        IntPtr GetBufferPointer();
        [PreserveSig]
        UInt32 GetBufferSize();
    }
}
