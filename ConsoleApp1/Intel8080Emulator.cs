using Intel8080Emulator.Exceptions;
using System;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Diagnostics;

namespace Intel8080Emulator
{
    internal unsafe class Intel8080Emulator
    {

		const int DRAW_NEXT_FRAME_TIME = 8;
		const int CLOCK_SPEED = 2000; // Time is measured in miliseconds, so 2000 * time = 2MHz
		const int MIDDLE_SCREEN_INTERRUPT = 1;
		const int END_SCREEN_INTERRUPT = 2;
		const int INTERRUPT_ENABLED = 1;
		const int INTERRUPT_DISABLED = 0;

		//private StringBuilder emulationLog;
		private InstructionSet instructionSet;

		public Ports ports;
		public Registers registers;

		public Intel8080Emulator()
        {
            //emulationLog = new();
			instructionSet = new InstructionSet();
			ports = new();
            registers = new();
		}

        /*public void ProcessorState(Registers registers)
        {
			emulationLog.Append("Carry flag: "); emulationLog.Append(registers.Flags.Cy.ToString()); emulationLog.Append("\n");
            emulationLog.Append("Parity flag: "); emulationLog.Append(registers.Flags.P.ToString()); emulationLog.Append("\n");
            emulationLog.Append("Sign flag: "); emulationLog.Append(registers.Flags.S.ToString()); emulationLog.Append("\n");
            emulationLog.Append("Zero flag: "); emulationLog.Append(registers.Flags.Z.ToString()); emulationLog.Append("\n");
            emulationLog.Append(String.Format("A: $0x{0:X}; B: $0x{1:X}; C: $0x{2:X}; D: $0x{3:X}; E: $0x{4:X}; " +
                "H: $0x{5:X}; L: $0x{6:X}; SP: $0x{7:X}\n", registers.A.ToString("X2"), registers.B.ToString("X2"),
                registers.C.ToString("X2"), registers.D.ToString("X2"), registers.E.ToString("X2"), registers.H.ToString("X2"),
                registers.L.ToString("X2"), registers.Sp.ToString("X4"))); 
            emulationLog.Append("\n");
		}*/


		public void ReadTestRom() // Intel 8080 Diagnostics ROM
		{
			FileStream romObj = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "ROM\\cpudiag.bin", FileMode.Open, FileAccess.Read);

			romObj.Seek(0, SeekOrigin.Begin);
			Registers registers = new();
			registers.Pc = 0;

			for (int i = 0 + 0x100; i < romObj.Length + 0x100; i++)
			{
				registers.memory[i] = (byte)romObj.ReadByte();
			}

			// Fix the first instruction to be JMP 0x100
			registers.memory[0] = 0xc3; // OPCode for JMP
			registers.memory[1] = 0;
			registers.memory[2] = 0x01;

			// Fix the stack pointer from 0x6ad to 0x7ad
			// 0x06 is byte 112 in the test code, which is 112 + 0x100 = 368 in memory
			registers.memory[368] = 0x7;

			// Skip DAA Test
			registers.memory[0x59c] = 0xc3; //JMP
			registers.memory[0x59d] = 0xc2;
			registers.memory[0x59e] = 0x05;

			while (true)
			{
				this.Emulate8080OpInstruction(registers);

				/* using (System.IO.StreamWriter file = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "\\DebugLogs\\testDebug.txt"))
				{
					file.WriteLine(emulationLog.ToString());
				} */
				//emulationLog.Clear();
			}
		}

        public void ReadRom(Registers registers)
        {
            FileStream romObj = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "ROM\\invaders", FileMode.Open, FileAccess.Read);
            romObj.Seek(0, SeekOrigin.Begin);
            registers.Pc = 0;

