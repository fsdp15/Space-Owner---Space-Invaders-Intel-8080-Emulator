using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8080Emulator
{
    internal class Ports
    {
        private byte shiftOffset;
        private byte shift1;
        private byte shift0;

        private byte[] inPorts = new byte[4];
        private byte[] outPorts = new byte[7];

        public byte ShiftOffset { get => shiftOffset; set => shiftOffset = value; }
        public byte Shift1 { get => shift1; set => shift1 = value; }
        public byte Shift0 { get => shift0; set => shift0 = value; }
        public byte[] InPorts { get => inPorts; set => inPorts = value; }
        public byte[] OutPorts { get => outPorts; set => outPorts = value; }
    }
}
