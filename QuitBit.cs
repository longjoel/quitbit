// Copyright (c) 2016, Joel Longanecker
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
//		qb.exe --controller=2 --buttons=2+0+1 --exec=c:\emulators\nes\nes.exe --params=c:\roms\nes\mario.nes
//

// Example emulation station usage:
// <system>
//		<name>genesis</name>
//		<fullname>Sega Genesis</fullname>
//		<path>C:\Roms\genesis</path>
//		<extension>.bin .zip</extension>
//		<command>qb.exe --buttons=6 --e=c:\retroarch\retroarch.exe --p=-D -L C:\retroarch\cores\genesis_plus_gx_libretro.dll "%ROM_RAW%"</command>
//		<platform>genesis</platform>
//		<theme>genesis</theme>
// </system>
//
// Note: --controller and --params are not necessary

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

    /// <summary>
    /// This class is used to un-screw with the screen resolution if a game is being naughty.
    /// </summary>
    internal sealed class WindowManager : IDisposable
    {
        struct POINTL
        {
            public Int32 x;
            public Int32 y;
        }

        [Flags()]
        enum DM : int
        {
            Orientation = 0x1,
            PaperSize = 0x2,
            PaperLength = 0x4,
            PaperWidth = 0x8,
            Scale = 0x10,
            Position = 0x20,
            NUP = 0x40,
            DisplayOrientation = 0x80,
            Copies = 0x100,
            DefaultSource = 0x200,
            PrintQuality = 0x400,
            Color = 0x800,
            Duplex = 0x1000,
            YResolution = 0x2000,
            TTOption = 0x4000,
            Collate = 0x8000,
            FormName = 0x10000,
            LogPixels = 0x20000,
            BitsPerPixel = 0x40000,
            PelsWidth = 0x80000,
            PelsHeight = 0x100000,
            DisplayFlags = 0x200000,
            DisplayFrequency = 0x400000,
            ICMMethod = 0x800000,
            ICMIntent = 0x1000000,
            MediaType = 0x2000000,
            DitherType = 0x4000000,
            PanningWidth = 0x8000000,
            PanningHeight = 0x10000000,
            DisplayFixedOutput = 0x20000000
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
        struct DEVMODE
        {
            public const int CCHDEVICENAME = 32;
            public const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            [System.Runtime.InteropServices.FieldOffset(0)]
            public string dmDeviceName;
            [System.Runtime.InteropServices.FieldOffset(32)]
            public Int16 dmSpecVersion;
            [System.Runtime.InteropServices.FieldOffset(34)]
            public Int16 dmDriverVersion;
            [System.Runtime.InteropServices.FieldOffset(36)]
            public Int16 dmSize;
            [System.Runtime.InteropServices.FieldOffset(38)]
            public Int16 dmDriverExtra;
            [System.Runtime.InteropServices.FieldOffset(40)]
            public DM dmFields;

            [System.Runtime.InteropServices.FieldOffset(44)]
            Int16 dmOrientation;
            [System.Runtime.InteropServices.FieldOffset(46)]
            Int16 dmPaperSize;
            [System.Runtime.InteropServices.FieldOffset(48)]
            Int16 dmPaperLength;
            [System.Runtime.InteropServices.FieldOffset(50)]
            Int16 dmPaperWidth;
            [System.Runtime.InteropServices.FieldOffset(52)]
            Int16 dmScale;
            [System.Runtime.InteropServices.FieldOffset(54)]
            Int16 dmCopies;
            [System.Runtime.InteropServices.FieldOffset(56)]
            Int16 dmDefaultSource;
            [System.Runtime.InteropServices.FieldOffset(58)]
            Int16 dmPrintQuality;

            [System.Runtime.InteropServices.FieldOffset(44)]
            public POINTL dmPosition;
            [System.Runtime.InteropServices.FieldOffset(52)]
            public Int32 dmDisplayOrientation;
            [System.Runtime.InteropServices.FieldOffset(56)]
            public Int32 dmDisplayFixedOutput;

            [System.Runtime.InteropServices.FieldOffset(60)]
            public short dmColor; // See note below!
            [System.Runtime.InteropServices.FieldOffset(62)]
            public short dmDuplex; // See note below!
            [System.Runtime.InteropServices.FieldOffset(64)]
            public short dmYResolution;
            [System.Runtime.InteropServices.FieldOffset(66)]
            public short dmTTOption;
            [System.Runtime.InteropServices.FieldOffset(68)]
            public short dmCollate; // See note below!
            [System.Runtime.InteropServices.FieldOffset(72)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            [System.Runtime.InteropServices.FieldOffset(102)]
            public Int16 dmLogPixels;
            [System.Runtime.InteropServices.FieldOffset(104)]
            public Int32 dmBitsPerPel;
            [System.Runtime.InteropServices.FieldOffset(108)]
            public Int32 dmPelsWidth;
            [System.Runtime.InteropServices.FieldOffset(112)]
            public Int32 dmPelsHeight;
            [System.Runtime.InteropServices.FieldOffset(116)]
            public Int32 dmDisplayFlags;
            [System.Runtime.InteropServices.FieldOffset(116)]
            public Int32 dmNup;
            [System.Runtime.InteropServices.FieldOffset(120)]
            public Int32 dmDisplayFrequency;
        }

        [DllImport("user32.dll")]
        static extern int EnumDisplaySettings(
         string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        static extern int ChangeDisplaySettings(
              ref DEVMODE devMode, int flags);

        const int ENUM_CURRENT_SETTINGS = -1;
        const int CDS_UPDATEREGISTRY = 0x01;
        const int CDS_TEST = 0x02;
        const int DISP_CHANGE_SUCCESSFUL = 0;
        const int DISP_CHANGE_RESTART = 1;
        const int DISP_CHANGE_FAILED = -1;

        private List<DEVMODE> _resolutions;

        /// <summary>
        /// Save all the settings in the constructor
        /// </summary>
        public WindowManager()
        {
            _resolutions = new List<DEVMODE>();

            DEVMODE vDevMode = new DEVMODE();
            int i = 0;
            while (EnumDisplaySettings(null, i, ref vDevMode) == 1)
            {
                _resolutions.Add(vDevMode);

            }

        }

        /// <summary>
        /// Restore it all in the dispose
        /// </summary>
        public void Dispose()
        {
            DEVMODE dMode;
            foreach (var d in _resolutions)
            {
                dMode = d;
                ChangeDisplaySettings(ref dMode, CDS_UPDATEREGISTRY);
            }
        }
    }

    internal sealed class Controller
    {
        [DllImport("winmm.dll")]
        internal static extern int joyGetPosEx(int uJoyID, ref JOYINFOEX pji); //Get the state of a controller with their ID
        [DllImport("winmm.dll")]
        public static extern Int32 joyGetNumDevs(); //How many controllers are plugged in

        private int controllerNum;
        private int combo;
        private JOYINFOEX state = new JOYINFOEX();

        public Controller(int n, int c)
        {
            controllerNum = n;
            combo = c;

            state.dwFlags = 128;
            state.dwSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(JOYINFOEX));
        }

        public bool comboPressed()
        {
            if (controllerNum > -1) //Checking one controller
            {
                joyGetPosEx(controllerNum, ref state);
                return (combo == state.dwButtons);
            }
            else //Checking all controllers
            {
                for (int i = 0; i < joyGetNumDevs(); i++)
                {
                    joyGetPosEx(i, ref state);
                    if (combo == state.dwButtons)
                        return true;
                }
                return false;
            }
        }
    }

    internal sealed class Program
    {
        [STAThread]
        private static void Main()
        {
            Controller controller;
            WindowManager windowManager = null;
            System.Diagnostics.Process runProgram;
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            int time;

            {
                string
                    controllerString = "-1",
                    buttonsString = "",
                    execString = "",
                    paramsString = "",
                    timeString = "0",

                    clString = Environment.CommandLine;

                string[] stringElements = clString.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);
                int buttonCombo = 0, controllerNum = -1;

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
                        else if (lSide == "time" || lSide == "t")
                            timeString = rSide;
                        else if (lSide == "contoller" || lSide == "c")
                            controllerString = rSide;
                    }
                    else
                    {
                        if (s == "rr")
                        {
                            windowManager = new WindowManager();

                        }
                    }
                }

                {
                    bool error = false;
                    int oVal = 0;

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    foreach (var b in buttonsString.Split('+')) //Find Button Combo that is required
                    {
                        if (int.TryParse(b, out oVal))
                            buttonCombo += (int)Math.Pow(2, oVal);
                        else
                        {
                            if (buttonsString == string.Empty)
                                Console.WriteLine("A button combination is not specififed.");
                            else
                                Console.WriteLine("The button argument is not used properly.");
                            error = true;
                            break;
                        }
                    }
                    if (!System.IO.File.Exists(execString))
                    {
                        if (execString == string.Empty)
                            Console.WriteLine("An executable is not specififed.");
                        else
                            Console.WriteLine("The executable does not exist, it's possibly an invalid path.");
                        error = true;
                    }
                    if (!int.TryParse(timeString, out time))
                    {
                        Console.WriteLine("The time argument not used properly.");
                        error = true;
                    }
                    if (!int.TryParse(controllerString, out controllerNum))
                    {
                        Console.WriteLine("The controller argument not used properly.");
                        error = true;
                    }
                    if (error)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Command      Alt   Purpose");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("--rr               Restore Resolution if your emulator is not respecting the desktop" + Environment.NewLine +
                                          "--buttons    --b   Button combination to close the program" + Environment.NewLine +
                                          "                       --b=0+8+6" + Environment.NewLine +
                                          "--exec       --e   Full path to the executable" + Environment.NewLine +
                                          "                       --e=C:\\Emulators\\nestopia.exe" + Environment.NewLine +
                                          "--controller --c   ID of specific controller to use           [Optional]" + Environment.NewLine +
                                          "                       --c=0" + Environment.NewLine +
                                          "--time       --t   Milliseconds to hold down the combination  [Optional]" + Environment.NewLine +
                                          "                       --t=2500" + Environment.NewLine +
                                          "--params     --p   Parameters when launching the program      [Optional]" + Environment.NewLine +
                                          "                       --p=C:\\roms\\NES\\Super Mario Bros..nes");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        return;
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.Gray;
                }

                controller = new Controller(controllerNum, buttonCombo); //Controller class that handles button presses when checked

                runProgram = new System.Diagnostics.Process(); //Start up the program
                runProgram.StartInfo.FileName = execString;
                runProgram.StartInfo.Arguments = paramsString;
                runProgram.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(execString);
                runProgram.Start();
            }

            while (true)
            {
                if (!controller.comboPressed())
                {
                    timer.Restart();
                }
                else if (timer.ElapsedMilliseconds >= time)
                {
                    try
                    {
                        if (windowManager != null)
                        {
                            windowManager.Dispose();
                            windowManager = null;
                        }

                    }
                    catch { }
                    try
                    {

                        runProgram.Kill();
                    }
                    catch { }
                    return;
                }

                System.Threading.Thread.Sleep(35);
            }
        }
    }
}
