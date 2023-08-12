using Intel8080Emulator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Intel8080Emulator
{
    public unsafe class Disassembler
    {
        private InstructionSet instructionSet;

        public Disassembler()
        {
            InstructionSet = new();
        }

        public InstructionSet InstructionSet { get => instructionSet; set => instructionSet = value; }

        public void Disassemble8080Op(byte* codebuffer, UInt16 pc, StringBuilder disassembly)
        {

            disassembly.Append(pc.ToString("{0:X4}"));
            disassembly.Append(String.Format("0x{0:X}", pc.ToString("X4")));
            disassembly.Append("    ");
            disassembly.Append("    "); 

            disassembly.Append(InstructionSet.opDictionary[codebuffer[pc]].Instruction);

            if (InstructionSet.opDictionary[codebuffer[pc]].OpSize == 2)
            {
                disassembly.Append(",");
                disassembly.Append(codebuffer[pc + (UInt16)1].ToString("X2"));
            }
            else if (InstructionSet.opDictionary[codebuffer[pc]].OpSize == 3)
            {
                disassembly.Append(", ");
                disassembly.Append(codebuffer[pc + (UInt16)2].ToString("X2"));
                disassembly.Append(", ");
                disassembly.Append(codebuffer[pc + (UInt16)1].ToString("X2"));
            } 

			disassembly.Append(", ");
            disassembly.Append("Cycle count: " + InstructionSet.opDictionary[codebuffer[pc]].CycleCount);

			disassembly.Append("\n");

         } 

         public void Dissassembly()
        {
            StringBuilder disassembly = new();
			FileStream romObj = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "ROM\\invaders", FileMode.Open, FileAccess.Read); //change to argv
            romObj.Seek(0, SeekOrigin.Begin);
            byte[] codeBuffer = new byte[romObj.Length];

            for (int i = 0; i < romObj.Length; i++)
            {
                codeBuffer[i] = (byte)romObj.ReadByte();
            }

            UInt16 pc = 0;

            fixed (byte* opcode = &codeBuffer[0]) 
            {
                while (pc < romObj.Length)
                {
                    this.Disassemble8080Op(opcode, pc, disassembly);
                    pc = (UInt16)(pc + (UInt16)InstructionSet.opDictionary[codeBuffer[pc]].OpSize);
                }
            }

            /* using (System.IO.StreamWriter file = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\DebugLogs\\invadersDump.txt"))
            {
                file.WriteLine(disassembly.ToString());
            } */
        }


    }
}
