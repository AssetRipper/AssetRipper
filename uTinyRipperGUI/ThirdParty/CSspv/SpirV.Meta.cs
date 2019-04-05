using System;
using System.Collections.Generic;

namespace SpirV
{
    class Meta
    {
        public static uint MagicNumber
        {
            get
            {
                return 0x07230203;
            }

            set
            {
            }
        }

        public static uint Version
        {
            get
            {
                return 66048U;
            }

            set
            {
            }
        }

        public static uint Revision
        {
            get
            {
                return 2U;
            }

            set
            {
            }
        }

        public static uint OpCodeMask
        {
            get
            {
                return 65535U;
            }

            set
            {
            }
        }

        public static uint WordCountShift
        {
            get
            {
                return 16U;
            }

            set
            {
            }
        }

        public class ToolInfo
        {
            public ToolInfo(string vendor)
            {
                Vendor = vendor;
            }

            public ToolInfo(string vendor, string name)
            {
                Vendor = vendor;
                Name = name;
            }

            public String Name
            {
                get;
            }

            public String Vendor
            {
                get;
            }
        }
        private readonly static Dictionary<int, ToolInfo> toolInfos_ = new Dictionary<int, ToolInfo> { { 0, new ToolInfo("Khronos") }, { 1, new ToolInfo("LunarG") }, { 2, new ToolInfo("Valve") }, { 3, new ToolInfo("Codeplay") }, { 4, new ToolInfo("NVIDIA") }, { 5, new ToolInfo("ARM") }, { 6, new ToolInfo("Khronos", "LLVM/SPIR-V Translator") }, { 7, new ToolInfo("Khronos", "SPIR-V Tools Assembler") }, { 8, new ToolInfo("Khronos", "Glslang Reference Front End") }, { 9, new ToolInfo("Qualcomm") }, { 10, new ToolInfo("AMD") }, { 11, new ToolInfo("Intel") }, { 12, new ToolInfo("Imagination") }, { 13, new ToolInfo("Google", "Shaderc over Glslang") }, { 14, new ToolInfo("Google", "spiregg") }, { 15, new ToolInfo("Google", "rspirv") }, { 16, new ToolInfo("X-LEGEND", "Mesa-IR/SPIR-V Translator") }, { 17, new ToolInfo("Khronos", "SPIR-V Tools Linker") }, }; public static IReadOnlyDictionary<int, ToolInfo> Tools
        {
            get => toolInfos_;
        }
    }
}