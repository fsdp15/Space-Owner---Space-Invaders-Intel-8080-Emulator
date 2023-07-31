using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8080Emulator
{
    internal unsafe class Registers
    {
        byte a;
        byte b;
        byte c;
        byte d;
        byte e;
        byte h;
        byte l;
        ushort sp;
        ushort pc;
        public byte[] memory; //16K // Encapsulate to not allow write before address 0x2000 or after address 0x4000
        Flags flags;
        byte int_enable;

        public Registers()
        {
            this.Flags = new Flags();
            this.memory = new byte[0x10000]; // 8k of memory = 65536 bytes
			this.Int_enable = 1;
        }

        public byte A { get => a; set => a = value; }
        public byte B { get => b; set => b = value; }
        public byte C { get => c; set => c = value; }
        public byte D { get => d; set => d = value; }
        public byte E { get => e; set => e = value; }
        public byte H { get => h; set => h = value; }
        public byte L { get => l; set => l = value; }
        public ushort Pc { get => pc; set => pc = value; }
        public ushort Sp { get => sp; set => sp = value; }
        public Flags Flags { get => flags; set => flags = value; }
        public byte Int_enable { get => int_enable; set => int_enable = value; }
    }
}
