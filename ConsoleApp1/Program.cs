using System.ComponentModel.Design;
using System.Numerics;
using System.Text;


public class Disassembler
{
    public void Disassemble8080Op(byte[] codebuffer, UInt16 pc)
    {
        var disassembly = new StringBuilder();

        byte code = codebuffer[pc];
        int opSize = 1; // Operation size in bytes

        Console.WriteLine("{0:X4}", pc);
        switch (code)
        {
            case 0x00:
                disassembly.Append("NOP");
                disassembly.Append(Environment.NewLine);
                    break;
            case 0x01:
                disassembly.Append("LXI     B,#${0:X2}{0:X2}", codebuffer[pc + 1], codebuffer[pc + 2]); // Loads a 16 bit address into the register pair BC
                opSize = 3; 
                    break;
            case 0x02:
                disassembly.Append("STAX    B");    // Store the value of the accumulator into the register pair BC (the address that they point to)
                disassembly.Append(Environment.NewLine);
                break;
            case 0x03:
                disassembly.Append("INX     B"); // 
            default:
                break;
        }
    }  


}


// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");



// while 1:
checked op dictionary using the currenty byte address 
