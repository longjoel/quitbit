#QuitBit

* Owner - Joel Longanecker
* Essential contributors (https://github.com/MiRIr)

* 2015

##About QuitBit

QuitBit is a simple piece of software designed to act as an intermediary between a frontend and an emulator.
A long standing problem with setting up a mouse and keyboard free emulation system is stopping an emulator and returning to the frontend. QuitBit does that, and only that.

##Getting this software

* Download this file: https://raw.githubusercontent.com/longjoel/quitbit/master/QuitBit.cs
* Open up cmd.exe
* Navigate to the folder where you downloaded QuitBit.cs
* Execute the following: C:\Windows\Microsoft.Net\Framework\v4.0.30319\csc.exe /out:QuitBit.exe QuitBit.cs

##Features

* Quits an emulator (or other program).
* Optional time required for holding the combo down before killing the emulator.
* Simple integration into different frontends (ICE, Emulation Station, etc).

##Usage

* Stand alone usage
```
qb.exe --buttons=1+2+4 --exec=c:\emulators\dolphin\dolphin.exe
qb.exe --controller=2 --buttons=2+0+1 --exec=c:\emulators\nes\nes.exe --params=c:\roms\nes\mario.nes
```

* Example usage for emulation station

```
Example emulation station usage:
<systemList>
    <system>
         <name>genesis</name>
         <fullname>Sega Genesis</fullname>
         <path>C:\Roms\genesis</path>
         <extension>.bin .zip</extension>
         <command>qb.exe --buttons=6 --exec=c:\retroarch\retroarch.exe --params=-D -L C:\retroarch\cores\genesis_plus_gx_libretro.dll "%ROM_RAW%"</command>
         <platform>genesis</platform>
         <theme>genesis</theme>
    </system>
<systemList>
```
