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
        public int Emulate8080Op(Registers registers)
        {
            byte* opcode = &registers.Memory[registers.Pc];

            switch (opcode) // I can implement this with a dictionary that poins to a... method? (delegate)
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


            }

            registers.Pc+=(ushort)1;

            return 1;
        }
    }
}
