using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Disassembler
    {
        InstructionSet instructionSet = new InstructionSet(); // I need to check how to fix this dependency
        

        public void Disassemble8080Op(byte[] codebuffer, UInt16 pc, StringBuilder disassembly)
        {

            disassembly.Append(pc.ToString("{0:X4}"));

            disassembly.Append(instructionSet.opDictionary[codebuffer[pc]].Instruction);

            if (instructionSet.opDictionary[codebuffer[pc]].OpSize == 2)
            {
                disassembly.Append(",");
                disassembly.Append(codebuffer[pc+(UInt16)1].ToString("{0:X2}"));
            }
            else if (instructionSet.opDictionary[codebuffer[pc]].OpSize == 3)
            {
                disassembly.Append(", ");
                disassembly.Append(codebuffer[pc + (UInt16)1].ToString("{0:X2}"));
                disassembly.Append(", ");
                disassembly.Append(codebuffer[pc + (UInt16)2].ToString("{0:X2}"));
            }

            disassembly.Append("\n");

            pc = (ushort)(pc + instructionSet.opDictionary[codebuffer[pc]].OpSize);
        }

        public void ReadRom()
        {
            instructionSet.populateOpDictionary(); // This should be static...
            FileStream romObj = new FileStream("invaders.h", FileMode.Open, FileAccess.Read); //change to argv
            romObj.Seek(0, SeekOrigin.Begin);
            byte[] codeBuffer = new byte[romObj.Length];
            for (int i = 0; i < romObj.Length; i++)
            {
                codeBuffer[i] = (byte) romObj.ReadByte();
            }

            UInt16 pc = 0;
            var disassembly = new StringBuilder();

            while (pc < romObj.Length)
            {
                this.Disassemble8080Op(codeBuffer, pc, disassembly);
            }
        }


    }
}
