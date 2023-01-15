using ConsoleApp1;
using Intel8080Emulator.Exceptions;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8080Emulator
{
    internal unsafe class Intel8080Emulator
    {
        private StringBuilder emulationLog;
        private Disassembler disassembler;
        // I need to check how to inject this dependency

        public Intel8080Emulator()
        {
            emulationLog = new();
            disassembler = new();
        }

        public void ProcessorState(Registers registers)
        {
            Console.WriteLine();
            emulationLog.Append("Carry flag: "); emulationLog.Append(registers.Flags.Cy.ToString()); emulationLog.Append("\n");
            emulationLog.Append("Parity flag: "); emulationLog.Append(registers.Flags.P.ToString()); emulationLog.Append("\n");
            emulationLog.Append("Sign flag: "); emulationLog.Append(registers.Flags.S.ToString()); emulationLog.Append("\n");
            emulationLog.Append("Zero flag: "); emulationLog.Append(registers.Flags.Z.ToString()); emulationLog.Append("\n");
            Console.WriteLine();
            emulationLog.Append(String.Format("A: $0x{0:X}; B: $0x{1:X}; C: $0x{2:X}; D: $0x{3:X}; E: $0x{4:X}; " +
                "H: $0x{5:X}; L: $0x{6:X}; SP: $0x{7:X}\n", registers.A.ToString("X2"), registers.B.ToString("X2"),
                registers.C.ToString("X2"), registers.D.ToString("X2"), registers.E.ToString("X2"), registers.H.ToString("X2"),
                registers.L.ToString("X2"), registers.Sp.ToString("X4")));
            Console.WriteLine();
        }

        public void ReadRom()
        {
            FileStream romObj = new FileStream("C:\\Users\\felip\\OneDrive\\Desktop\\Emulator\\invaders\\invaders", FileMode.Open, FileAccess.Read); //change to argv
            romObj.Seek(0, SeekOrigin.Begin);
            Registers registers = new();
            registers.Pc = 0;

            Console.WriteLine();

            for (int i = 0; i < romObj.Length; i++)
            {
                registers.memory[i] = (byte)romObj.ReadByte();
            }

            Console.WriteLine();

            while (1 == 1)
            {
                Console.WriteLine();
                this.Emulate8080Op(registers);

                Console.WriteLine();

                using (System.IO.StreamWriter file = File.AppendText("C:\\Users\\felip\\OneDrive\\Desktop\\Emulator\\invaders\\invadersDebug.txt"))
                {
                    file.WriteLine(emulationLog.ToString());
                }
                emulationLog.Clear();
            }
        }



        public static byte Parity(ushort parity)
        {
            return (byte) 0;
        }

        public int Emulate8080Op(Registers registers)
        {
            fixed (byte* opcode = &registers.memory[registers.Pc])
            {
                ushort answer;
                byte x;
                byte psw;
                UInt32 aux1;
                UInt32 aux2;

                Console.WriteLine();
                fixed (byte* codebuffer = &registers.memory[0])
                {
                    disassembler.Disassemble8080Op(codebuffer, registers.Pc, emulationLog);
                }
                Console.WriteLine();
                emulationLog.Append(String.Format("Opcode: $0x{0:X}\n", opcode[0].ToString("X2")));

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
                          registers.memory[registers.B << 8 | registers.C] = registers.A;
                          break; */

                    case 0x02:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));

                    case 0x03: // INX B
                        answer = (ushort)(registers.B);
                        answer = (ushort)(answer << 8);
                        answer = (ushort)(answer | (ushort)registers.C);
                        answer += (ushort)0x01;
                        registers.B = (byte)(answer >> 8 | (ushort)0xff);
                        registers.C = (byte)(answer & (ushort)0xff);
                        break;

                    case 0x04:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x05: // 	DCR B -- 	B <- B-1
                        registers.B--;
                        if ((registers.B & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.B & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.B);
                        break;

                    case 0x06: // MVI B, D8 -- B <- byte 2
                        registers.B = opcode[1];
                        registers.Pc++;                 
                        break;

                    case 0x07:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x08:
                        break;

                    case 0x09:
                        aux1 = (UInt32)(((UInt32)registers.H << 8) | ((UInt32)registers.L));
                        aux2 = (UInt32)(((UInt32)registers.B << 8) | ((UInt32)registers.C));
                        aux2 = (UInt32)(aux1 + aux2);
                        if ((aux2 > (UInt32)0xffff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        aux2 = (ushort)(aux2 & (ushort)0xffff); // Returning to 16 bits
                        registers.H = ((byte)((ushort)(aux2 & 0xff00) >> 8));
                        registers.L = ((byte)((ushort)(aux2 & 0x00ff)));
                        break;

                    case 0x0a:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x0b:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x0c:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x0d:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x0e:
                        registers.C = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x0f: // RRC 	A = A >> 1; bit 7 = prev bit 0; CY = prev bit 0
                        x = registers.A;
                        registers.A = (byte)((((byte)x & (byte)0x01) << (byte)0x07) | ((byte)x >> (byte)0x01));
                        if ((x & 0x01) == 0x01) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        break;

                    case 0x10:
                        break;

                    case 0x11: // 	LXI D,D16 -- D <- byte 3, E <- byte 2
                        registers.E = opcode[1];
                        registers.D = opcode[2];                 
                        registers.Pc += 2;
                        break;

                    case 0x12:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x13: // 	INX D -- 	DE <- DE + 1
                        answer = (ushort)(((ushort)registers.D << 8) | (ushort)registers.E);
                        answer++;
                        registers.D = (byte)((answer & 0xff00) >> 8);
                        registers.E = (byte)((answer & 0x00ff));
                        break;

                    case 0x14:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x15:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x16:
                        registers.D = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x17:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x18:
                        break;

                    case 0x19:
                        aux1 = (UInt32)(((UInt32)registers.H << 8) | ((UInt32)registers.L));
                        aux2 = (UInt32)(((UInt32)registers.D << 8) | ((UInt32)registers.E));
                        aux2 = (UInt32)(aux1 + aux2);
                        if ((aux2 > (UInt32)0xffff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        aux2 = (ushort)(aux2 & (ushort)0xffff); // Returning to 16 bits
                        registers.H = ((byte)((ushort)(aux2 & 0xff00) >> 8));
                        registers.L = ((byte)((ushort)(aux2 & 0x00ff)));
                        break;

                    case 0x1a:  // LDAX D -- A <- (DE)
                        registers.A = registers.memory[((ushort)registers.D << 8) | (ushort)registers.E];
                        break;

                    case 0x1b:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x1c:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x1d:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x1e:
                        registers.E = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x1f: // RAR 	A = A >> 1; bit 7 = prev bit 7; CY = prev bit 0
                        x = registers.A;
                        registers.A = (byte)((registers.Flags.Cy << 0x07) | (x >> 0x01));
                        if ((x & 0x01) == 0x01) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        break;

                    case 0x20:
                        break;

                    case 0x21:  // 	LXI H,D16 -- 	H <- byte 3, L <- byte 2
                        registers.L = opcode[1];
                        registers.H = opcode[2];
                        registers.Pc += 2;
                        break;

                    case 0x22:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x23: // INX H -- HL <- HL + 1
                        answer = (ushort)(((ushort)registers.H << 8) | (ushort)registers.L);
                        answer++;
                        registers.H = (byte)((answer & 0xff00) >> 8);
                        registers.L = (byte)((answer & 0x00ff));
                        break;

                    case 0x24:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x25:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x26:
                        registers.H = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x27:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x28:
                        break;

                    case 0x29: //DAD H -> HL = HL + HL
                        aux1 = (UInt32)(((UInt32)registers.H << 8) | ((UInt32)registers.L)); // 32 bits variable to hold carry flag
                        aux2 = ((UInt32) aux1) << 1;
                        if ((aux2 > (UInt32)0xffff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        aux2 = (ushort)(aux2 & (ushort)0xffff); // Returning to 16 bits
                        registers.H = ((byte)((ushort)(aux2 & 0xff00) >> 8));
                        registers.L = ((byte)((ushort)(aux2 & 0x00ff)));
                        break;

                    case 0x2a:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x2b:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x2c:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x2d:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x2e:
                        registers.L = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x2f: // CMA (not)
                        registers.A = (byte)~registers.A;
                        break;

                    case 0x30:
                        break;

                    case 0x31: // 	LXI SP, D16 //	SP.hi <- byte 3, SP.lo <- byte 2
                        registers.Sp = (ushort)((ushort)opcode[2] << 8);
                        registers.Sp = (ushort)(registers.Sp | (ushort)opcode[1]);
                        registers.Pc += (ushort)2;
                        break;

                    case 0x32:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x33:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x34:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x35:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x36: // 	MVI M,D8 -- (HL) <- byte 2
                        registers.memory[((ushort)registers.H << 8) | (ushort)registers.L] = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x37:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x38:
                        break;

                    case 0x39:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x3a:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x3b:
                        aux1 = (UInt32)(((UInt32)registers.H << 8) | ((UInt32)registers.L));
                        aux2 = (UInt32)(aux1 + (UInt32)registers.Sp);
                        if ((aux2 > (UInt32)0xffff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        aux2 = (ushort)(aux2 & (ushort)0xffff); // Returning to 16 bits
                        registers.H = ((byte)((ushort)(aux2 & 0xff00) >> 8));
                        registers.L = ((byte)((ushort)(aux2 & 0x00ff)));
                        break;

                    case 0x3c:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x3d:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x3e:
                        registers.A = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x3f:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x40:
                        registers.B = registers.B;
                        break;

                    case 0x41: // MOV B,C
                        registers.B = registers.C;
                        break;

                    case 0x42:
                        registers.B = registers.D;
                        break;

                    case 0x43:
                        registers.B = registers.E;
                        break;

                    case 0x44:
                        registers.B = registers.H;
                        break;

                    case 0x45:
                        registers.B = registers.L;
                        break;

                    case 0x46:
                        registers.B = registers.memory[((ushort)registers.H << 8) | (ushort)registers.L];
                        break;

                    case 0x47:
                        registers.B = registers.A;
                        break;

                    case 0x48:
                        registers.C = registers.B;
                        break;

                    case 0x49:
                        registers.C = registers.C;
                        break;

                    case 0x4a:
                        registers.C = registers.D;
                        break;

                    case 0x4b:
                        registers.C = registers.E;
                        break;

                    case 0x4c:
                        registers.C = registers.H;
                        break;

                    case 0x4d:
                        registers.C = registers.L;
                        break;

                    case 0x4e:
                        registers.C = registers.memory[((ushort)registers.H << 8) | (ushort)registers.L];
                        break;

                    case 0x4f:
                        registers.C = registers.A;
                        break;

                    case 0x50:
                        registers.D = registers.B;
                        break;

                    case 0x51:
                        registers.D = registers.C;
                        break;

                    case 0x52:
                        registers.D = registers.D;
                        break;

                    case 0x53:
                        registers.D = registers.E;
                        break;

                    case 0x54:
                        registers.D = registers.H;
                        break;

                    case 0x55:
                        registers.D = registers.L;
                        break;

                    case 0x56:
                        registers.D = registers.memory[((ushort)registers.H << 8) | (ushort)registers.L];
                        break;

                    case 0x57:
                        registers.D = registers.A;
                        break;

                    case 0x58:
                        registers.E = registers.B;
                        break;

                    case 0x59:
                        registers.E = registers.C;
                        break;

                    case 0x5a:
                        registers.E = registers.D;
                        break;

                    case 0x5b:
                        registers.E = registers.E;
                        break;

                    case 0x5c:
                        registers.E = registers.H;
                        break;

                    case 0x5d:
                        registers.E = registers.L;
                        break;

                    case 0x5e:
                        registers.E = registers.memory[((ushort)registers.H << 8) | (ushort)registers.L];
                        break;

                    case 0x5f:
                        registers.E = registers.A;
                        break;

                    case 0x60:
                        registers.H = registers.B;
                        break;

                    case 0x61:
                        registers.H = registers.C;
                        break;

                    case 0x62:
                        registers.H = registers.D;
                        break;

                    case 0x63:
                        registers.H = registers.E;
                        break;

                    case 0x64:
                        registers.H = registers.H;
                        break;

                    case 0x65:
                        registers.H = registers.L;
                        break;

                    case 0x66:
                        registers.H = registers.memory[((ushort)registers.H << 8) | (ushort)registers.L];
                        break;

                    case 0x67:
                        registers.H = registers.A;
                        break;

                    case 0x68:
                        registers.L = registers.B;
                        break;

                    case 0x69:
                        registers.L = registers.C;
                        break;

                    case 0x6a:
                        registers.L = registers.D;
                        break;

                    case 0x6b:
                        registers.L = registers.E;
                        break;

                    case 0x6c:
                        registers.L = registers.H;
                        break;

                    case 0x6d:
                        registers.L = registers.L;
                        break;

                    case 0x6e:
                        registers.L = registers.memory[((ushort)registers.H << 8) | (ushort)registers.L];
                        break;

                    case 0x6f:
                        registers.L = registers.A;
                        break;

                    case 0x70:
                        registers.memory[((ushort)registers.H << 8) | (ushort)registers.L] = registers.B;
                        break;

                    case 0x71:
                        registers.memory[((ushort)registers.H << 8) | (ushort)registers.L] = registers.C;
                        break;

                    case 0x72:
                        registers.memory[((ushort)registers.H << 8) | (ushort)registers.L] = registers.D;
                        break;

                    case 0x73:
                        registers.memory[((ushort)registers.H << 8) | (ushort)registers.L] = registers.E;
                        break;

                    case 0x74:
                        registers.memory[((ushort)registers.H << 8) | (ushort)registers.L] = registers.H;
                        break;

                    case 0x75:
                        registers.memory[((ushort)registers.H << 8) | (ushort)registers.L] = registers.L;
                        break;

                    case 0x76: //HALT
                        return -1;

                    case 0x77: // MOV M,A
                        registers.memory[((ushort)registers.H << 8) | (ushort)registers.L] = registers.A;
                        break;

                    case 0x78:
                        registers.A = registers.B;
                        break;

                    case 0x79:
                        registers.A = registers.C;
                        break;

                    case 0x7a:
                        registers.A = registers.D;
                        break;

                    case 0x7b:
                        registers.A = registers.E;
                        break;

                    case 0x7c:
                        registers.A = registers.H;
                        break;

                    case 0x7d:
                        registers.A = registers.L;
                        break;

                    case 0x7e:
                        registers.A = registers.memory[((ushort)registers.H << 8) | (ushort)registers.L];
                        break;

                    case 0x7f:
                        registers.A = registers.A;
                        break;

                    case 0x80: // 	ADD B
                        answer = (ushort)((ushort)registers.A + (ushort)registers.B); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity(answer);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x81:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x82:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x83:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x84:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x85:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x86: // ADD M -> | A <- A + (HL)
                        ushort offset = (ushort)((ushort)(registers.H << 8) | (ushort)registers.L);
                        answer = (ushort)((ushort)registers.A + (ushort)registers.memory[offset]); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity(answer);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x87:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x88: // ADC B | A <- A + B + CY
                        answer = (ushort)((ushort)registers.A + (ushort)registers.B + (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity(answer);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x89:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;


                    case 0x8a:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x8b:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x8c:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x8d:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x8e:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x8f:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x90:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x91:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x92:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x93:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x94:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x95:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x96: //HALT
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x97:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x98:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x99:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x9a:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x9b:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x9c:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x9d:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x9e:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0x9f:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xa0:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xa1:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xa2:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xa3:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xa4:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xa5:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xa6: //HALT
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xa7:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xa8:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xa9:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xaa:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xab:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xac:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xad:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xae:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xaf:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xb0:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xb1:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xb2:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xb3:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xb4:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xb5:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xb6: //HALT
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xb7:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xb8:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xb9:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xba:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xbb:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xbc:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xbd:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xbe:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xbf:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xc0:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xc1:  // POP B
                        registers.C = registers.memory[registers.Sp];
                        registers.B = registers.memory[registers.Sp + 1];
                        registers.Sp += 2;
                        break;

                    case 0xc2: // JNZ address
                        if (registers.Flags.Z == 0)
                        {
                            registers.Pc = (ushort)((ushort)opcode[(byte)2] << 8 | (ushort)opcode[(byte)1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xc3: // JMP address
                        registers.Pc = (ushort)((ushort)opcode[(byte)2] << 8 | (ushort)opcode[(byte)1]);
                        registers.Pc--; //Workaround because of the pc== at the end. Will think of a better logic later
                        break;

                    case 0xc4:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xc5: // PUSH B
                        registers.memory[registers.Sp - 1] = registers.B;
                        registers.memory[registers.Sp - 2] = registers.C;
                        registers.Sp -= 2;
                        break;

                    case 0xC6: // 	ADI D8
                        answer = (ushort)((ushort)registers.A + (ushort)opcode[1]); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity(answer);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0xc7:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xc8:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xc9:  // RET
                        registers.Pc = (ushort)((ushort)registers.memory[registers.Sp + 1] << 8 | (ushort)registers.memory[registers.Sp]);
                        registers.Sp += 2;
                        break;

                    case 0xca:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xcb:
                        break;

                    case 0xcc:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xcd:  // CALL address
                        ushort ret = (ushort)(registers.Pc + 2);
                        registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                        registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                        registers.Sp = (ushort)(registers.Sp - 2);
                        registers.Pc = (ushort)((((ushort)opcode[2]) << (ushort)8) | (ushort)opcode[1]);
                        registers.Pc--;
                        break;

                    case 0xce:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xcf:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xd0:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xd1:
                        registers.E = registers.memory[registers.Sp];
                        registers.D = registers.memory[registers.Sp + 1];
                        registers.Sp += 2;
                        break;

                    case 0xd2:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xd3: // OUT
                        registers.Pc++;
                        break;

                    case 0xd4:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xd5:
                        registers.memory[registers.Sp - 1] = registers.D;
                        registers.memory[registers.Sp - 2] = registers.E;
                        registers.Sp -= 2;
                        break;

                    case 0xd6:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xd7:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xd8:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xd9:
                        break;

                    case 0xda:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xdb: // IN
                        registers.Pc++;
                        break;

                    case 0xdc:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xdd:
                        break;

                    case 0xde:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xdf:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xe0:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xe1: // POP H	
                        registers.L = registers.memory[registers.Sp];
                        registers.H = registers.memory[registers.Sp + 1];
                        registers.Sp += 2;
                        break;

                    case 0xe2:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xe3:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xe4:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xe5:
                        registers.memory[registers.Sp - 1] = registers.H;
                        registers.memory[registers.Sp - 2] = registers.L;
                        registers.Sp -= 2;
                        break;

                    case 0xe6: // ANI byte
                        x = (byte)(registers.A & opcode[1]);
                        if (x == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((x & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity(x);
                        registers.A = x;
                        registers.Pc++;
                        break;

                    case 0xe7:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xe8:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xe9:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xea:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xeb: // XCHG
                        byte temp1 = registers.D;
                        registers.D = registers.H;
                        registers.H = temp1;
                        byte temp2 = registers.L;
                        registers.L = registers.E;
                        registers.E = temp2;
                        break;

                    case 0xec:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xed:
                        break;

                    case 0xee:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xef:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xf0:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xf1: // POP PSW
                        registers.A = registers.memory[registers.Sp + 1];
                        psw = registers.memory[registers.Sp];
                        if (0x01 == (psw & 0x01)) registers.Flags.Z = 0x01; else registers.Flags.Z = 0x00;
                        if (0x02 == (psw & 0x02)) registers.Flags.S = 0x01; else registers.Flags.S = 0x00;
                        if (0x04 == (psw & 0x04)) registers.Flags.P = 0x01; else registers.Flags.P = 0x00;
                        if (0x08 == (psw & 0x08)) registers.Flags.Cy = 0x01; else registers.Flags.Cy = 0x00;
                        if (0x10 == (psw & 0x10)) registers.Flags.Ac = 0x01; else registers.Flags.Ac = 0x00;
                        registers.Sp += 2;
                        break;

                    case 0xf2:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xf3: // DI
                        registers.Int_enable = 0x00;
                        break;

                    case 0xf4:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xf5: // PUSH PSW
                        registers.memory[registers.Sp - 1] = registers.A;
                        psw = ((byte)(registers.Flags.Z | registers.Flags.S << 1 | registers.Flags.P << 2
                            | registers.Flags.Cy << 3 | registers.Flags.Ac << 4));
                        registers.memory[registers.Sp - 2] = psw;
                        registers.Sp = (ushort)(registers.Sp - (ushort)2);
                        break;

                    case 0xf6:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xf7:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xf8:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xf9:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xfa:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xfb:  // EI
                        registers.Int_enable = 0x01;
                        break;

                    case 0xfc:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                    case 0xfd:
                        break;

                    case 0xfe: // CPI byte
                        x = (byte)(registers.A - opcode[1]);
                        if (x == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((x & 0x80) == 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(x);
                        if ((registers.A < opcode[1])) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Pc++;
                        break;

                    case 0xff:
                        throw new UnimplementedInstruction(opcode[0].ToString("X2"));
                        break;

                }

                registers.Pc += (ushort)1;
                this.ProcessorState(registers);
                return 1;
            }
        }
    }
}
