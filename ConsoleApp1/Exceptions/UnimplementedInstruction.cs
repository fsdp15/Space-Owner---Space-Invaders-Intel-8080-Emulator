using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Intel8080Emulator.Exceptions
{
    public class UnimplementedInstruction : Exception
    {
        public UnimplementedInstruction(string message) : base(message)
        {
            Console.WriteLine("Error: Unimplemented instruction: $0x{0:X}\n", message);
            using (System.IO.StreamWriter file = File.AppendText("C:\\Users\\felip\\OneDrive\\Desktop\\Emulator\\invaders\\invadersDebug.txt"))
            {
                file.WriteLine("Error: Unimplemented instruction: $0x{0:X}\n", message);
            }
            System.Environment.Exit(1);
        }

        protected UnimplementedInstruction(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            Console.WriteLine("Error: Unimplemented instruction\n");
            using (System.IO.StreamWriter file = File.AppendText("C:\\Users\\felip\\OneDrive\\Desktop\\Emulator\\invaders\\invadersDebug.txt"))
            {
                file.WriteLine("Error: Unimplemented instruction: $0x{0:X}\n");
            }
            System.Environment.Exit(1);
        }
    }
}
