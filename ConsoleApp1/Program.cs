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
using static SDL2.SDL;
using System.Windows;
using System.Media;

Intel8080Emulator.Intel8080Emulator intel8080Emulator = new();
Thread newThread1 = new Thread(new ThreadStart(ThreadMethod1)); // CPU
Thread newThread2 = new Thread(new ThreadStart(ThreadMethod2)); // Video and Inputs
Thread newThread3 = new Thread(new ThreadStart(ThreadMethod3)); // Sound

newThread1.Start();
newThread2.Start();
newThread3.Start();

//Disassembler disassembler = new Disassembler();
//disassembler.ReadRom();

void ThreadMethod1() {
    intel8080Emulator.ReadRom(intel8080Emulator.registers);
    //intel8080Emulator.ReadTestRom();
}



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
            224 * 3,
            256 * 3,
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

    // Checks to see if there are any events to be processed. Control inputs
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
				case SDL.SDL_EventType.SDL_KEYDOWN:
					switch (e.key.keysym.sym)
					{
						case SDL2.SDL.SDL_Keycode.SDLK_u:
							intel8080Emulator.MachineKeyDown(1); // Port 0 Fire
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_j:
							intel8080Emulator.MachineKeyDown(2); // Port 0 Left
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_l:
							intel8080Emulator.MachineKeyDown(3); // Port 0 Right
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_1:
							//Console.WriteLine();
						//	intel8080Emulator.emulationLog.Append(String.Format("F1 Key Pressed", intel8080Emulator.ports.InPorts[1].ToString("X2")));
						//	intel8080Emulator.emulationLog.Append("\n");
							intel8080Emulator.MachineKeyDown(4); // Credit
							//intel8080Emulator.emulationLog.Append(String.Format("F1 Key pressed and after Machine KeyDown: InPorts[1] when MachineKeyDown == $0x{0:X}", intel8080Emulator.ports.InPorts[1].ToString("X2")));
						//	intel8080Emulator.emulationLog.Append("\n");
							//Console.WriteLine();
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_RCTRL:
							intel8080Emulator.MachineKeyDown(5); // 2P START
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_RETURN:
							intel8080Emulator.MachineKeyDown(6); // 1P START
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_d:
							intel8080Emulator.MachineKeyDown(7); // 1P SHOT
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_LEFT:
							intel8080Emulator.MachineKeyDown(8); // 1P LEFT
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_RIGHT:
							intel8080Emulator.MachineKeyDown(9); // 1P RIGHT
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_n:
							intel8080Emulator.MachineKeyDown(10); // 2P SHOT
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_o:
							intel8080Emulator.MachineKeyDown(11); // 2P LEFT
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_p:
							intel8080Emulator.MachineKeyDown(12); // 2P RIGHT
							break;
					}
					break;

				case SDL.SDL_EventType.SDL_KEYUP:
					switch (e.key.keysym.sym)
					{
						case SDL2.SDL.SDL_Keycode.SDLK_u:
							intel8080Emulator.MachineKeyUp(1); // Port 0 Fire
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_j:
							intel8080Emulator.MachineKeyUp(2); // Port 0 Left
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_l:
							intel8080Emulator.MachineKeyUp(3); // Port 0 Right
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_1:
							Console.WriteLine();
						//	intel8080Emulator.emulationLog.Append(String.Format("F1 Key Released", intel8080Emulator.ports.InPorts[1].ToString("X2")));
						//	intel8080Emulator.emulationLog.Append("\n");
							intel8080Emulator.MachineKeyUp(4); // Credit
						//	intel8080Emulator.emulationLog.Append(String.Format("F1 Key Released and after Machine KeyUp: InPorts[1] when MachineKeyDown == $0x{0:X}", intel8080Emulator.ports.InPorts[1].ToString("X2")));
						//	intel8080Emulator.emulationLog.Append("\n");
							Console.WriteLine();

							// botar o console log aqui

							break;
						case SDL2.SDL.SDL_Keycode.SDLK_RCTRL:
							intel8080Emulator.MachineKeyUp(5); // 2P START
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_RETURN:
							intel8080Emulator.MachineKeyUp(6); // 1P START
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_d:
							intel8080Emulator.MachineKeyUp(7); // 1P SHOT
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_LEFT:
							intel8080Emulator.MachineKeyUp(8); // 1P LEFT
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_RIGHT:
							intel8080Emulator.MachineKeyUp(9); // 1P RIGHT
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_n:
							intel8080Emulator.MachineKeyUp(10); // 2P SHOT
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_o:
							intel8080Emulator.MachineKeyUp(11); // 2P LEFT
							break;
						case SDL2.SDL.SDL_Keycode.SDLK_p:
							intel8080Emulator.MachineKeyUp(12); // 2P RIGHT
							break;
					}
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
					var rect = new SDL.SDL_Rect
					{
						x = destCol * 3,
						y = (destRow - bitIndex) * 3,
						w = 3,
						h = 3
					};
					if (bit == true)
						{
							SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
							//SDL.SDL_RenderDrawPoint(renderer, destCol, (destRow - bitIndex));
						}
						else
						{
							SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
						//	SDL.SDL_RenderDrawPoint(renderer, destCol, (destRow - bitIndex));
						}
					SDL.SDL_RenderFillRect(renderer, ref rect);
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
} // Draw Graphics

