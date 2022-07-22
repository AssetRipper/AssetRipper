﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectXDisassembler.Blocks
{
    public class OSGN : ShaderBlock
    {
        public int outputCount;
        public int unknown;
        public Output[] outputs;
        public OSGN(Stream stream)
        {
            fourCc = "OSGN";
            using (BinaryReader reader = new BinaryReader(stream))
            {
                outputCount = reader.ReadInt32();
                unknown = reader.ReadInt32();
                outputs = new Output[outputCount];
                for (int i = 0; i < outputCount; i++)
                {
                    Output output = new Output(reader);
                    outputs[i] = output;
                }
                reader.Align(4);
            }
        }
        public class Output
        {
            public int nameIndex;
            public string name;
            public int index;
            public int sysValue;
            public int format;
            public int register;
            public byte mask;
            public byte usedMask;
            public Output(BinaryReader reader)
            {
                nameIndex = reader.ReadInt32();
                index = reader.ReadInt32();
                sysValue = reader.ReadInt32();
                format = reader.ReadInt32();
                register = reader.ReadInt32();
                mask = reader.ReadByte();
                usedMask = reader.ReadByte();
                reader.BaseStream.Position += 2;
                long markedPos = reader.BaseStream.Position;
                reader.BaseStream.Position = nameIndex;
                name = reader.ReadNullString();
                reader.BaseStream.Position = markedPos;
            }
        }
    }
}
