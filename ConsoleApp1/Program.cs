using ConsoleApp1;
using System.ComponentModel.Design;
using System.Numerics;
using System.Text;
using Intel8080Emulator;
using SDL2;
using System;
using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Data;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

//System.Threading.Thread.Sleep(5000);

Intel8080Emulator.Intel8080Emulator intel8080Emulator = new();
Thread newThread1 = new Thread(new ThreadStart(ThreadMethod1));
Thread newThread2 = new Thread(new ThreadStart(ThreadMethod2));
newThread1.Start();
newThread2.Start();

//Disassembler disassembler = new Disassembler();
//disassembler.ReadRom();

void ThreadMethod1() {
    intel8080Emulator.ReadRom(intel8080Emulator.registers);
    //intel8080Emulator.ReadTestRom();
}



// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

void ThreadMethod2()
{

    IntPtr window;
    IntPtr renderer;
	IntPtr texture;
    bool running = true;

    Setup();

	var time = new Stopwatch();
	double lastTimer = 0.0;
	double nextRefreshTime = 0.0;
	time.Start();


	const int FPS = 60;
	const int frameDelay = 1000 / FPS;
	UInt32 frameStart;
	UInt32 frameTime;
	//int count = 0;
	while (running)
    {
		frameStart = SDL.SDL_GetTicks();


		PollEvents();
            //Console.Write(count);
            Render();

		frameTime = SDL.SDL_GetTicks() - frameStart;
		if (frameDelay > frameTime)
		{
			SDL.SDL_Delay(frameDelay - frameTime);
		}
		// count++;
	}

    //Console.Write("aaa");

    CleanUp();

    /// Setup all of the SDL resources we'll need to display a window.
    void Setup()
    {

        // Initializes SDL
        if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
        {
            Console.WriteLine($"There was an issue initializing SDL. {SDL.SDL_GetError()}");
        }

        // Create a new window given a title, size, and passes it a a flag indicating it should be shown.
        window = SDL.SDL_CreateWindow(
            "Space Invaders Emulator",
            SDL.SDL_WINDOWPOS_UNDEFINED,
            SDL.SDL_WINDOWPOS_UNDEFINED,
            224,
            256,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

        if (window == IntPtr.Zero)
        {
            Console.WriteLine($"There was an issue creating the window. {SDL.SDL_GetError()}");
        }

        // Creates a new SDL hardware renderer using the default graphics device with VSYNC enabled.
        renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
            SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

        if (renderer == IntPtr.Zero)
        {
            Console.WriteLine($"There was an issue creating the renderer. {SDL.SDL_GetError()}");
        }
    }

    // Checks to see if there are any events to be processed.
    void PollEvents()
    {
        // Check to see if there any events and continue to do so until the queue is empty.
        while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
        {
            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_QUIT:
                    running = false;
                    break;
            }
        }
    }

    // Renders to the window
    void Render()
	{


		unsafe
		{
			// Sets the color that the screen will be cleared with.
			SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
			// Clears the current render surface
			SDL.SDL_RenderClear(renderer);

			/*	byte[,] newVram = new byte[256, 28]; // VRAM Buffer in bytes

				int newRow = 0;
				for (int i =  9216; i < 16384; i+=28)
				{
					for (int j = 0; j < 28; j++)
					{
						newVram[newRow, j] = intel8080Emulator.registers.memory[i + j];
					}
					newRow++;
				}
				//  for (int i = 0; i <)
				for (int i = 0; i < 256; i++)
				{
					for (int k = 0; k < 28; k++)
					{
						for (int j = 0; j < 8; j++)
						{
					   //     if (newVram[i, k] != 0)
						 //   {
								int a = 1 << j;
								bool bit = (newVram[i, k] & (1 << j)) != 0;
								if (bit == true)
								{
									SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
									SDL.SDL_RenderDrawPoint(renderer, i, (k * 8) + (7-j));			
							}
								else
								{
									SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
									SDL.SDL_RenderDrawPoint(renderer, i, (k * 8) + (7-j));
							}
						//	}

						}
					}
				}

				SDL.SDL_RenderPresent(renderer); */




				for (var byteIndex = 0; byteIndex < 7168; byteIndex++)
				{
					//	Console.WriteLine(a);
					var loc = byteIndex + 9216;

					var value = intel8080Emulator.registers.memory[loc];
					var pixelIndex = byteIndex * 8;
					var sourceRow = pixelIndex / 256;
					var sourceCol = pixelIndex % 256;

					var destCol = sourceRow;
					var destRow = 256 - 1 - sourceCol;

					for (var bitIndex = 0; bitIndex < 8; bitIndex++)
					{
						bool bit = (value & (1 << bitIndex)) != 0;
						if (bit == true)
						{
							SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
							SDL.SDL_RenderDrawPoint(renderer, destCol, (destRow - bitIndex));
						}
						else
						{
							SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
							SDL.SDL_RenderDrawPoint(renderer, destCol, (destRow - bitIndex));
						}

					}

				}
				SDL.SDL_RenderPresent(renderer);
				//Console.WriteLine(a);

			// Console.Write("a");

		}
	}

    void CleanUp()
    {
        SDL.SDL_DestroyRenderer(renderer);
        SDL.SDL_DestroyWindow(window);
        SDL.SDL_Quit();
    }
}