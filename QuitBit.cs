// Copyright (c) 2015, Joel Longanecker
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following 
// disclaimer in the documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
// IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; 
// OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// QuitBit
// Emulator Killer written by Joel Longanecker
// 2015
//
// Usage:
//		qb.exe --controller=2 --buttons=0+1+2 --exec="c:\emulators\nes\nes.exe" --params="c:\roms\nes\mario.nes"
//
// Example emulation station usage:
// <system>
//		<name>genesis</name>
//		<fullname>Sega Genesis</fullname>
//		<path>C:\Roms\genesis</path>
//		<extension>.bin .zip</extension>
//		<command>qb.exe --controller=All --buttons=6 --exec=c:\retroarch\retroarch.exe --params=-D -L C:\retroarch\cores\genesis_plus_gx_libretro.dll "%ROM_RAW%"</command>
//		<platform>genesis</platform>
//		<theme>genesis</theme>
// </system>

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace QuitBit
{
    [StructLayout(LayoutKind.Sequential)]
    public struct JOYINFOEX
    {
        public int dwSize;
        public int dwFlags;
        public int dwXpos;
        public int dwYpos;
        public int dwZpos;
        public int dwRpos;
        public int dwUpos;
        public int dwVpos;
        public int dwButtons;
        public int dwButtonNumber;
        public int dwPOV;
        public int dwReserved1;
        public int dwReserved2;
    }

    internal sealed class Controller
    {
        [DllImport("winmm.dll")]
        internal static extern int joyGetPosEx(int uJoyID, ref JOYINFOEX pji); //Get the state of a controller with their ID
        [DllImport("winmm.dll")]
        public static extern Int32 joyGetNumDevs(); //How many controllers are plugged in

        bool specific = false; //Checks all by default
        private int controllerNum;
        private HashSet<int> btns = new HashSet<int>();
        JOYINFOEX state = new JOYINFOEX();

        public Controller(string number)
        {
            if (int.TryParse(number, out controllerNum))
                specific = true;
            state.dwFlags = 128;
            state.dwSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(JOYINFOEX));
        }

        public List<int> getButtons() //Finds the buttons pressed at the moment
        {
            btns.Clear();
            if (specific) //Checking one controller
            {
                joyGetPosEx(controllerNum, ref state);
                findPressedButtons();
            }
            else //Checking all controllers
            {
                for (int i = 0; i < joyGetNumDevs(); i++)
                {
                    joyGetPosEx(i, ref state);
                    findPressedButtons();
                }
            }

            return btns.ToList();
        }

        private void findPressedButtons()
        {
            for (int i = 0; i < 16; i++) //16 == Number of Buttons
            {
                var mask = (int)Math.Pow(2, i);

                if ((state.dwButtons & mask) == mask)
                    btns.Add(i);
            }
        }
    }

    internal sealed class Program
    {
        [STAThread]
        private static void Main() //string[] args not utilized
        {
            List<int> buttons = new List<int>();
            string controllerString = "", buttonsString = "", execString = "", paramsString = "";

            var clString = Environment.CommandLine;

            var stringElements = clString.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var s in stringElements)
            {
                if (s.Contains("="))
                {
                    var lSide = s.Split('=')[0];
                    var rSide = s.Split('=')[1];

                    if (lSide == "buttons" || lSide == "b")
                        buttonsString = rSide;
                    else if (lSide == "exec" || lSide == "e")
                        execString = rSide;
                    else if (lSide == "params" || lSide == "p")
                        paramsString = rSide;
                    else if (lSide == "contoller" || lSide == "c")
                        controllerString = rSide;
                }
            }

            if (buttonsString == string.Empty || execString == string.Empty) //No buttons or parameters? Pff.
            {
                return;
            }

            Controller controller = new Controller(controllerString); //Controller class that handles button presses when checked

            foreach (var b in buttonsString.Split('+')) //Find Button Combo that is required
            {
                int oVal = 0;
                if (int.TryParse(b, out oVal))
                {
                    buttons.Add(oVal);
                }
                else
                {
                    return;
                }
            }
            buttons.Sort();

            System.Diagnostics.Process runningEmulator = new System.Diagnostics.Process(); //Start up the emulator
            runningEmulator.StartInfo.FileName = execString;
            runningEmulator.StartInfo.Arguments = paramsString;
            runningEmulator.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(execString);
            runningEmulator.Start();

            bool matched;
            while (true) //Start Checking for the button combo
            {
                System.Threading.Thread.Sleep(75); //Removing this greatly increases CPU usage, 75 is good enough

                matched = true;
                var pressedButtons = controller.getButtons();
                pressedButtons.Sort();

                //Console.WriteLine(string.Join(" ", pressedButtons.ToArray())); //For Testing Purposes

                if (pressedButtons.Count() != buttons.Count())
                    continue;

                for (int i = 0; i < pressedButtons.Count(); i++)
                {
                    if (pressedButtons[i] != buttons[i])
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched) //Shutting things down
                {
                    try
                    {
                        runningEmulator.Kill();
                    }
                    catch { }
                    return;
                }
            }
        }
    }
}
