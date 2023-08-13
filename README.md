# SpaceInvaders8080Emulator

## Summary

Hello!

This is a Space Invaders emulator made in C#, called Space Owner. The aim of this emulator is to run the ROM for the original 1978 game made for the Intel 8080 arcade machine and learn more about emulation.

## Minimal Path to Awesome

To run it, just browse the "Space Owner Executable" folder and double-click on Space Owner Emulator.exe. The ROM is already included.

![Tab Personal SSO QuickstartGif](GitAssets/SpaceOwner.gif)  

## Languages and libraries

- C#
- SDL2

## Prerequisites

* [.NET](https://dotnet.microsoft.com/en-us/download) version 6.0 or higher.

  determine dotnet version
  ```bash
  dotnet --version
  ```

## Version history

Version|Date|Author|Comments
-------|----|----|--------
1.0|Augustu 13th, 2023|Felipe Dotti|Initial release

## Controls

The controls are keyboard only. Below are the key bindings:


1 – Insert Credit

Enter (return) – Start

Key Arrows – Move Ship

D - Fire

## Credits


For the CPU emulation, I followed this excellent guide by Emulator 101: http://emulator101.com/.

For the video rendering, I have decided to use the SDL2 bindings for C#: https://jsayers.dev/c-sharp-sdl-tutorial-part-1-setup/.

The audio is not emulated. The emulator plays .wav files as the game demands. The .wav files were obtained here: http://www.classicgaming.cc/classics/space-invaders/sounds.

The Computer Archeology page for Space Invaders (Arcade) was also immensely helpful for understanding the Space Invaders hardware and checking the correct code execution of the game: http://computerarcheology.com/Arcade/SpaceInvaders/

Other publicly available Space Invaders emulators on the Internet also helped me to compare my code against it and see what I was missing to make it work.

And, of course, credit to Taito and Tomohiro Nishikado, for creating the original game in 1978.
