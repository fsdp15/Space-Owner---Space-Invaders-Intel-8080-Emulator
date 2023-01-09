using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Intel8080Emulator
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
        public Dictionary<Byte, Operation> opDictionary = new Dictionary<byte, Operation>(); // Nao preciso construir um novo, so ja inicializar um dicionario com valores?

        public void populateOpDictionary()
        {
            opDictionary.Add(0x00, new Operation(0x00, "NOP", 1));
            opDictionary.Add(0x01, new Operation(0x01, "LXI     B,D16", 3)); // Loads a 16 bit address into the register pair BC
            opDictionary.Add(0x02, new Operation(0x02, "STAX    B", 1));  // Store the value of the accumulator into the register pair BC (the address that they point to)
            opDictionary.Add(0x03, new Operation(0x03, "INX     B", 1)); // 	BC <- BC+1
            opDictionary.Add(0x04, new Operation(0x04, "INR     B", 1)); // 	B <- B+1
            opDictionary.Add(0x05, new Operation(0x05, "DCR     B", 1)); // 	B <- B-1
            opDictionary.Add(0x06, new Operation(0x06, "MVI     B,D8", 2)); // 	B <- byte 2
            opDictionary.Add(0x07, new Operation(0x07, "RLC", 1)); // 	A = A << 1; bit 0 = prev bit 7; CY = prev bit 7
            opDictionary.Add(0x08, new Operation(0x08, "NOP", 1));

            opDictionary.Add(0x09, new Operation(0x09, "DAD     B", 1)); // 	HL = HL + BC
            opDictionary.Add(0x0a, new Operation(0x0a, "LDAX    B", 1)); // 		A <- (BC)
            opDictionary.Add(0x0b, new Operation(0x0b, "DCX     B", 1)); // 		BC = BC-1
            opDictionary.Add(0x0c, new Operation(0x0c, "INR     C", 1)); // 		C <- C+1
            opDictionary.Add(0x0d, new Operation(0x0d, "DCR     C", 1)); // 		C <- C-1
            opDictionary.Add(0x0e, new Operation(0x0e, "MVI     C,D8", 2)); // 		C <- byte 2
            opDictionary.Add(0x0f, new Operation(0x0f, "RRC", 1)); // 			A = A >> 1; bit 7 = prev bit 0; CY = prev bit 0

            opDictionary.Add(0x10, new Operation(0x10, "NOP", 1));
            opDictionary.Add(0x11, new Operation(0x11, "LXI     D,D16", 3)); // 		D <- byte 3, E <- byte 2
            opDictionary.Add(0x12, new Operation(0x12, "STAX    D", 1)); // 		// Store the value of the accumulator into the register pair DE (the address that they point to)
            opDictionary.Add(0x13, new Operation(0x13, "INX     D", 1)); // DE <- DE + 1
            opDictionary.Add(0x14, new Operation(0x14, "INR     D", 1)); //	D <- D+1
            opDictionary.Add(0x15, new Operation(0x15, "DCR     D", 1)); //	D <- D-1
            opDictionary.Add(0x16, new Operation(0x16, "MVI     D,D8", 2)); //	D <- byte 2
            opDictionary.Add(0x17, new Operation(0x17, "RAL", 1)); //		A = A << 1; bit 0 = prev CY; CY = prev bit 7
            opDictionary.Add(0x18, new Operation(0x18, "NOP", 1));

            opDictionary.Add(0x19, new Operation(0x19, "DAD     D", 1)); //		HL = HL + DE
            opDictionary.Add(0x1a, new Operation(0x1a, "LDAX    D", 1)); //			A <- (DE)
            opDictionary.Add(0x1b, new Operation(0x1b, "DCX     D", 1)); //			DE = DE-1
            opDictionary.Add(0x1c, new Operation(0x1c, "INR     E", 1)); //				E <-E+1
            opDictionary.Add(0x1d, new Operation(0x1d, "DCR     E", 1)); //				E <- E-1
            opDictionary.Add(0x1e, new Operation(0x1e, "MVI     E,D8", 2)); //					E <- byte 2
            opDictionary.Add(0x1f, new Operation(0x1f, "RAR", 1)); //					E <- byte 2
            opDictionary.Add(0x20, new Operation(0x20, "NOP", 1));

            opDictionary.Add(0x21, new Operation(0x21, "LXI     H,D16", 3)); //	 H <- byte 3, L <- byte 2
            opDictionary.Add(0x22, new Operation(0x22, "SHLD    adr", 3)); //	 (adr) <-L; (adr+1)<-H
            opDictionary.Add(0x23, new Operation(0x23, "INX     H", 1)); //	 HL <- HL + 1
            opDictionary.Add(0x24, new Operation(0x24, "INR     H", 1)); //	 	H <- H+1
            opDictionary.Add(0x25, new Operation(0x25, "DCR     H", 1)); //	 	H <- H-1
            opDictionary.Add(0x26, new Operation(0x26, "MVI     H,D8", 2)); //	 	H <- byte 2
            opDictionary.Add(0x27, new Operation(0x27, "DAA", 1)); //	 	special
            opDictionary.Add(0x28, new Operation(0x28, "NOP", 1));

            opDictionary.Add(0x29, new Operation(0x29, "DAD     H", 1)); //	 HL = HL + HI
            opDictionary.Add(0x2a, new Operation(0x2a, "LHLD    adr", 3)); //	 	L <- (adr); H<-(adr+1)
            opDictionary.Add(0x2b, new Operation(0x2b, "DCX     H", 1)); //	 		HL = HL-1
            opDictionary.Add(0x2c, new Operation(0x2c, "INR     L", 1)); //	 		L <- L+1
            opDictionary.Add(0x2d, new Operation(0x2d, "DCR     L", 1)); //	 		L <- L-1
            opDictionary.Add(0x2e, new Operation(0x2e, "MVI     L,D8", 2)); //	 		L <- byte 2
            opDictionary.Add(0x2f, new Operation(0x2f, "CMA", 1)); //	A <- !A
            opDictionary.Add(0x30, new Operation(0x30, "NOP", 1));

            opDictionary.Add(0x31, new Operation(0x31, "LXI     SP,D16", 3)); // SP.hi < -byte 3, SP.lo < -byte 2
            opDictionary.Add(0x32, new Operation(0x32, "STA     adr", 3)); // 	(adr) <- A
            opDictionary.Add(0x33, new Operation(0x33, "INX     SP", 1)); // 		SP = SP + 1
            opDictionary.Add(0x34, new Operation(0x34, "INR     M", 1)); // 		(HL) <- (HL)+1
            opDictionary.Add(0x35, new Operation(0x35, "DCR     M", 1)); // 		(HL) <- (HL)-1
            opDictionary.Add(0x36, new Operation(0x36, "MVI     M,D8", 2)); // 		(HL) <- byte 2
            opDictionary.Add(0x37, new Operation(0x37, "STC", 1)); // 		CY = 1

            opDictionary.Add(0x38, new Operation(0x38, "NOP", 1));

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

            opDictionary.Add(0x90, new Operation(0x90, "SUB     B", 1)); // 			    A <- A - B
            opDictionary.Add(0x91, new Operation(0x91, "SUB     C", 1)); // 			    A <- A - C
            opDictionary.Add(0x92, new Operation(0x92, "SUB     D", 1)); // 			    A <- A - D
            opDictionary.Add(0x93, new Operation(0x93, "SUB     E", 1)); // 			    A <- A - E
            opDictionary.Add(0x94, new Operation(0x94, "SUB     H", 1)); // 			    A <- A - H
            opDictionary.Add(0x95, new Operation(0x95, "SUB     L", 1)); // 			    A <- A - L
            opDictionary.Add(0x96, new Operation(0x96, "SUB     M", 1)); // 			    A <- A - (HL)
            opDictionary.Add(0x97, new Operation(0x97, "SUB     A", 1)); // 			    A <- A - A


            opDictionary.Add(0x98, new Operation(0x98, "SBB     B", 1)); // 			    A <- A - B - CY
            opDictionary.Add(0x99, new Operation(0x99, "SBB     C", 1)); // 			    A <- A - C - CY
            opDictionary.Add(0x9a, new Operation(0x9a, "SBB     D", 1)); // 			    A <- A - D - CY 
            opDictionary.Add(0x9b, new Operation(0x9b, "SBB     E", 1)); // 			    A <- A - E - CY 
            opDictionary.Add(0x9c, new Operation(0x9c, "SBB     H", 1)); // 			    A <- A - H - CY 
            opDictionary.Add(0x9d, new Operation(0x9d, "SBB     L", 1)); // 			    A <- A - L - CY 
            opDictionary.Add(0x9e, new Operation(0x9e, "SBB     M", 1)); // 			    A <- A - (HL) - CY 
            opDictionary.Add(0x9f, new Operation(0x9e, "SBB     A", 1)); // 			    A <- A - A - CY

            opDictionary.Add(0xa0, new Operation(0xa0, "ANA     B", 1)); // 			    A <- A & B
            opDictionary.Add(0xa1, new Operation(0xa1, "ANA     C", 1)); // 			    A <- A & C
            opDictionary.Add(0xa2, new Operation(0xa2, "ANA     D", 1)); // 			    A <- A & D
            opDictionary.Add(0xa3, new Operation(0xa3, "ANA     E", 1)); // 			    A <- A & E
            opDictionary.Add(0xa4, new Operation(0xa4, "ANA     H", 1)); // 			    A <- A & H
            opDictionary.Add(0xa5, new Operation(0xa5, "ANA     L", 1)); // 			    A <- A & L
            opDictionary.Add(0xa6, new Operation(0xa6, "ANA     M", 1)); // 			    A <- A & (HL) 
            opDictionary.Add(0xa7, new Operation(0xa7, "ANA     A", 1)); // 			    A <- A & A

            opDictionary.Add(0xa8, new Operation(0xa8, "XRA     B", 1)); // 			    A <- A ^ B // Exclusive OR
            opDictionary.Add(0xa9, new Operation(0xa9, "XRA     C", 1)); // 			    A <- A ^ C
            opDictionary.Add(0xaa, new Operation(0xaa, "XRA     D", 1)); // 			    A <- A ^ D
            opDictionary.Add(0xab, new Operation(0xab, "XRA     E", 1)); // 			    A <- A ^ E
            opDictionary.Add(0xac, new Operation(0xac, "XRA     H", 1)); // 			    A <- A ^ H
            opDictionary.Add(0xad, new Operation(0xad, "XRA     L", 1)); // 			    A <- A ^ L
            opDictionary.Add(0xae, new Operation(0xae, "XRA     M", 1)); // 			    A <- A ^ (HL) 
            opDictionary.Add(0xaf, new Operation(0xaf, "XRA     A", 1)); // 			    A <- A ^ A

            opDictionary.Add(0xb0, new Operation(0xa8, "ORA     B", 1)); // 			    A <- A | B
            opDictionary.Add(0xb1, new Operation(0xa9, "ORA     C", 1)); // 			    A <- A | C
            opDictionary.Add(0xb2, new Operation(0xaa, "ORA     D", 1)); // 			    A <- A | D
            opDictionary.Add(0xb3, new Operation(0xab, "ORA     E", 1)); // 			    A <- A | E
            opDictionary.Add(0xb4, new Operation(0xac, "ORA     H", 1)); // 			    A <- A | H
            opDictionary.Add(0xb5, new Operation(0xad, "ORA     L", 1)); // 			    A <- A | L
            opDictionary.Add(0xb6, new Operation(0xae, "ORA     M", 1)); // 			    A <- A | (HL) 
            opDictionary.Add(0xb7, new Operation(0xaf, "ORA     A", 1)); // 			    A <- A | A

            opDictionary.Add(0xb8, new Operation(0xa8, "CMP     B", 1)); // 			    A <- A - B // compare, EQUAL, greater, less...
            opDictionary.Add(0xb9, new Operation(0xa9, "CMP     C", 1)); // 			    A <- A - C
            opDictionary.Add(0xba, new Operation(0xaa, "CMP     D", 1)); // 			    A <- A - D
            opDictionary.Add(0xbb, new Operation(0xab, "CMP     E", 1)); // 			    A <- A - E
            opDictionary.Add(0xbc, new Operation(0xac, "CMP     H", 1)); // 			    A <- A - H
            opDictionary.Add(0xbd, new Operation(0xad, "CMP     L", 1)); // 			    A <- A - L
            opDictionary.Add(0xbe, new Operation(0xae, "CMP     M", 1)); // 			    A <- A - (HL) 
            opDictionary.Add(0xbf, new Operation(0xaf, "CMP     A", 1)); // 			    A <- A - A

            opDictionary.Add(0xc0, new Operation(0xc0, "RNZ", 1)); // 			    Return if Not Zero -- Verifies if the Zero bit is not zero
            opDictionary.Add(0xc1, new Operation(0xc1, "POP     B", 1)); // 			    	C <- (sp); B <- (sp+1); sp <- sp+2
            opDictionary.Add(0xc2, new Operation(0xc2, "JNZ     addr", 3)); // 			    		if NZ, PC <- adr // Usado para IFs
            opDictionary.Add(0xc3, new Operation(0xc3, "JMP     addr", 3)); // 			    		PC <= adr
            opDictionary.Add(0xc4, new Operation(0xc4, "CNZ     addr", 3)); // 			    		if NZ, CALL adr
            opDictionary.Add(0xc5, new Operation(0xc5, "PUSH    B", 1)); // 			   	(sp-2)<-C; (sp-1)<-B; sp <- sp - 2
            opDictionary.Add(0xc6, new Operation(0xc6, "ADI     D8", 2)); // 			   A <- A + byte
            opDictionary.Add(0xc7, new Operation(0xc7, "RST     0", 2)); // 			   	CALL $0 -> interrupts
            opDictionary.Add(0xc8, new Operation(0xc8, "RZ", 1)); // 			   	if Z, RET
            opDictionary.Add(0xc9, new Operation(0xc9, "RET", 1)); // 			   	PC.lo <- (sp); PC.hi<-(sp+1); SP <- SP+2
            opDictionary.Add(0xca, new Operation(0xca, "JZ      adr", 3)); // 			   	if Z, PC <- adr

            opDictionary.Add(0xcc, new Operation(0xcc, "CZ      adr", 3)); // 			    if Z, CALL adr
            opDictionary.Add(0xcd, new Operation(0xcd, "CALL    adr", 3)); // 			(SP-1)<-PC.hi;(SP-2)<-PC.lo;SP<-SP-2;PC=adr
            opDictionary.Add(0xce, new Operation(0xce, "ACI     D8", 2)); //               	A <- A + data + CY

            opDictionary.Add(0xcf, new Operation(0xcf, "RST     1", 1)); //               	CALL $8

            opDictionary.Add(0xd0, new Operation(0xd0, "RNC", 1)); //               		if NCY, RET
            opDictionary.Add(0xd1, new Operation(0xd1, "POP     D", 1)); //               	E <- (sp); D <- (sp+1); sp <- sp+2
            opDictionary.Add(0xd2, new Operation(0xd2, "JNC     adr", 3)); //              if NCY, PC<-adr
            opDictionary.Add(0xd3, new Operation(0xd3, "OUT     D8", 2)); //               Output
            opDictionary.Add(0xd4, new Operation(0xd4, "CNC     adr", 3)); //              if NCY, CALL adr
            opDictionary.Add(0xd5, new Operation(0xd5, "PUSH    D", 1)); //              	(sp-2)<-E; (sp-1)<-D; sp <- sp - 2
            opDictionary.Add(0xd6, new Operation(0xd6, "SUI     D8", 2)); //              	A <- A - data
            opDictionary.Add(0xd7, new Operation(0xd7, "RST     2", 1)); //              	CALL $10
            opDictionary.Add(0xd8, new Operation(0xd8, "RC", 1)); //              	        if CY, RET


            opDictionary.Add(0xda, new Operation(0xda, "JC      adr", 3)); //              	     if CY, PC<-adr
            opDictionary.Add(0xdb, new Operation(0xdb, "IN      D8", 2)); //              	Input
            opDictionary.Add(0xdc, new Operation(0xdb, "CC      adr", 3)); //              	     if CY,CALL adr

            opDictionary.Add(0xde, new Operation(0xde, "SBI     D8", 2)); //              	     A <- A - data - CY
            opDictionary.Add(0xdf, new Operation(0xdf, "RST     3", 1)); //              	CALL $18
            opDictionary.Add(0xe0, new Operation(0xe0, "RPO", 1)); //              		if pair flag is odd, return. PO = Parity ODD
            opDictionary.Add(0xe1, new Operation(0xe1, "POP     H", 1)); //              			L <- (sp); H <- (sp+1); sp <- sp+2
            opDictionary.Add(0xe2, new Operation(0xe2, "JPO     adr", 3)); //              			if PO, PC <- adr
            opDictionary.Add(0xe3, new Operation(0xe3, "XTHL    adr", 1)); //              				L <-> (SP); H <-> (SP+1) | <-> means exchanged



            opDictionary.Add(0xe4, new Operation(0xe4, "CPO     adr", 3)); //              			if PO, CALL adr
            opDictionary.Add(0xe5, new Operation(0xe5, "PUSH    H", 1)); //              				(sp-2)<-L; (sp-1)<-H; sp <- sp - 2
            opDictionary.Add(0xe6, new Operation(0xe6, "ANI     D8", 2)); //              					A <- A & data (immediate)
            opDictionary.Add(0xe7, new Operation(0xe7, "RST     4", 1)); //              					CALL $20
            opDictionary.Add(0xe8, new Operation(0xe8, "RPE", 1)); //              						if PE, RET // parity even
            opDictionary.Add(0xe9, new Operation(0xe9, "PCHL", 1)); //              						PC.hi <- H; PC.lo <- L
            opDictionary.Add(0xea, new Operation(0xea, "JPE     adr", 3)); //              				if PE, PC <- adr
            opDictionary.Add(0xeb, new Operation(0xeb, "XCHG", 1)); //              					H <-> D; L <-> E
            opDictionary.Add(0xec, new Operation(0xec, "CPE     adr", 3)); //              					if PE, CALL adr

            opDictionary.Add(0xee, new Operation(0xee, "XRI     D8", 2)); //                    	A <- A ^ data
            opDictionary.Add(0xef, new Operation(0xef, "RST     5", 1)); //                    CALL $28
            opDictionary.Add(0xf0, new Operation(0xf0, "RP", 1)); //                If the Sign bit is zero (indicating a positive result). a return operation is performed.
            opDictionary.Add(0xf1, new Operation(0xf1, "POP     PSW", 1)); //              	flags <- (sp); A <- (sp+1); sp <- sp+2
            opDictionary.Add(0xf2, new Operation(0xf2, "JP     adr", 1)); //              		if P=1 PC <- adr
            opDictionary.Add(0xf3, new Operation(0xf3, "DI", 1)); //              		special
            opDictionary.Add(0xf4, new Operation(0xf4, "CP      adr", 1)); //              			if P, PC <- adr
            opDictionary.Add(0xf5, new Operation(0xf5, "PUSH    PSW", 1)); //              				(sp-2)<-flags; (sp-1)<-A; sp <- sp - 2
            opDictionary.Add(0xf6, new Operation(0xf6, "ORI     D8", 2)); //              				A <- A | data
            opDictionary.Add(0xf7, new Operation(0xf7, "RST     6", 1)); //              				CALL $30
            opDictionary.Add(0xf8, new Operation(0xf8, "RM", 1)); //              					if M, RET -- Return if Minus. Sign Bit is one
            opDictionary.Add(0xf9, new Operation(0xf9, "SPHL", 1)); //              				SP=HL
            opDictionary.Add(0xfa, new Operation(0xfa, "JM      adr", 3)); //              					if M, PC <- adr
            opDictionary.Add(0xfb, new Operation(0xfb, "EI", 1)); //              				special
            opDictionary.Add(0xfc, new Operation(0xfc, "CM      adr", 3)); //              				if M, CALL adr

            opDictionary.Add(0xfe, new Operation(0xfe, "CPI     D8", 2)); //              					A - data
            opDictionary.Add(0xff, new Operation(0xff, "RST     7", 1)); //              					CALL $38

        }


    }
}
