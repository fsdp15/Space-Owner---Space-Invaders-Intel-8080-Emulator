using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8080Emulator
{
    public class Flags
    {
        private byte z; // Zero bit
		private byte s; // Sign bit
		private byte p; // Parity bit
		private byte cy; // Carry bit
		private byte ac; // Auxiliary carry bit
		private byte pad;

        public Flags()
        {
            this.Z = 0;
            this.S = 0;
            this.P = 0;
            this.Cy = 0;
            this.Ac = 0;
            this.Pad = 3;
        }

        public byte Z { get => z; set => z = value; }
        public byte S { get => s; set => s = value; }
        public byte P { get => p; set => p = value; }
        public byte Cy { get => cy; set => cy = value; }
        public byte Ac { get => ac; set => ac = value; }
        public byte Pad { get => pad; set => pad = value; }
    }
}
