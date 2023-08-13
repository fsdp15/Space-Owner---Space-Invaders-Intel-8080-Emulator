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

const int FPS = 60; // Limiting SDL to 60 FPS. The CPU fills VRAM at 60 fps.
const int frameDelay = 1000 / FPS;

Intel8080Emulator.Intel8080Emulator intel8080Emulator = new();
Thread newThread1 = new Thread(new ThreadStart(ThreadMethod1)); // CPU
Thread newThread2 = new Thread(new ThreadStart(ThreadMethod2)); // Video and Inputs

newThread1.IsBackground = true;

newThread1.Start();
newThread2.Start();


void ThreadMethod1() {
	//intel8080Emulator.ReadTestRom(); // For CPU testing if needed
	intel8080Emulator.ReadRom(intel8080Emulator.registers);

	intel8080Emulator.DoEmulation(intel8080Emulator.registers);
}

void ThreadMethod2() // SDL
{
	IntPtr window;
    IntPtr renderer;
    bool running = true;

    Setup();

	Thread newThread3 = new Thread(new ThreadStart(ThreadMethod3)); // Sound
	newThread3.IsBackground = true;
	newThread3.Start();

	var time = new Stopwatch();
	time.Start();


	UInt32 frameStart;
	UInt32 frameTime;

	while (running)
    {
		frameStart = SDL.SDL_GetTicks();
		PollEvents();
        Render();
		frameTime = SDL.SDL_GetTicks() - frameStart;
		if (frameDelay > frameTime)
		{
			SDL.SDL_Delay(frameDelay - frameTime);
		}
	}

    CleanUp();
	const int WIDTH = 224;
	const int HEIGHT = 256;
	const int SCALE = 2;
	const int VRAM_OFFSET = 9216;
	const int WIDTH_PIXELS = WIDTH * 32; 


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
            "Space Owner Emulator",
            SDL.SDL_WINDOWPOS_UNDEFINED,
            SDL.SDL_WINDOWPOS_UNDEFINED,
			WIDTH * SCALE,
			HEIGHT * SCALE,
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
        while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1) // Polling for video and input events
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
							intel8080Emulator.MachineKeyDown(4); // Credit
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
							intel8080Emulator.MachineKeyUp(4); // Credit
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
				for (var byteIndex = 0; byteIndex < WIDTH_PIXELS; byteIndex++) // Alghoritm to rotate the screen counter clockwise.
				{
					var loc = byteIndex + VRAM_OFFSET; // VRAM location

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
						x = destCol * SCALE,
						y = (destRow - bitIndex) * SCALE,
						w = SCALE,
						h = SCALE
					};

					if (bit == true) {
						SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
						}
					else{
						SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
						}

					SDL.SDL_RenderFillRect(renderer, ref rect);

					}
				}
				SDL.SDL_RenderPresent(renderer);
		}
	}

	// Quitting
    void CleanUp()
    {
        SDL.SDL_DestroyRenderer(renderer);
        SDL.SDL_DestroyWindow(window);
        SDL.SDL_Quit();

    }
}

