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

		      /*  case 0x02: // STAX B // Welp, it seems my memory is 8 bit only. Goodbye STAX
                    registers.Memory[registers.B << 8 | registers.C] = registers.A;
                    break; */

                case 0x03: // INX B
                    answer = (ushort)(registers.B);
                    answer = (ushort)(answer << 8);
                    answer = (ushort)(answer | (ushort)registers.C);
                    answer += (ushort) 0x01;
                    registers.B = (byte)(answer >> 8 | (ushort) 0xff);
                    registers.C = (byte)(answer & (ushort)0xff);
                    break;

                case 0x0f: // RRC 	A = A >> 1; bit 7 = prev bit 0; CY = prev bit 0
                    byte x = registers.A;
                    registers.A = (byte)((((byte)x & (byte)0xff) << (byte) 0x07) | ((byte) x >> (byte) 0x01));
                    if ((registers.A > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                    break;

                case 0x2f: // CMA (not)
                    registers.A = (byte)~registers.A;
                    break;

                case 0xe6: // ANI byte
                    byte x = (byte)(registers.A & opcode[1]);

                    if (x == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                    if ((x & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                    registers.Flags.Cy = 0; // Carry Flag
                    registers.Flags.P = Intel8080Emulator.Parity(x);
                    registers.A = x;
                    registers.Pc++;
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

                case 0x88: // ADC B | A <- A + B + CY
                    answer = (ushort)((ushort)registers.A + (ushort)registers.B + (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                    if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                    if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                    if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                    registers.Flags.P = Intel8080Emulator.Parity(answer);
                    registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                    break;

                case 0xc2: // JNZ address
                    if (registers.Flags.Z == 0) registers.Pc = (ushort)((ushort)opcode[(byte)2] << 8 | (ushort)opcode[(byte)1]); 
                    else registers.Pc += (ushort)2;
                    break;

                case 0xc3: // JMP address
                    registers.Pc = (ushort)((ushort)opcode[(byte)2] << 8 | (ushort)opcode[(byte)1]);
                    break;

                case 0xcd:  // CALL address
                    ushort ret = (ushort)(registers.Pc + 2);
                    registers.Memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                    registers.Memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                    registers.Sp = (ushort)(registers.Sp - 2);
                    registers.Pc = (ushort)((((ushort) opcode[2]) << (ushort) 8) | (ushort)opcode[1]);
                    break;

                case 0xc9:  // RET
                    registers.Pc = (ushort)((ushort)registers.Memory[registers.Sp + 1] << 8 | (ushort)registers.Memory[registers.Sp]);
                    registers.Sp += 2;
                    break;

            }

            registers.Pc+=(ushort)1;

            return 1;
        }
    }
}