void ThreadMethod3()
{
	byte lastOutPort3 = new byte();
	byte lastOutPort5 = new byte();
#pragma warning disable CA1416 // Validate platform compatibility
	SoundPlayer player = new SoundPlayer();
#pragma warning restore CA1416 // Validate platform compatibility
	while (true)
	{
		if (lastOutPort3 != intel8080Emulator.ports.OutPorts[3]) {
			if (((intel8080Emulator.ports.OutPorts[3] & 0x01) == 0x01) && ((intel8080Emulator.ports.OutPorts[3] & 0x01) != (lastOutPort3 & 0x01)))
			{
				player.SoundLocation = "C:\\Users\\felip\\Documents\\git\\SpaceInvaders8080Emulator\\ConsoleApp1\\Audio\\ufo_lowpitch.wav";
				player.PlaySync();
				// Play UFO sound for a while
			}

			if (((intel8080Emulator.ports.OutPorts[3] & 0x02) == 0x02) && ((intel8080Emulator.ports.OutPorts[3] & 0x02) != (lastOutPort3 & 0x02)))
			{
				Console.WriteLine();
				player.SoundLocation = "C:\\Users\\felip\\Documents\\git\\SpaceInvaders8080Emulator\\ConsoleApp1\\Audio\\shoot.wav";
				player.PlaySync();
				// Play shot sound
			}

			if (((intel8080Emulator.ports.OutPorts[3] & 0x04) == 0x04) && ((intel8080Emulator.ports.OutPorts[3] & 0x04) != (lastOutPort3 & 0x04)))
			{
				player.SoundLocation = "C:\\Users\\felip\\Documents\\git\\SpaceInvaders8080Emulator\\ConsoleApp1\\Audio\\explosion.wav";
				player.PlaySync();
				// Player death sound
			}

			if (((intel8080Emulator.ports.OutPorts[3] & 0x08) == 0x08) && ((intel8080Emulator.ports.OutPorts[3] & 0x08) != (lastOutPort3 & 0x08)))
			{
				player.SoundLocation = "C:\\Users\\felip\\Documents\\git\\SpaceInvaders8080Emulator\\ConsoleApp1\\Audio\\invaderkilled.wav";
				player.PlaySync();
				// Player alien death sound
			}

			lastOutPort3 = intel8080Emulator.ports.OutPorts[3];
		}

		if (lastOutPort5 != intel8080Emulator.ports.OutPorts[5])
		{
			if (((intel8080Emulator.ports.OutPorts[5] & 0x01) == 0x01) && ((intel8080Emulator.ports.OutPorts[5] & 0x01) != (lastOutPort5 & 0x01)))
			{
				player.SoundLocation = "C:\\Users\\felip\\Documents\\git\\SpaceInvaders8080Emulator\\ConsoleApp1\\Audio\\fastinvader1.wav";
				player.PlaySync();
			}

			if (((intel8080Emulator.ports.OutPorts[5] & 0x02) == 0x02) && ((intel8080Emulator.ports.OutPorts[5] & 0x02) != (lastOutPort5 & 0x02)))
			{
				player.SoundLocation = "C:\\Users\\felip\\Documents\\git\\SpaceInvaders8080Emulator\\ConsoleApp1\\Audio\\fastinvader2.wav";
				player.PlaySync();
				// SX7 5.raw
			}

			if (((intel8080Emulator.ports.OutPorts[5] & 0x04) == 0x04) && ((intel8080Emulator.ports.OutPorts[5] & 0x04) != (lastOutPort5 & 0x04)))
			{
				player.SoundLocation = "C:\\Users\\felip\\Documents\\git\\SpaceInvaders8080Emulator\\ConsoleApp1\\Audio\\fastinvader3.wav";
				player.PlaySync();
			}

			if (((intel8080Emulator.ports.OutPorts[5] & 0x08) == 0x08) && ((intel8080Emulator.ports.OutPorts[5] & 0x08) != (lastOutPort5 & 0x08)))
			{
				player.SoundLocation = "C:\\Users\\felip\\Documents\\git\\SpaceInvaders8080Emulator\\ConsoleApp1\\Audio\\fastinvader4.wav";
				player.PlaySync();
			}

			if (((intel8080Emulator.ports.OutPorts[5] & 0x10) == 0x10) && ((intel8080Emulator.ports.OutPorts[5] & 0x10) != (lastOutPort5 & 0x10)))
			{
				player.SoundLocation = "C:\\Users\\felip\\Documents\\git\\SpaceInvaders8080Emulator\\ConsoleApp1\\Audio\\invaderkilled.wav";
				player.PlaySync();
			}

			lastOutPort5 = intel8080Emulator.ports.OutPorts[5];
		}
	}
}