using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Operation
    {
        private byte opCode;
        private String instruction;
        private int opSize;

        public Operation(byte opCode, string instruction, int opSize)
        {
            this.opCode = opCode;
            this.instruction = instruction;
            this.opSize = opSize;
        }

        public int OpSize { get => opSize; set => opSize = value; }
        public string Instruction { get => instruction; set => instruction = value; }
        public byte OpCode { get => opCode; set => opCode = value; }

    }

    public class InstructionSet
    {
        public static Dictionary<Byte, Operation> opDictionary = new Dictionary<byte, Operation>();

        public void populateOpDictionary()
        {
            opDictionary.Add(0x00, new Operation(0x00, "NOP", 1));
            opDictionary.Add(0x01, new Operation(0x01, "LXI     B,D16", 3)); // Loads a 16 bit address into the register pair BC
            opDictionary.Add(0x02, new Operation(0x02, "STAX    B", 1));  // Store the value of the accumulator into the register pair BC (the address that they point to)
            opDictionary.Add(0x03, new Operation(0x03, "INX    B", 1)); // 	BC <- BC+1
            opDictionary.Add(0x04, new Operation(0x04, "INR    B", 1)); // 	B <- B+1
            opDictionary.Add(0x05, new Operation(0x05, "DCR    B", 1)); // 	B <- B-1
            opDictionary.Add(0x06, new Operation(0x06, "MVI    B,D8", 2)); // 	B <- byte 2
            opDictionary.Add(0x07, new Operation(0x07, "RLC", 1)); // 	A = A << 1; bit 0 = prev bit 7; CY = prev bit 7

            opDictionary.Add(0x09, new Operation(0x09, "DAD    B", 1)); // 	HL = HL + BC
            opDictionary.Add(0x0a, new Operation(0x0a, "LDAX    B", 1)); // 		A <- (BC)
            opDictionary.Add(0x0b, new Operation(0x0b, "DCX     B", 1)); // 		BC = BC-1
            opDictionary.Add(0x0c, new Operation(0x0c, "INR     C", 1)); // 		C <- C+1
            opDictionary.Add(0x0d, new Operation(0x0d, "DCR     C", 1)); // 		C <- C-1
            opDictionary.Add(0x0e, new Operation(0x0e, "MVI     C,D8", 2)); // 		C <- byte 2
            opDictionary.Add(0x0f, new Operation(0x0f, "RRC", 1)); // 			A = A >> 1; bit 7 = prev bit 0; CY = prev bit 0

            opDictionary.Add(0x11, new Operation(0x11, "LXI     D,D16", 3)); // 		D <- byte 3, E <- byte 2
            opDictionary.Add(0x12, new Operation(0x12, "STAX     D", 1)); // 		// Store the value of the accumulator into the register pair DE (the address that they point to)
            opDictionary.Add(0x13, new Operation(0x13, "INX      D", 1)); // DE <- DE + 1
            opDictionary.Add(0x14, new Operation(0x14, "INR       D", 1)); //	D <- D+1
            opDictionary.Add(0x15, new Operation(0x15, "DCR       D", 1)); //	D <- D-1
            opDictionary.Add(0x16, new Operation(0x16, "MVI       D,D8", 2)); //	D <- byte 2
            opDictionary.Add(0x17, new Operation(0x17, "RAL", 1)); //		A = A << 1; bit 0 = prev CY; CY = prev bit 7

            opDictionary.Add(0x19, new Operation(0x19, "DAD     D", 1)); //		HL = HL + DE
            opDictionary.Add(0x1a, new Operation(0x1a, "LDAX     D", 1)); //			A <- (DE)
            opDictionary.Add(0x1b, new Operation(0x1b, "DCX     D", 1)); //			DE = DE-1
            opDictionary.Add(0x1c, new Operation(0x1c, "INR     E", 1)); //				E <-E+1
            opDictionary.Add(0x1d, new Operation(0x1d, "DCR     E", 1)); //				E <- E-1
            opDictionary.Add(0x1e, new Operation(0x1e, "MVI     E,D8", 2)); //					E <- byte 2
            opDictionary.Add(0x1f, new Operation(0x1f, "RAR", 1)); //					E <- byte 2

            opDictionary.Add(0x21, new Operation(0x21, "LXI     H,D16", 3)); //	 H <- byte 3, L <- byte 2
            opDictionary.Add(0x22, new Operation(0x22, "SHLD    adr", 3)); //	 (adr) <-L; (adr+1)<-H
            opDictionary.Add(0x23, new Operation(0x23, "INX    H", 1)); //	 HL <- HL + 1
            opDictionary.Add(0x24, new Operation(0x24, "INR    H", 1)); //	 	H <- H+1
            opDictionary.Add(0x25, new Operation(0x25, "DCR    H", 1)); //	 	H <- H-1
            opDictionary.Add(0x26, new Operation(0x26, "MVI    H,D8", 2)); //	 	H <- byte 2
            opDictionary.Add(0x27, new Operation(0x27, "DAA", 1)); //	 	special

            opDictionary.Add(0x29, new Operation(0x29, "DAD    H", 1)); //	 HL = HL + HI
            opDictionary.Add(0x2a, new Operation(0x2a, "LHLD    adr", 3)); //	 	L <- (adr); H<-(adr+1)
            opDictionary.Add(0x2b, new Operation(0x2b, "DCX    H", 1)); //	 		HL = HL-1
            opDictionary.Add(0x2c, new Operation(0x2c, "INR    L", 1)); //	 		L <- L+1
            opDictionary.Add(0x2d, new Operation(0x2d, "DCR    L", 1)); //	 		L <- L-1
            opDictionary.Add(0x2e, new Operation(0x2e, "MVI    L,D8", 1)); //	 		L <- byte 2
            opDictionary.Add(0x2f, new Operation(0x2f, "CMA", 1)); //	A <- !A


        }


    }
}
