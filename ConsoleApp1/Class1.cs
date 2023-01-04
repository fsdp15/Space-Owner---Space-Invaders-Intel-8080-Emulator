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
        public static Dictionary<Byte, Operation> opDictionary = new Dictionary<byte, Operation>(); // Nao preciso construir um novo, so ja inicializar um dicionario com valores?

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
            opDictionary.Add(0x2e, new Operation(0x2e, "MVI    L,D8", 2)); //	 		L <- byte 2
            opDictionary.Add(0x2f, new Operation(0x2f, "CMA", 1)); //	A <- !A

            opDictionary.Add(0x31, new Operation(0x31, "LXI    SP,D16", 3)); // SP.hi < -byte 3, SP.lo < -byte 2
            opDictionary.Add(0x32, new Operation(0x32, "STA     adr", 3)); // 	(adr) <- A
            opDictionary.Add(0x33, new Operation(0x33, "INX     SP", 1)); // 		SP = SP + 1
            opDictionary.Add(0x34, new Operation(0x34, "INR     M", 1)); // 		(HL) <- (HL)+1
            opDictionary.Add(0x35, new Operation(0x35, "DCR     M", 1)); // 		(HL) <- (HL)-1
            opDictionary.Add(0x36, new Operation(0x36, "MVI     M,D8", 2)); // 		(HL) <- byte 2
            opDictionary.Add(0x37, new Operation(0x37, "STC", 1)); // 		CY = 1

            opDictionary.Add(0x39, new Operation(0x39, "DAP     SP", 1)); // 			HL = HL + SP
            opDictionary.Add(0x3a, new Operation(0x3a, "LDA     adr", 3)); // 			A <- (adr)
            opDictionary.Add(0x3b, new Operation(0x3b, "DCX     SP", 1)); // 			SP = SP-1
            opDictionary.Add(0x3c, new Operation(0x3c, "INR     A", 1)); // 			A <- A+1
            opDictionary.Add(0x3d, new Operation(0x3d, "DCR     A", 1)); // 			A <- A-1
            opDictionary.Add(0x3e, new Operation(0x3e, "MVI     A,D8", 2)); // 			A <- byte 2
            opDictionary.Add(0x3f, new Operation(0x3f, "CMC", 1)); // 		CY=!CY   CY is a flag

            opDictionary.Add(0x40, new Operation(0x40, "MOV     B,B", 1)); // 			B <- B
            opDictionary.Add(0x41, new Operation(0x41, "MOV     B,C", 1)); // 			B <- C
            opDictionary.Add(0x42, new Operation(0x42, "MOV     B,D", 1)); // 			B <- D
            opDictionary.Add(0x43, new Operation(0x43, "MOV     B,E", 1)); // 			B <- E
            opDictionary.Add(0x44, new Operation(0x44, "MOV     B,H", 1)); // 			B <- H
            opDictionary.Add(0x45, new Operation(0x45, "MOV     B,L", 1)); // 			B <- L
            opDictionary.Add(0x46, new Operation(0x46, "MOV     B,M", 1)); // 			B <- (HL)
            opDictionary.Add(0x47, new Operation(0x47, "MOV     B,A", 1)); // 			B <- A

            opDictionary.Add(0x48, new Operation(0x48, "MOV     C,B", 1)); // 			C <- B
            opDictionary.Add(0x49, new Operation(0x49, "MOV     C,C", 1)); // 			C <- C
            opDictionary.Add(0x4a, new Operation(0x4a, "MOV     C,D", 1)); // 			C <- D
            opDictionary.Add(0x4b, new Operation(0x4b, "MOV     C,E", 1)); // 			C <- E
            opDictionary.Add(0x4c, new Operation(0x4c, "MOV     C,H", 1)); // 			C <- H
            opDictionary.Add(0x4d, new Operation(0x4d, "MOV     C,L", 1)); // 			C <- L
            opDictionary.Add(0x4e, new Operation(0x4e, "MOV     C,M", 1)); // 			C <- (HL)
            opDictionary.Add(0x4f, new Operation(0x4f, "MOV     C,A", 1)); // 			C <- A

            opDictionary.Add(0x50, new Operation(0x50, "MOV     D,B", 1)); // 			D <- B
            opDictionary.Add(0x51, new Operation(0x51, "MOV     D,C", 1)); // 			D <- C
            opDictionary.Add(0x52, new Operation(0x52, "MOV     D,D", 1)); // 			D <- D
            opDictionary.Add(0x53, new Operation(0x53, "MOV     D,E", 1)); // 			D <- E
            opDictionary.Add(0x54, new Operation(0x54, "MOV     D,H", 1)); // 			D <- H
            opDictionary.Add(0x55, new Operation(0x55, "MOV     D,L", 1)); // 			D <- L
            opDictionary.Add(0x56, new Operation(0x56, "MOV     D,M", 1)); // 			D <- (HL)
            opDictionary.Add(0x57, new Operation(0x57, "MOV     D,A", 1)); // 			D <- A

            opDictionary.Add(0x58, new Operation(0x58, "MOV     E,B", 1)); // 			E <- B
            opDictionary.Add(0x59, new Operation(0x59, "MOV     E,C", 1)); // 			E <- C
            opDictionary.Add(0x5a, new Operation(0x5a, "MOV     E,D", 1)); // 			E <- D
            opDictionary.Add(0x5b, new Operation(0x5b, "MOV     E,E", 1)); // 			E <- E
            opDictionary.Add(0x5c, new Operation(0x5c, "MOV     E,H", 1)); // 			E <- H
            opDictionary.Add(0x5d, new Operation(0x5d, "MOV     E,L", 1)); // 			E <- L
            opDictionary.Add(0x5e, new Operation(0x5e, "MOV     E,M", 1)); // 			E <- (HL)
            opDictionary.Add(0x5f, new Operation(0x5f, "MOV     E,A", 1)); // 			E <- A

            opDictionary.Add(0x60, new Operation(0x60, "MOV     H,B", 1)); // 			H <- B
            opDictionary.Add(0x61, new Operation(0x61, "MOV     H,C", 1)); // 			H <- C
            opDictionary.Add(0x62, new Operation(0x62, "MOV     H,D", 1)); // 			H <- D
            opDictionary.Add(0x63, new Operation(0x63, "MOV     H,E", 1)); // 			H <- E
            opDictionary.Add(0x64, new Operation(0x64, "MOV     H,H", 1)); // 			H <- H
            opDictionary.Add(0x65, new Operation(0x65, "MOV     H,L", 1)); // 			H <- L
            opDictionary.Add(0x66, new Operation(0x66, "MOV     H,M", 1)); // 			H <- (HL)
            opDictionary.Add(0x67, new Operation(0x67, "MOV     H,A", 1)); // 			H <- A

            opDictionary.Add(0x68, new Operation(0x68, "MOV     L,B", 1)); // 			L <- B
            opDictionary.Add(0x69, new Operation(0x69, "MOV     L,C", 1)); // 			L <- C
            opDictionary.Add(0x6a, new Operation(0x6a, "MOV     L,D", 1)); // 			L <- D
            opDictionary.Add(0x6b, new Operation(0x6b, "MOV     L,E", 1)); // 			L <- E
            opDictionary.Add(0x6c, new Operation(0x6c, "MOV     L,H", 1)); // 			L <- H
            opDictionary.Add(0x6d, new Operation(0x6d, "MOV     L,L", 1)); // 			L <- L
            opDictionary.Add(0x6e, new Operation(0x6e, "MOV     L,M", 1)); // 			L <- (HL)
            opDictionary.Add(0x6f, new Operation(0x6f, "MOV     L,A", 1)); // 			L <- A

            opDictionary.Add(0x70, new Operation(0x70, "MOV     M,B", 1)); // 			(HL) <- B
            opDictionary.Add(0x71, new Operation(0x71, "MOV     M,C", 1)); // 			(HL) <- C
            opDictionary.Add(0x72, new Operation(0x72, "MOV     M,D", 1)); // 			(HL) <- D
            opDictionary.Add(0x73, new Operation(0x73, "MOV     M,E", 1)); // 			(HL) <- E
            opDictionary.Add(0x74, new Operation(0x74, "MOV     M,H", 1)); // 			(HL) <- H
            opDictionary.Add(0x75, new Operation(0x75, "MOV     M,L", 1)); // 			(HL) <- L
            opDictionary.Add(0x76, new Operation(0x76, "HLT", 1)); // 		Halt
            opDictionary.Add(0x77, new Operation(0x77, "MOV     M,A", 1)); // 		    (HL) <- A

            opDictionary.Add(0x78, new Operation(0x78, "MOV     A,B", 1)); // 				A <- B
            opDictionary.Add(0x79, new Operation(0x79, "MOV     A,C", 1)); // 				A <- C
            opDictionary.Add(0x7a, new Operation(0x7a, "MOV     A,D", 1)); // 				A <- D
            opDictionary.Add(0x7b, new Operation(0x7b, "MOV     A,E", 1)); // 			    A <- E  
            opDictionary.Add(0x7c, new Operation(0x7c, "MOV     A,H", 1)); // 			    A <- H
            opDictionary.Add(0x7d, new Operation(0x7d, "MOV     A,L", 1)); // 			    A <- L
            opDictionary.Add(0x7e, new Operation(0x7e, "MOV     A,M", 1)); // 			    A <- (HL)
            opDictionary.Add(0x7f, new Operation(0x7f, "MOV     A,A", 1)); // 			    A <- A


            opDictionary.Add(0x80, new Operation(0x80, "ADD     B", 1)); // 				A <- A + B
            opDictionary.Add(0x81, new Operation(0x81, "ADD     C", 1)); // 				A <- A + C
            opDictionary.Add(0x82, new Operation(0x82, "ADD     D", 1)); // 				A <- A + D
            opDictionary.Add(0x83, new Operation(0x83, "ADD     E", 1)); // 			    A <- A + E
            opDictionary.Add(0x84, new Operation(0x84, "ADD     H", 1)); // 			    A <- A + H
            opDictionary.Add(0x85, new Operation(0x85, "ADD     L", 1)); // 			    A <- A + L
            opDictionary.Add(0x86, new Operation(0x86, "ADD     M", 1)); // 			    A <- A + (HL)
            opDictionary.Add(0x87, new Operation(0x87, "ADD     A", 1)); // 			    A <- A + A

            opDictionary.Add(0x88, new Operation(0x88, "ADC     B", 1)); // 				A <- A + B + CY
            opDictionary.Add(0x89, new Operation(0x89, "ADC     C", 1)); // 				A <- A + C + CY
            opDictionary.Add(0x8a, new Operation(0x8a, "ADC     D", 1)); // 				A <- A + D + CY
            opDictionary.Add(0x8b, new Operation(0x8b, "ADC     E", 1)); // 			    A <- A + E + CY
            opDictionary.Add(0x8c, new Operation(0x8c, "ADC     H", 1)); // 			    A <- A + H + CY
            opDictionary.Add(0x8d, new Operation(0x8d, "ADC     L", 1)); // 			    A <- A + L + CY
            opDictionary.Add(0x8e, new Operation(0x8e, "ADC     M", 1)); // 			   	A <- A + (HL) + CY
            opDictionary.Add(0x8f, new Operation(0x8f, "ADC     A", 1)); // 			    A <- A + A + CY
        }


    }
}
