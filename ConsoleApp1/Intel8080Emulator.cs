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
                    throw new UnimplementedInstruction("Instruction is not implemented");
                    break;
            }

            registers.Pc++;

            return 1;
        }
    }
}
