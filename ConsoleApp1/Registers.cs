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
        byte* memory;
        Flags flags;
        byte int_enable;

        public Registers()
        {
            this.Flags = new Flags();

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
        public unsafe byte* Memory { get => memory; set => memory = value; }
        public Flags Flags { get => flags; set => flags = value; }
        public byte Int_enable { get => int_enable; set => int_enable = value; }
    }
}
