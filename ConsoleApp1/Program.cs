using ConsoleApp1;
using System.ComponentModel.Design;
using System.Numerics;
using System.Text;
using Intel8080Emulator;



//Disassembler disassembler = new Disassembler();
//disassembler.ReadRom();

Intel8080Emulator.Intel8080Emulator intel8080Emulator = new();
intel8080Emulator.ReadRom();
//intel8080Emulator.ReadTestRom();



// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");








