using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderLabConvert
{
    public class UShaderProgram
    {
        public UShaderFunctionType shaderFunctionType;
        public List<USILLocal> locals;
        public List<USILInstruction> instructions;
        public List<USILInputOutput> inputs;
        public List<USILInputOutput> outputs;

        public UShaderProgram()
        {
            shaderFunctionType = UShaderFunctionType.Unknown;
            locals = new List<USILLocal>();
            instructions = new List<USILInstruction>();
            inputs = new List<USILInputOutput>();
            outputs = new List<USILInputOutput>();
        }
    }
}
