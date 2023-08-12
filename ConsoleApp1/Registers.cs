using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8080Emulator
{
    internal unsafe class Registers
    {
        const int INTERRUPT_ENABLED = 1;
        const int INTERRUPT_DISABLED = 0;

        private byte a;
		private byte b;
		private byte c;
		private byte d;
		private byte e;
		private byte h;
		private byte l;
		private ushort sp;
		private ushort pc;
        public byte[] memory; //16K
		private Flags flags;
		private byte int_enable;

        public Registers()
        {
            this.Flags = new Flags();
            this.memory = new byte[0x10000]; // = 8k bytes of memory = 65536 bits
			this.Int_enable = INTERRUPT_ENABLED;
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
