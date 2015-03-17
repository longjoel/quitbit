#QuitBit
Emulator Killer written by Joel Longanecker
2015

Usage:
qb.exe --buttons=0+1+2 --exec="c:\emulators\nes\nes.exe" --params="c:\roms\nes\mario.nes"

Example emulation station usage:
<system>
    <name>genesis</name>
    <fullname>Sega Genesis</fullname>
    <path>C:\Roms\genesis</path>
    <extension>.bin .zip</extension>
    <command>qb.exe --buttons=6 --exec=c:\retroarch\retroarch.exe --params=-D -L C:\retroarch\cores\genesis_plus_gx_libretro.dll "%ROM_RAW%"</command>
    <platform>genesis</platform>
    <theme>genesis</theme>
</system>
