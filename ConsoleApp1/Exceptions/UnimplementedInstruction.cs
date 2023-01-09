using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Intel8080Emulator.Exceptions
{
    internal class UnimplementedInstruction : Exception
    {
        public UnimplementedInstruction(string message) : base(message)
        {
            Console.WriteLine("Error: Unimplemented instruction\n");
            System.Environment.Exit(1);
        }

        protected UnimplementedInstruction(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            Console.WriteLine("Error: Unimplemented instruction\n");
            System.Environment.Exit(1);
        }
    }
}
