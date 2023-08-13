# SpaceInvaders8080Emulator

Hello!

This is a Space Invaders emulator made in C#, called Space Owner. The aim of this emulator is to run the ROM for the original 1978 game made for the Intel 8080 arcade machine. 

To run it, just browse the "Space Owner Executable" folder and double-click on Space Owner Emulator.exe.

Note: you need to have .NET 6.0 or higher installed (https://dotnet.microsoft.com/en-us/download).

The controls are keyboard only. Below are the key bindings:


1 – Insert Credit

Enter (return) – Start

Key Arrows – Move Ship

D - Fire


For the CPU emulation, I followed this great guide provided by Emulator 101: http://emulator101.com/.

For the video rendering, I have decided to use the SDL2 bindings for C#: https://jsayers.dev/c-sharp-sdl-tutorial-part-1-setup/.

The audio is not emulated. The emulator plays .wav files as the game demands. The .wav files were obtained here: http://www.classicgaming.cc/classics/space-invaders/sounds.

The Computer Archeology page for Space Invaders (arcade) was also immensely helpful for understanding the Space Invaders hardware and checking the correct code execution of the game.

Other publicly available Space Invaders emulators on the Internet also helped me to compare my code against and see what I was missing to make it work.

And, of course, credits to Taito and Tomohiro Nishikado, for creating the original game in 1978.