void ThreadMethod3() // Play audio .wav files whenever the game indicates to do so. Future improvement: listen to events to play sound instead of an infinite loop (less CPU usage)
{

	byte lastOutPort3 = new byte();
	byte lastOutPort5 = new byte();
	#pragma warning disable CA1416 // Validate platform compatibility
	SoundPlayer player = new SoundPlayer();
	#pragma warning restore CA1416 // Validate platform compatibility

	UInt32 frameStart;
	UInt32 frameTime;

	while (true) 
	{

		frameStart = SDL.SDL_GetTicks();


		if (lastOutPort3 != intel8080Emulator.ports.OutPorts[3]) {
			if (((intel8080Emulator.ports.OutPorts[3] & 0x01) == 0x01) && ((intel8080Emulator.ports.OutPorts[3] & 0x01) != (lastOutPort3 & 0x01)))
			{
				#pragma warning disable CA1416 // Validate platform compatibility
				player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory  + "\\Audio\\ufo_lowpitch.wav";
				player.PlaySync();
				#pragma warning restore CA1416 // Validate platform compatibility
				// Play UFO sound for a while
			}

			if (((intel8080Emulator.ports.OutPorts[3] & 0x02) == 0x02) && ((intel8080Emulator.ports.OutPorts[3] & 0x02) != (lastOutPort3 & 0x02)))
			{
				#pragma warning disable CA1416 // Validate platform compatibility
				player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Audio\\shoot.wav";
				player.PlaySync();
				#pragma warning restore CA1416 // Validate platform compatibility
				// Play shot sound
			}

			if (((intel8080Emulator.ports.OutPorts[3] & 0x04) == 0x04) && ((intel8080Emulator.ports.OutPorts[3] & 0x04) != (lastOutPort3 & 0x04)))
			{
				#pragma warning disable CA1416 // Validate platform compatibility
				player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Audio\\explosion.wav";
				player.PlaySync();
				#pragma warning restore CA1416 // Validate platform compatibility
				// Player death sound
			}

			if (((intel8080Emulator.ports.OutPorts[3] & 0x08) == 0x08) && ((intel8080Emulator.ports.OutPorts[3] & 0x08) != (lastOutPort3 & 0x08)))
			{
				#pragma warning disable CA1416 // Validate platform compatibility
				player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Audio\\invaderkilled.wav";
				player.PlaySync();
				#pragma warning restore CA1416 // Validate platform compatibility
				// Player alien death sound
			}

			lastOutPort3 = intel8080Emulator.ports.OutPorts[3];
		}

		if (lastOutPort5 != intel8080Emulator.ports.OutPorts[5])
		{
			if (((intel8080Emulator.ports.OutPorts[5] & 0x01) == 0x01) && ((intel8080Emulator.ports.OutPorts[5] & 0x01) != (lastOutPort5 & 0x01)))
			{
				#pragma warning disable CA1416 // Validate platform compatibility
				player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Audio\\fastinvader1.wav";
				player.PlaySync();
				#pragma warning restore CA1416 // Validate platform compatibility
			}

			if (((intel8080Emulator.ports.OutPorts[5] & 0x02) == 0x02) && ((intel8080Emulator.ports.OutPorts[5] & 0x02) != (lastOutPort5 & 0x02)))
			{
				#pragma warning disable CA1416 // Validate platform compatibility
				player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Audio\\fastinvader2.wav";
				player.PlaySync();
				#pragma warning restore CA1416 // Validate platform compatibility
			}

			if (((intel8080Emulator.ports.OutPorts[5] & 0x04) == 0x04) && ((intel8080Emulator.ports.OutPorts[5] & 0x04) != (lastOutPort5 & 0x04)))
			{
				#pragma warning disable CA1416 // Validate platform compatibility
				player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Audio\\fastinvader3.wav";
				player.PlaySync();
				#pragma warning restore CA1416 // Validate platform compatibility
			}

			if (((intel8080Emulator.ports.OutPorts[5] & 0x08) == 0x08) && ((intel8080Emulator.ports.OutPorts[5] & 0x08) != (lastOutPort5 & 0x08)))
			{
				#pragma warning disable CA1416 // Validate platform compatibility
				player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Audio\\fastinvader4.wav";
				player.PlaySync();
				#pragma warning restore CA1416 // Validate platform compatibility
			}

			if (((intel8080Emulator.ports.OutPorts[5] & 0x10) == 0x10) && ((intel8080Emulator.ports.OutPorts[5] & 0x10) != (lastOutPort5 & 0x10)))
			{
				#pragma warning disable CA1416 // Validate platform compatibility
				player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Audio\\invaderkilled.wav";
				player.PlaySync();
				#pragma warning restore CA1416 // Validate platform compatibility
			}

			lastOutPort5 = intel8080Emulator.ports.OutPorts[5];

		}
		frameTime = SDL.SDL_GetTicks() - frameStart;
		if (frameDelay > frameTime)
		{
			Thread.Sleep((int)(frameDelay - frameTime));
		}
	}
}