            for (int i = 0; i < romObj.Length; i++)
            {
                registers.memory[i] = (byte)romObj.ReadByte();
            }

        }

        public void DoEmulation(Registers registers) {

			var time = new Stopwatch();
			double lastTimer = 0.0;
			double nextInterruptTime = 0.0;
			byte whichInterrupt = MIDDLE_SCREEN_INTERRUPT;

			time.Start();

			while (true)
			{

				if (lastTimer == 0.0)
				{
					lastTimer = time.Elapsed.TotalMilliseconds;
					nextInterruptTime = lastTimer + DRAW_NEXT_FRAME_TIME * 2;
					whichInterrupt = MIDDLE_SCREEN_INTERRUPT;
				}

				if (registers.Int_enable == INTERRUPT_ENABLED && time.Elapsed.TotalMilliseconds > nextInterruptTime)
				{
					if (whichInterrupt == MIDDLE_SCREEN_INTERRUPT)
					{
						//	emulationLog.Append("Generating Interrupt 1 "); emulationLog.Append("\n");
						this.GenerateInterrupt(registers, MIDDLE_SCREEN_INTERRUPT); // Interrupt 1        
						whichInterrupt = END_SCREEN_INTERRUPT;
					}
					else
					{
						//	emulationLog.Append("Generating Interrupt 2 "); emulationLog.Append("\n");
						this.GenerateInterrupt(registers, END_SCREEN_INTERRUPT); // Interrupt 2, Middle of frame    
						whichInterrupt = MIDDLE_SCREEN_INTERRUPT;
					}
					nextInterruptTime = time.Elapsed.TotalMilliseconds + DRAW_NEXT_FRAME_TIME;
				}

				// CPU = 2Mhz = 2000000 cycle/second
				double sinceLast = time.Elapsed.TotalMilliseconds - lastTimer;
				if (sinceLast < DRAW_NEXT_FRAME_TIME)
				{
					Thread.Sleep((int)(time.Elapsed.TotalMilliseconds - lastTimer));
				} 
				sinceLast = time.Elapsed.TotalMilliseconds - lastTimer;
				int cyclesLeft = (int)(CLOCK_SPEED * sinceLast); 
				int cycles = 0;

				while (cyclesLeft > cycles)
				{
					//	emulationLog.Append("cyclesLeft: "); emulationLog.Append(cyclesLeft.ToString()); emulationLog.Append("\n");
					//	emulationLog.Append("Cycles: "); emulationLog.Append(cycles.ToString()); emulationLog.Append("\n");
					//	emulationLog.Append("Timer: "); emulationLog.Append(time.Elapsed.TotalMilliseconds.ToString()); emulationLog.Append("\n");
					fixed (byte* opcode = &registers.memory[registers.Pc])
						if (registers.memory[registers.Pc] == 0xdb) // IN INSTRUCTION
						{
							//emulationLog.Append("In Instruction: "); emulationLog.Append("\n");
							byte port = (registers.memory[registers.Pc + 1]);
							this.MachineIn(registers, port);
							registers.Pc += 2;
							cycles += 3;
						}
						else if (registers.memory[registers.Pc] == 0xd3) // OUT INSTRUCTION
						{
							//emulationLog.Append("Out INSTRUCTION: "); emulationLog.Append("\n");
							byte port = (registers.memory[registers.Pc + 1]);
							this.MachineOut(port, registers.A);
							registers.Pc += 2;
							cycles += 3;
						}
						else
						{
							cycles += this.Emulate8080OpInstruction(registers);
						}
				}
             
				lastTimer = time.Elapsed.TotalMilliseconds;


				// using (System.IO.StreamWriter file = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "DebugLogs\\invadersDebug.txt"))
				//{
				//file.WriteLine(emulationLog.ToString());
				//} 
				//emulationLog.Clear();  
			}

		}

		public void MachineIn(Registers registers, byte port)
        {
            switch (port)
            {
                case 0:
                    registers.A = this.ports.InPorts[0];
                    break;
                case 1:
					registers.A = this.ports.InPorts[1];
					break;
				case 2:
					registers.A = this.ports.InPorts[2];
					break;
				case 3: // Read shift data
                    ushort aux = (ushort)((((ushort)(this.ports.Shift1)) << 8) | ((ushort)this.ports.Shift0));
					registers.A = (byte)(((aux >> (8 - this.ports.ShiftOffset)) & 0x00ff));
					break;
                default:
                    return;
            }
        }

        public void MachineOut(byte port, byte value) // value of register A
        {
            switch (port)
            {
                case 2: // shift offset
                    this.ports.ShiftOffset = (byte)(value & 0x7); // bits 0, 1 and 2 define the offset
                    break;
                case 3:
					this.ports.OutPorts[3] = value;
					break;
				case 4:
                    this.ports.Shift0 = this.ports.Shift1;
                    this.ports.Shift1 = value;
                    break;
                case 5:
					this.ports.OutPorts[5] = value;
                    break;
				case 6:
                    this.ports.OutPorts[6] = value;
                    break;
                default:
                    return;
            }
        }

        public void MachineKeyDown(int key)
        {
            switch (key)
            {
                case 1:
                    this.ports.InPorts[0] |= 0x10; // Port 0 Fire
                    break;
                case 2:
                    this.ports.InPorts[0] |= 0x20; // Port 0 Left
                    break;
                case 3:
                    this.ports.InPorts[0] |= 0x40; // Port 0 Right
                    break;
                case 4:
					this.ports.InPorts[1] |= 0x01; // Credit
					break;
                case 5:
                    this.ports.InPorts[1] |= 0x02; // 2P START
                    break;
                case 6:
                    this.ports.InPorts[1] |= 0x04; // 1P START
                    break;
                case 7:
                    this.ports.InPorts[1] |= 0x10; // 1P SHOT
                    break;
                case 8:
                    this.ports.InPorts[1] |= 0x20; // 1P LEFT
                    break;
                case 9:
                    this.ports.InPorts[1] |= 0x40; // 1P RIGHT
                    break;
                case 10:
                    this.ports.InPorts[2] |= 0x10; // 2P SHOT
                    break;
                case 11:
                    this.ports.InPorts[2] |= 0x20; // 2P LEFT
                    break;
                case 12:
                    this.ports.InPorts[2] |= 0x40; // 2P RIGHT
                    break;
            }
        }



        public void MachineKeyUp(int key)
        {
            switch (key)
            {
                case 1:
                    this.ports.InPorts[0] &= 0xEF; // Port 0 Fire
                    break;
                case 2:
                    this.ports.InPorts[0] &= 0xDF; // Port 0 Left
                    break;
                case 3:
                    this.ports.InPorts[0] &= 0xBF; // Port 0 Right
                    break;
                case 4:
					this.ports.InPorts[1] &= 0xFE; // Credit
					break;
                case 5:
                    this.ports.InPorts[1] &= 0xFD; // 2P START
                    break;
                case 6:
                    this.ports.InPorts[1] &= 0xFB; // 1P START
                    break;
                case 7:
                    this.ports.InPorts[1] &= 0xEF;
                    break;
                case 8:
                    this.ports.InPorts[1] &= 0xDF;
                    break;
                case 9:
                    this.ports.InPorts[1] &= 0xBF;
                    break;
                case 10:
                    this.ports.InPorts[2] &= 0xEF; // 2P SHOT
                    break;
                case 11:
                    this.ports.InPorts[2] &= 0xDF; // 2P LEFT
                    break;
                case 12:
                    this.ports.InPorts[2] &= 0xBF; // 2P RIGHT
                    break;
            }
        }


        public void GenerateInterrupt(Registers registers, int interruptNum)
        {
            ushort aux = (ushort)(registers.Pc - (ushort)0x0001);
			Byte high = (byte)((aux & 0xFF00) >> 8);
			Byte low = (byte)((aux) & 0x00ff);
			Push(registers, high, low);
            registers.Pc = (ushort)(8 * interruptNum);
            registers.Int_enable = INTERRUPT_DISABLED;
        }

        public void Push(Registers registers, byte A, byte B)
        {
			registers.memory[registers.Sp - 2] = B;
            registers.memory[registers.Sp - 1] = A;
            registers.Sp -= 2;
		}

        public static byte Parity(int x, int size)
        {
            int i;
            int p = 0;
            x = (x & ((1 << size) - 1));
            for (i = 0; i < size; i++)
            {
                if ((x & 0x1) == 0x1) p++;
                x = x >> 1;
            }

            if ((p & 1) == 0)
            {
                return 1;
            }
            else
            {
                return 0;
            };
        }

        public int Emulate8080OpInstruction(Registers registers)
        {
            fixed (byte* opcode = &registers.memory[registers.Pc])
            {
                ushort answer;
                ushort offset;
                ushort ret;
                byte x;
                byte psw;
                UInt32 aux1;
                UInt32 aux2;
                int cycleCount;

                fixed (byte* codebuffer = &registers.memory[0])
                {
					cycleCount = instructionSet.opDictionary[codebuffer[registers.Pc]].CycleCount;
				}
				//  emulationLog.Append(String.Format("Opcode: $0x{0:X}\n", opcode[0].ToString("X2")));
				//  emulationLog.Append(String.Format("PC: $0x{0:X}\n", registers.Pc.ToString("X4")));

				switch (*opcode) 
                {
                    case 0x00: // NOP               
                        break;

                    case 0x01: // LXI B
                        registers.C = opcode[(byte)1];
                        registers.B = opcode[(byte)2];
                        registers.Pc += (ushort)2;
                        break;

                    case 0x02: // STAX B //
                        registers.memory[((ushort)registers.B) << 8 | (ushort)registers.C] = registers.A;
                        break;

                    case 0x03: // INX B
                        answer = (ushort)(((ushort)registers.B << 8) | (ushort)registers.C);
                        answer++;
                        registers.B = (byte)((answer & 0xff00) >> 8);
                        registers.C = (byte)((answer & 0x00ff));
                        break;

                    case 0x04: // INR B
                        registers.B++;
                        if ((registers.B & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.B & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.B, 8);
                        break;

                    case 0x05: // 	DCR B -- 	B <- B-1
                        registers.B--;
                        if ((registers.B & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.B & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.B, 8);
                        break;

                    case 0x06: // MVI B, D8 -- B <- byte 2
                        registers.B = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x07: // RLC
                        x = registers.A;
                        registers.A = (byte)((x << (byte)1) | ((x & (byte)0x80) >> (byte)7));
                        if ((registers.A & (byte)0x01) == (byte)0x01) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        break;

                    case 0x08: // NOP
                        break;

                    case 0x09: // DAD B
                        aux1 = (UInt32)(((UInt32)registers.H << 8) | ((UInt32)registers.L));
                        aux2 = (UInt32)(((UInt32)registers.B << 8) | ((UInt32)registers.C));
                        aux2 = (UInt32)(aux1 + aux2);
                        if ((aux2 > (UInt32)0xffff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        aux2 = (ushort)(aux2 & (ushort)0xffff); // Returning to 16 bits
                        registers.H = ((byte)((ushort)(aux2 & 0xff00) >> 8));
                        registers.L = ((byte)((ushort)(aux2 & 0x00ff)));
                        break;

                    case 0x0a: // LDAX B
                        registers.A = registers.memory[((ushort)registers.B << 8) | (ushort)registers.C];
                        break;

                    case 0x0b: // DCX B 
                        answer = (ushort)(((ushort)registers.B << 8) | (ushort)registers.C);
                        answer--;
                        registers.B = (byte)((answer & 0xff00) >> 8);
                        registers.C = (byte)((answer & 0x00ff));
                        break;

                    case 0x0c: // INR C
                        registers.C++;
                        if ((registers.C & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.C & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.C, 8);
                        break;

                    case 0x0d: // DCR C
                        registers.C--;
                        if ((registers.C & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.C & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.C, 8);
                        break;

                    case 0x0e: // MVI C,D8
                        registers.C = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x0f: // RRC 	A = A >> 1; bit 7 = prev bit 0; CY = prev bit 0
						var y = registers.A;
						x = registers.A;
						registers.A = (byte)((byte)registers.A >> (byte)0x01);
						y = (byte)(((byte)(y << (byte)0x07)) & ((byte)(0x80)));
						registers.A = (byte)(registers.A | y);
						if ((byte)((x & (byte)0x01)) == (byte)0x01) registers.Flags.Cy = 1; else registers.Flags.Cy = 0;
						break;


                    case 0x10: // NOP
                        break;

                    case 0x11: // 	LXI D,D16 -- D <- byte 3, E <- byte 2
                        registers.E = opcode[1];
                        registers.D = opcode[2];
                        registers.Pc += 2;
                        break;

                    case 0x12: // STAX D
                        registers.memory[((ushort)registers.D) << 8 | (ushort)registers.E] = registers.A;
                        break;

                    case 0x13: // 	INX D -- 	DE <- DE + 1
                        answer = (ushort)(((ushort)registers.D << 8) | (ushort)registers.E);
                        answer++;
                        registers.D = (byte)((answer & 0xff00) >> 8);
                        registers.E = (byte)((answer & 0x00ff));
                        break;

                    case 0x14:  // 	INR D
                        registers.D++;
                        if ((registers.D & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.D & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.D, 8);
                        break;

                    case 0x15: // DCR D
                        registers.D--;
                        if ((registers.D & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.D & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.D, 8);
                        break;

                    case 0x16: // MVI D, D8
                        registers.D = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x17: // RAL
                        x = registers.A;
                        registers.A = (byte)((((byte)x & (byte)0x01) << (byte)0x07) | ((byte)x >> (byte)0x01));
                        byte prevCarry = registers.Flags.Cy;
                        registers.Flags.Cy = (byte)((x & 0x80) >> 7);
                        registers.A = (byte)((x << 1) | (prevCarry));
                        break;

                    case 0x18: // NOP
                        break;

                    case 0x19: // DAD D
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

                    case 0x1b: // DCX D
                        answer = (ushort)(((ushort)registers.D << 8) | (ushort)registers.E);
                        answer--;
                        registers.D = (byte)((answer & 0xff00) >> 8);
                        registers.E = (byte)((answer & 0x00ff));
                        break;

                    case 0x1c: // INR E
                        registers.E++;
                        if ((registers.E & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.E & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.E, 8);
                        break;

                    case 0x1d: // DCR E
                        registers.E--;
                        if ((registers.E & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.E & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.E, 8);
                        break;

                    case 0x1e: // MVI E,D8
                        registers.E = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x1f: // RAR 	A = A >> 1; bit 7 = prev bit 7; CY = prev bit 0
                        x = registers.A;
                        registers.A = (byte)((registers.Flags.Cy << 0x07) | (x >> 0x01));
                        if ((x & 0x01) == 0x01) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        break;

                    case 0x20: //NOP
                        break;

                    case 0x21:  // 	LXI H,D16 -- 	H <- byte 3, L <- byte 2
                        registers.L = opcode[1];
                        registers.H = opcode[2];
                        registers.Pc += 2;
                        break;

                    case 0x22:  // SHLD adr
                        registers.memory[((ushort)opcode[2] << 8) | (ushort)opcode[1]] = registers.L;
                        registers.memory[(((ushort)opcode[2] << 8) | (ushort)opcode[1]) + 1] = registers.H;
                        registers.Pc += 2;
                        break;

                    case 0x23: // INX H -- HL <- HL + 1
                        answer = (ushort)(((ushort)registers.H << 8) | (ushort)registers.L);
                        answer++;
                        registers.H = (byte)((answer & 0xff00) >> 8);
                        registers.L = (byte)((answer & 0x00ff));
                        break;

                    case 0x24: // INR H
                        registers.H++;
                        if ((registers.H & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.H & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.H, 8);
                        break;

                    case 0x25: // DCR H
                        registers.H--;
                        if ((registers.H & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.H & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.H, 8);
                        break;

                    case 0x26: // MVI H,D8
                        registers.H = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x27:  //DAA // This is for adding binary-coded decimal values. https://www.righto.com/2023/01/understanding-x86s-decimal-adjust-after.html
						if ((registers.A & 0x0f) > 9)
                        {
                            registers.A += 6; // This is when the added decimal binary codes end up having A-F in the first digit (base 16). We should correct adding 6
                        }
                        if ((registers.A & 0xf0) > 0x90) // When adding only 6 will not work, because we have a carry (the addition overflowed 8 bits)
                        {
                            ushort res = (ushort)((ushort)registers.A + (ushort)0x60);
							if ((res > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
							if ((res & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
							if ((res & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
							registers.Flags.P = Intel8080Emulator.Parity((byte)(res & (ushort)0xff), 8);
							registers.A = (byte)(res & (ushort)0xff); // Returning to 8 bits


						}
                        break;

					case 0x28: // NOP
                        break;

                    case 0x29: //DAD H -> HL = HL + HL
                        aux1 = (UInt32)(((UInt32)registers.H << 8) | ((UInt32)registers.L)); // 32 bits variable to hold carry flag
                        aux2 = ((UInt32)aux1) << 1; // Adding HL to HL has the same effect of << 1
                        if ((aux2 > (UInt32)0xffff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        aux2 = (ushort)(aux2 & (ushort)0xffff); // Returning to 16 bits
                        registers.H = ((byte)((ushort)(aux2 & 0xff00) >> 8));
                        registers.L = ((byte)((ushort)(aux2 & 0x00ff)));
                        break;

                    case 0x2a: //	LHLD adr
                        registers.L = registers.memory[(((ushort)opcode[2]) << 8) | (ushort)opcode[1]];
                        registers.H = registers.memory[((((ushort)opcode[2]) << 8) | (ushort)opcode[1]) + 1];
                        registers.Pc += 2;
                        break;

                    case 0x2b: // DCX H
                        answer = (ushort)(((ushort)registers.H << 8) | (ushort)registers.L);
                        answer--;
                        registers.H = (byte)((answer & 0xff00) >> 8);
                        registers.L = (byte)((answer & 0x00ff));
                        break;

                    case 0x2c: // INR L
                        registers.L++;
                        if ((registers.L & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.L & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.L, 8);
                        break;

                    case 0x2d:  // DCR L
                        registers.L--;
                        if ((registers.L & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.L & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.L, 8);
                        break;

                    case 0x2e:  // MVI L, D8
                        registers.L = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x2f: // CMA (not)
                        registers.A = (byte)~registers.A;
                        break;

                    case 0x30: // NOP
                        break;

                    case 0x31: // 	LXI SP, D16 //	SP.hi <- byte 3, SP.lo <- byte 2
                        registers.Sp = (ushort)((ushort)opcode[2] << 8);
                        registers.Sp = (ushort)(registers.Sp | (ushort)opcode[1]);
                        registers.Pc += (ushort)2;
                        break;

                    case 0x32: // STA Adr
                        registers.memory[(((ushort)opcode[2]) << 8) | ((ushort)opcode[1])] = registers.A;
                        registers.Pc += 2;
                        break;

                    case 0x33: // 	INX SP
                        registers.Sp++;
                        break;

                    case 0x34: // INR M
                        registers.memory[(((ushort)registers.H) << 8) | (ushort)registers.L]++;
                        if ((registers.memory[(((ushort)registers.H) << 8) | (ushort)registers.L] & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.memory[(((ushort)registers.H) << 8) | (ushort)registers.L] & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.memory[(((ushort)registers.H) << 8) | (ushort)registers.L], 8);
                        break;

                    case 0x35: // DCR M
                        registers.memory[(((ushort)registers.H) << 8) | (ushort)registers.L]--;
                        if ((registers.memory[(((ushort)registers.H) << 8) | (ushort)registers.L] & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.memory[(((ushort)registers.H) << 8) | (ushort)registers.L] & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.memory[(((ushort)registers.H) << 8) | (ushort)registers.L], 8);
                        break;

                    case 0x36: // 	MVI M,D8 -- (HL) <- byte 2
                        registers.memory[(((ushort)registers.H) << 8) | (ushort)registers.L] = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x37: // STC
                        registers.Flags.Cy = 1;
                        break;

                    case 0x38: // NOP
                        break;

                    case 0x39: // DAD SP
                        aux1 = (UInt32)(((UInt32)registers.H << 8) | ((UInt32)registers.L));
                        aux2 = (UInt32)(aux1 + (UInt32)registers.Sp);
                        if ((aux2 > (UInt32)0xffff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        aux2 = (ushort)(aux2 & (ushort)0xffff); // Returning to 16 bits
                        registers.H = ((byte)((ushort)(aux2 & 0xff00) >> 8));
                        registers.L = ((byte)((ushort)(aux2 & 0x00ff)));
                        break;

                    case 0x3a: // LDA adr
                        registers.A = registers.memory[(((ushort)opcode[2]) << 8) | ((ushort)opcode[1])];
                        registers.Pc += 2;
                        break;

                    case 0x3b: // DCX SP
                        registers.Sp--;
                        break;

                    case 0x3c: // INR A
                        registers.A++;
                        if ((registers.A & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.A & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        break;

                    case 0x3d: // DCR A
                        registers.A--;
                        if ((registers.A & 0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((registers.A & 0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        break;

                    case 0x3e: // MVI A,D8
                        registers.A = opcode[1];
                        registers.Pc++;
                        break;

                    case 0x3f: // CMC
                        registers.Flags.Cy = (byte)~registers.Flags.Cy;
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

                    case 0x76: // HALT AND CATCH FIRE
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
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x81: // ADD C
                        answer = (ushort)((ushort)registers.A + (ushort)registers.C); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x82: // ADD D
                        answer = (ushort)((ushort)registers.A + (ushort)registers.D); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x83: // ADD E
                        answer = (ushort)((ushort)registers.A + (ushort)registers.E); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x84: // ADD H
                        answer = (ushort)((ushort)registers.A + (ushort)registers.H); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x85: // ADD L
                        answer = (ushort)((ushort)registers.A + (ushort)registers.L); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x86: // ADD M -> | A <- A + (HL)
                        offset = (ushort)((ushort)(registers.H << 8) | (ushort)registers.L);
                        answer = (ushort)((ushort)registers.A + (ushort)registers.memory[offset]); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x87:
                        answer = (ushort)((ushort)registers.A + (ushort)registers.A); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x88: // ADC B | A <- A + B + CY
                        answer = (ushort)((ushort)registers.A + (ushort)registers.B + (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x89:
                        answer = (ushort)((ushort)registers.A + (ushort)registers.C + (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x8a:
                        answer = (ushort)((ushort)registers.A + (ushort)registers.D + (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x8b:
                        answer = (ushort)((ushort)registers.A + (ushort)registers.E + (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x8c:
                        answer = (ushort)((ushort)registers.A + (ushort)registers.H + (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x8d:
                        answer = (ushort)((ushort)registers.A + (ushort)registers.L + (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x8e:
                        offset = (ushort)((ushort)(registers.H << 8) | (ushort)registers.L);
                        answer = (ushort)((ushort)registers.A + (ushort)registers.memory[offset] + (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x8f:
                        answer = (ushort)((ushort)registers.A + (ushort)registers.A + (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x90: // SUB B
                        answer = (ushort)((ushort)registers.A - (ushort)registers.B); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x91:
                        answer = (ushort)((ushort)registers.A - (ushort)registers.C); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x92:
                        answer = (ushort)((ushort)registers.A - (ushort)registers.D); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x93:
                        answer = (ushort)((ushort)registers.A - (ushort)registers.E); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x94:
                        answer = (ushort)((ushort)registers.A - (ushort)registers.H); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x95:
                        answer = (ushort)((ushort)registers.A - (ushort)registers.L); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x96:
                        offset = (ushort)((ushort)(registers.H << 8) | (ushort)registers.L);
                        answer = (ushort)((ushort)registers.A - (ushort)registers.memory[offset]); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x97:
                        answer = (ushort)((ushort)registers.A - (ushort)registers.A); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x98: // SBB B
                        answer = (ushort)((ushort)registers.A - (ushort)registers.B - (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x99:
                        answer = (ushort)((ushort)registers.A - (ushort)registers.C - (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x9a:
                        answer = (ushort)((ushort)registers.A - (ushort)registers.D - (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x9b:
                        answer = (ushort)((ushort)registers.A - (ushort)registers.E - (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x9c:
                        answer = (ushort)((ushort)registers.A - (ushort)registers.H - (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x9d:
                        answer = (ushort)((ushort)registers.A - (ushort)registers.L - (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x9e:
                        offset = (ushort)((ushort)(registers.H << 8) | (ushort)registers.L);
                        answer = (ushort)((ushort)registers.A - (ushort)registers.memory[offset] - (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0x9f:
                        answer = (ushort)((ushort)registers.A - (ushort)registers.A - (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        break;

                    case 0xa0: // ANA B
                        registers.A = (byte)(registers.A & registers.B);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xa1: // ANA C
                        registers.A = (byte)(registers.A & registers.C);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xa2: // ANA D
                        registers.A = (byte)(registers.A & registers.D);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xa3: // ANA E
                        registers.A = (byte)(registers.A & registers.E);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xa4: // ANA H
                        registers.A = (byte)(registers.A & registers.H);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xa5: // ANA L
                        registers.A = (byte)(registers.A & registers.L);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xa6: // ANA (HL)
                        registers.A = (byte)(registers.A & (registers.memory[((ushort)registers.H << 8) | (ushort)registers.L]));
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xa7: // ANA A
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xa8: // XRA B
                        registers.A = (byte)(registers.A ^ registers.B);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xa9: // XRA C
                        registers.A = (byte)(registers.A ^ registers.C);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xaa: // XRA D
                        registers.A = (byte)(registers.A ^ registers.D);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xab: // XRA E
                        registers.A = (byte)(registers.A ^ registers.E);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xac: // XRA H
                        registers.A = (byte)(registers.A ^ registers.H);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xad: // XRA L
                        registers.A = (byte)(registers.A ^ registers.L);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xae: // XRA M (HL)
                        registers.A = (byte)(registers.A ^ (registers.memory[((ushort)registers.H << 8) | (ushort)registers.L]));
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xaf: // XOR A (accumulator) -> zero the accumulator
                        registers.A = 0;
                        registers.Flags.Z = 1;
                        registers.Flags.S = 0;
                        registers.Flags.P = 1;
                        registers.Flags.Cy = 0;
                        break;

                    case 0xb0: // ORA B 
                        registers.A = (byte)(registers.A | registers.B);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xb1: // ORA C
                        registers.A = (byte)(registers.A | registers.C);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xb2: // ORA D
                        registers.A = (byte)(registers.A | registers.D);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xb3: // ORA E
                        registers.A = (byte)(registers.A | registers.E);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xb4: // ORA H
                        registers.A = (byte)(registers.A | registers.H);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xb5: // ORA L
                        registers.A = (byte)(registers.A | registers.L);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xb6: // ORA M
                        registers.A = (byte)(registers.A | (registers.memory[((ushort)registers.H << 8) | (ushort)registers.L]));
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xb7: // ORA A
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        break;

                    case 0xb8: // CMP B
                        x = (byte)(registers.A - registers.B);
                        if (x == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((x & 0x80) == 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(x, 8);
                        if ((registers.A < registers.B)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        break;

                    case 0xb9: // CMP C
                        x = (byte)(registers.A - registers.C);
                        if (x == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((x & 0x80) == 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(x, 8);
                        if ((registers.A < registers.C)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        break;

                    case 0xba: // CMP D
                        x = (byte)(registers.A - registers.D);
                        if (x == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((x & 0x80) == 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(x, 8);
                        if ((registers.A < registers.D)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        break;

                    case 0xbb: // CMP E
                        x = (byte)(registers.A - registers.E);
                        if (x == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((x & 0x80) == 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(x, 8);
                        if ((registers.A < registers.E)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        break;

                    case 0xbc: // CMP H
                        x = (byte)(registers.A - registers.H);
                        if (x == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((x & 0x80) == 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(x, 8);
                        if ((registers.A < registers.H)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        break;

                    case 0xbd: // CMP L
                        x = (byte)(registers.A - registers.L);
                        if (x == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((x & 0x80) == 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(x, 8);
                        if ((registers.A < registers.L)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        break;

                    case 0xbe: // CMP M (HL)
                        offset = (ushort)((ushort)(registers.H << 8) | (ushort)registers.L);
                        x = (byte)(registers.A - registers.memory[offset]);
                        if (x == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((x & 0x80) == 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(x, 8);
                        if ((registers.A < registers.memory[offset])) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        break;

                    case 0xbf:  // CMP A
                        registers.Flags.Z = 1;
                        registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0; // Carry Flag
                        break;

                    case 0xc0: //RNZ -> Return If Not Zero
                        if (registers.Flags.Z == 0)
                        {
                            registers.Pc = (ushort)((ushort)registers.memory[registers.Sp + 1] << 8 | (ushort)registers.memory[registers.Sp]);
                            registers.Sp += 2;
                        }
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

                    case 0xc4: // CNZ adr
                        if (registers.Flags.Z == 0)
                        {
                            ret = (ushort)(registers.Pc + 2);
                            registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                            registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                            registers.Sp = (ushort)(registers.Sp - 2);
                            registers.Pc = (ushort)((((ushort)opcode[2]) << (ushort)8) | (ushort)opcode[1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
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
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        registers.Pc++;
                        break;

                    case 0xc7: // RST 0 -- CAll $0
                        ret = (ushort)(registers.Pc + 2);
                        registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                        registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                        registers.Sp = (ushort)(registers.Sp - 2);
                        registers.Pc = (ushort)0x00;
                        registers.Pc--;
                        break;

                    case 0xc8: // RZ
                        if (registers.Flags.Z == 1)
                        {
                            registers.Pc = (ushort)((ushort)registers.memory[registers.Sp + 1] << 8 | (ushort)registers.memory[registers.Sp]);
                            registers.Sp += 2;
                        }
                        break;

                    case 0xc9:  // RET                        
						registers.Pc = (ushort)((ushort)registers.memory[registers.Sp + 1] << 8 | (ushort)registers.memory[registers.Sp]);
						registers.Sp += 2;
                        break;

                    case 0xca: // JZ ADR
                        if (registers.Flags.Z == 1)
                        {
                            registers.Pc = (ushort)((ushort)opcode[(byte)2] << 8 | (ushort)opcode[(byte)1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xcb: // NOP
                        break;

                    case 0xcc: // CZ ADR
                        if (registers.Flags.Z == 1)
                        {
                            ret = (ushort)(registers.Pc + 2);
                            registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                            registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                            registers.Sp = (ushort)(registers.Sp - 2);
                            registers.Pc = (ushort)((((ushort)opcode[2]) << (ushort)8) | (ushort)opcode[1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xcd:  // CALL address

                       // Console.WriteLine("");
                      /*   if (0x0005 == ((((ushort)opcode[2]) << 8) | ((ushort)opcode[1])))
                        {
                            Console.WriteLine("");
                            if (registers.C == 9)
                            {
                                Console.WriteLine("");
                                ushort offset2 = (ushort)(((ushort)registers.D) << 8 | registers.E);
                                int i = 3; //Skip prefix bytes
                                byte str = registers.memory[offset2 + i];
                                while ((char)str != '$')
                                {
                                    Console.Write((char)str);
                                    i++;
                                    str = registers.memory[offset2 + i];
                                }
                                Console.Write((char)str);
                                Console.WriteLine("");
                                System.Environment.Exit(1);
                            }
                            else
                            {
                                Console.WriteLine("");
                                if (registers.C == 2)
                                {
                                    Console.WriteLine("Console.WriteLine(\"\");");
                                }
                            }
                        }
                        else if (0x0000 == ((((ushort)opcode[2]) << 8) | ((ushort)opcode[1])))
                        {
                            Console.WriteLine("");
                            System.Environment.Exit(1);
                        }

                        else 
                        { */

                            ret = (ushort)(registers.Pc + 2);
                            registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                            registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                            registers.Sp = (ushort)(registers.Sp - 2);
                            registers.Pc = (ushort)((((ushort)opcode[2]) << (ushort)8) | (ushort)opcode[1]);
                            registers.Pc--;
                                
                       // }
                            break;

                    case 0xce: // ACI D8
                        answer = (ushort)((ushort)registers.A + (ushort)opcode[1] + (ushort)registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        registers.Pc++;
                        break;

                    case 0xcf: // RST 1
                        ret = (ushort)(registers.Pc + 2);
                        registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                        registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                        registers.Sp = (ushort)(registers.Sp - 2);
                        registers.Pc = (ushort)0x08;
                        registers.Pc--;
                        break;

                    case 0xd0: // RNC
                        if (registers.Flags.Cy == 0)
                        {
                            registers.Pc = (ushort)((ushort)registers.memory[registers.Sp + 1] << 8 | (ushort)registers.memory[registers.Sp]);
                            registers.Sp += 2;
                        }
                        break;

                    case 0xd1:
                        registers.E = registers.memory[registers.Sp];
                        registers.D = registers.memory[registers.Sp + 1];
                        registers.Sp += 2;
                        break;

                    case 0xd2: // JNC ADR
                        if (registers.Flags.Cy == 0)
                        {
                            registers.Pc = (ushort)((ushort)opcode[(byte)2] << 8 | (ushort)opcode[(byte)1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xd3: // OUT D8
                        registers.Pc++;
                        break;

                    case 0xd4: // CNC ADR
                        if (registers.Flags.Cy == 0)
                        {
                            ret = (ushort)(registers.Pc + 2);
                            registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                            registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                            registers.Sp = (ushort)(registers.Sp - 2);
                            registers.Pc = (ushort)((((ushort)opcode[2]) << (ushort)8) | (ushort)opcode[1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xd5:
						registers.memory[registers.Sp - 1] = registers.D;
                        registers.memory[registers.Sp - 2] = registers.E;
                        registers.Sp -= 2;
						break;

                    case 0xd6: // SUI D8
                        answer = (ushort)((ushort)registers.A - (ushort)opcode[1]); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        registers.Pc++;
                        break;

                    case 0xd7: // RST 2
                        ret = (ushort)(registers.Pc + 2);
                        registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                        registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                        registers.Sp = (ushort)(registers.Sp - 2);
                        registers.Pc = (ushort)0x10;
                        registers.Pc--;
                        break;

                    case 0xd8: // RC
                        if (registers.Flags.Cy == 1)
                        {
                            registers.Pc = (ushort)((ushort)registers.memory[registers.Sp + 1] << 8 | (ushort)registers.memory[registers.Sp]);
                            registers.Sp += 2;
                        }
                        break;

                    case 0xd9: // NOP
                        break;

                    case 0xda:  // JC ADR
                        if (registers.Flags.Cy == 1)
                        {
                            registers.Pc = (ushort)((ushort)opcode[(byte)2] << 8 | (ushort)opcode[(byte)1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xdb: // IN
                        registers.Pc++;
                        break;

                    case 0xdc:  // CC adr
                        if (registers.Flags.Cy == 1)
                        {
                            ret = (ushort)(registers.Pc + 2);
                            registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                            registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                            registers.Sp = (ushort)(registers.Sp - 2);
                            registers.Pc = (ushort)((((ushort)opcode[2]) << (ushort)8) | (ushort)opcode[1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xdd: // NOP
                        break;

                    case 0xde: // SBI D8
                        answer = (ushort)((ushort)registers.A - (ushort)opcode[1] - registers.Flags.Cy); // Higher precision to capture carry out
                        if ((answer & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((answer & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        if ((answer > (ushort)0xff)) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity((byte)(answer & (ushort)0xff), 8);
                        registers.A = (byte)(answer & (ushort)0xff); // Returning to 8 bits
                        registers.Pc++;
                        break;

                    case 0xdf: // RST 3
                        ret = (ushort)(registers.Pc + 2);
                        registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                        registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                        registers.Sp = (ushort)(registers.Sp - 2);
                        registers.Pc = (ushort)0x18;
                        registers.Pc--;
                        break;

                    case 0xe0: // RPO
                        if (registers.Flags.P == 0)
                        {
                            registers.Pc = (ushort)((ushort)registers.memory[registers.Sp + 1] << 8 | (ushort)registers.memory[registers.Sp]);
                            registers.Sp += 2;
                        }
                        break;

                    case 0xe1: // POP H	
                        registers.L = registers.memory[registers.Sp];
                        registers.H = registers.memory[registers.Sp + 1];
                        registers.Sp += 2;
                        break;

                    case 0xe2: // JPO adr
                        if (registers.Flags.P == 0)
                        {
                            registers.Pc = (ushort)((ushort)opcode[(byte)2] << 8 | (ushort)opcode[(byte)1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xe3: // XTHL
                        aux1 = registers.L;
                        registers.L = registers.memory[registers.Sp];
                        registers.memory[registers.Sp] = (byte)((byte)aux1 - (byte)0x01); // Subtracting 1 because PC always adds 1 at the end of the switch
                        aux2 = registers.H;
                        registers.H = registers.memory[registers.Sp + 1];
                        registers.memory[registers.Sp + 1] = (byte)aux2;
                        break;

                    case 0xe4: // CPO Adr
                        if (registers.Flags.P == 0)
                        {
                            ret = (ushort)(registers.Pc + 2);
                            registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                            registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                            registers.Sp = (ushort)(registers.Sp - 2);
                            registers.Pc = (ushort)((((ushort)opcode[2]) << (ushort)8) | (ushort)opcode[1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xe5: // PUSH H
                        registers.memory[registers.Sp - 1] = registers.H;
                        registers.memory[registers.Sp - 2] = registers.L;
                        registers.Sp -= 2;
                        break;

                    case 0xe6: // ANI byte
                        x = (byte)(registers.A & opcode[1]);
                        if (x == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((x & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.Cy = 0; // Carry Flag
                        registers.Flags.P = Intel8080Emulator.Parity(x, 8);
                        registers.A = x;
                        registers.Pc++;
                        break;

                    case 0xe7: // RST 4
                        ret = (ushort)(registers.Pc + 2);
                        registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                        registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                        registers.Sp = (ushort)(registers.Sp - 2);
                        registers.Pc = (ushort)0x20;
                        registers.Pc--;
                        break;

                    case 0xe8: // RPE
                        if (registers.Flags.P == 1)
                        {
                            registers.Pc = (ushort)((ushort)registers.memory[registers.Sp + 1] << 8 | (ushort)registers.memory[registers.Sp]);
                            registers.Sp += 2;
                        }
                        break;

                    case 0xe9: // PCHL
                        registers.Pc = (ushort)(((ushort)registers.H) << 8);
                        registers.Pc = (ushort)(registers.Pc | (ushort)registers.L);
                        registers.Pc--;
                        break;

                    case 0xea: // JPE adr
                        if (registers.Flags.P == 1)
                        {
                            registers.Pc = (ushort)((ushort)opcode[(byte)2] << 8 | (ushort)opcode[(byte)1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xeb: // XCHG
                        byte temp1 = registers.D;
                        registers.D = registers.H;
                        registers.H = temp1;
                        byte temp2 = registers.L;
                        registers.L = registers.E;
                        registers.E = temp2;
                        break;

                    case 0xec: // CPE adr
                        if (registers.Flags.P == 1)
                        {
                            ret = (ushort)(registers.Pc + 2);
                            registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                            registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                            registers.Sp = (ushort)(registers.Sp - 2);
                            registers.Pc = (ushort)((((ushort)opcode[2]) << (ushort)8) | (ushort)opcode[1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xed: // NOP
                        break;

                    case 0xee:
                        registers.A = (byte)(registers.A ^ opcode[1]);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Pc++;
                        break;

                    case 0xef: // RST 5
                        ret = (ushort)(registers.Pc + 2);
                        registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                        registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                        registers.Sp = (ushort)(registers.Sp - 2);
                        registers.Pc = (ushort)0x28;
                        registers.Pc--;
                        break;

                    case 0xf0: // RP
                        if (registers.Flags.P == 1)
                        {
                            registers.Pc = (ushort)((ushort)registers.memory[registers.Sp + 1] << 8 | (ushort)registers.memory[registers.Sp]);
                            registers.Sp += 2;
                        }
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

                    case 0xf2: // JP adr
                        if (registers.Flags.P == 1)
                        {
                            registers.Pc = (ushort)((ushort)opcode[(byte)2] << 8 | (ushort)opcode[(byte)1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xf3: // DI
                        registers.Int_enable = 0x00;
                        break;

                    case 0xf4: // CP adr
                        if (registers.Flags.S == 0)
                        {
                            ret = (ushort)(registers.Pc + 2);
                            registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                            registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                            registers.Sp = (ushort)(registers.Sp - 2);
                            registers.Pc = (ushort)((((ushort)opcode[2]) << (ushort)8) | (ushort)opcode[1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xf5: // PUSH PSW
                        registers.memory[registers.Sp - 1] = registers.A;
						psw = ((byte)(registers.Flags.Z | registers.Flags.S << 1 | registers.Flags.P << 2
                            | registers.Flags.Cy << 3 | registers.Flags.Ac << 4));
						registers.memory[registers.Sp - 2] = psw;
						registers.Sp = (ushort)(registers.Sp - (ushort)2);
						break;

                    case 0xf6: // ORI D8
                        registers.A = (byte)(registers.A | opcode[1]);
                        if ((registers.A & (ushort)0xff) == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        registers.Flags.P = Intel8080Emulator.Parity(registers.A, 8);
                        registers.Flags.Cy = 0;
                        if ((registers.A & (ushort)0x80) >= 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Pc++;
                        break;

                    case 0xf7: // RST 6
                        ret = (ushort)(registers.Pc + 2);
                        registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                        registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                        registers.Sp = (ushort)(registers.Sp - 2);
                        registers.Pc = (ushort)0x30;
                        registers.Pc--;
                        break;

                    case 0xf8: // RM
                        if (registers.Flags.S == 1)
                        {
                            registers.Pc = (ushort)((ushort)registers.memory[registers.Sp + 1] << 8 | (ushort)registers.memory[registers.Sp]);
                            registers.Sp += 2;
                        }
                        break;

                    case 0xf9: // SPHL
                        registers.Sp = (ushort)(((ushort)registers.H) << 8);
                        registers.Sp = (ushort)(registers.Sp | (ushort)registers.L);
                        break;

                    case 0xfa: // JM Addr
                        if (registers.Flags.S == 1)
                        {
                            registers.Pc = (ushort)((ushort)opcode[(byte)2] << 8 | (ushort)opcode[(byte)1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xfb:  // EI
                        registers.Int_enable = 0x01;
                        break;

                    case 0xfc: // CM ADR
                        if (registers.Flags.S == 1)
                        {
                            ret = (ushort)(registers.Pc + 2);
                            registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                            registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                            registers.Sp = (ushort)(registers.Sp - 2);
                            registers.Pc = (ushort)((((ushort)opcode[2]) << (ushort)8) | (ushort)opcode[1]);
                            registers.Pc--;
                        }
                        else registers.Pc += (ushort)2;
                        break;

                    case 0xfd: // NOP
                        break;

                    case 0xfe: // CPI byte
                        x = (byte)(registers.A - opcode[1]);
                        if (x == 0) registers.Flags.Z = 1; else registers.Flags.Z = 0; // Zero Flag
                        if ((x & 0x80) == 0x80) registers.Flags.S = 1; else registers.Flags.S = 0; // Sign Flag
                        registers.Flags.P = Intel8080Emulator.Parity(x, 8);
                        if ((registers.A < opcode[1])) registers.Flags.Cy = 1; else registers.Flags.Cy = 0; // Carry Flag
                        registers.Pc++;
                        break;

                    case 0xff: // RST 7
                        ret = (ushort)(registers.Pc + 2);
                        registers.memory[registers.Sp - 1] = (byte)((ret >> 8) & 0xff);
                        registers.memory[registers.Sp - 2] = (byte)((ret) & 0xff);
                        registers.Sp = (ushort)(registers.Sp - 2);
                        registers.Pc = (ushort)0x38;
                        registers.Pc--;
                        break;

                }
				registers.Pc += (ushort)1;
				//this.ProcessorState(registers);
                return cycleCount;
            }
        }
    }
}
