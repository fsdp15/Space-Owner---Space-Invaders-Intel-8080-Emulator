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
            byte opcode = registers.Memory[registers.Pc];

            switch (opcode)
            {
                case 0x00:
                    break;
		case 0x01:
		    registers.B = registers.Memory[registers.Pc+(byte)2];
		    registers.C = registers.Memory[registers.Pc+(byte)1];
		    registers.Pc+=(ushort)2;
		case 0x02:

            }

            registers.Pc+=(ushort)1;

            return 1;
        }
    }
}
