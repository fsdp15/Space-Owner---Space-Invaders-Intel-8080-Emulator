using Intel8080Emulator.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8080Emulator
{
    internal unsafe class Intel8080Emulator
    {
        public static byte Parity(ushort parity)
        {
            return (byte) parity;
        }

        public int Emulate8080Op(Registers registers)
        {
            byte* opcode = &registers.Memory[registers.Pc];
            ushort answer;

            switch (*opcode) // I can implement this with a dictionary that points to a... method? (delegate)
            {
                case 0x00: // NOP
                    break;
		        case 0x01: // LXI B
                    registers.C = opcode[(byte)1];
                    registers.B = opcode[(byte)2];
                    registers.Pc += (ushort)2;
                    break;
		        case 0x02: // STAX B
                    registers.Memory[registers.B << 8 | registers.C] = registers.A;
                    break;

                case 0x41: // MOV B,C
                    registers.B = registers.C;
                    break;

                case 0x80: // 	ADD B
                    answer = (ushort)((ushort)registers.A + (ushort)registers.B); // Higher precision to capture carry out
                    if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                    if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                    if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                    registers.Flags.P = Intel8080Emulator.Parity(answer);
                    registers.A = (byte)(answer & (ushort) 0xff); // Returning to 8 bits
                    break;

                case 0xC6: // 	ADI D8
                    answer = (ushort)((ushort)registers.A + (ushort)opcode[1]); // Higher precision to capture carry out
                    if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                    if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                    if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                    registers.Flags.P = Intel8080Emulator.Parity(answer);
                    registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                    break;

                case 0x86: // ADD M -> | A <- A + (HL)
                    ushort offset = (ushort)((ushort)(registers.H << 8) | (ushort)registers.L);
                    answer = (ushort)((ushort)registers.A + (ushort)registers.Memory[offset]); // Higher precision to capture carry out
                    if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                    if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                    if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                    registers.Flags.P = Intel8080Emulator.Parity(answer);
                    registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                    break;

            }

            registers.Pc+=(ushort)1;

            return 1;
        }
    }
}
