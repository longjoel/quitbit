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
//		qb.exe --buttons=0+1+2 --exec="c:\emulators\nes\nes.exe" --params="c:\roms\nes\mario.nes"
//
// Example emulation station usage:
// <system>
//		<name>genesis</name>
//		<fullname>Sega Genesis</fullname>
//		<path>C:\Roms\genesis</path>
//		<extension>.bin .zip</extension>
//		<command>qb.exe --buttons=6 --exec=c:\retroarch\retroarch.exe --params=-D -L C:\retroarch\cores\genesis_plus_gx_libretro.dll "%ROM_RAW%"</command>
//		<platform>genesis</platform>
//		<theme>genesis</theme>
// </system>

using System;
using System.Windows.Forms;

using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Linq.Expressions;



namespace QuitBit
{
	 public enum DPadEnum : ushort
    {
        None = ushort.MaxValue,
        Up = 0,
        Down = 18000,
        Left = 27000,
        Right = 9000,
        UpLeft = 31500,
        UpRight = 4500,
        DownLeft = 22500,
        DownRight = 13500

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Joycaps
    {
        /// <summary>
        ///     Manufacturer identifier. Manufacturer identifiers are defined in Manufacturer and Product Identifiers.
        /// </summary>
        //[CLSCompliant(false)]
        public ushort WMid;
        /// <summary>
        ///     Product identifier. Product identifiers are defined in Manufacturer and Product Identifiers.
        /// </summary>
        //[CLSCompliant(false)]
        public ushort WPid;
        /// <summary>
        ///     Null-terminated string containing the joystick product name.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public String SzPname;
        /// <summary>
        ///     Minimum X-coordinate.
        /// </summary>
        public Int32 WXmin;
        /// <summary>
        ///     Maximum X-coordinate.
        /// </summary>
        public Int32 WXmax;
        /// <summary>
        /// Minimum Y-coordinate.
        /// </summary>
        public Int32 WYmin;
        /// <summary>
        ///     Maximum Y-coordinate.
        /// </summary>
        public Int32 WYmax;
        /// <summary>
        ///     Minimum Z-coordinate.
        /// </summary>
        public Int32 WZmin;
        /// <summary>
        ///     Maximum Z-coordinate.
        /// </summary>
        public Int32 WZmax;
        /// <summary>
        ///     Number of joystick buttons.
        /// </summary>
        public Int32 WNumButtons;
        /// <summary>
        ///     Smallest polling frequency supported when captured by the  function.
        /// </summary>
        public Int32 WPeriodMin;

        public Int32 WPeriodMax;
        /// <summary>
        ///     Minimum rudder value. The rudder is a fourth axis of movement.
        /// </summary>
        public Int32 WRmin;
        /// <summary>
        ///     Maximum rudder value. The rudder is a fourth axis of movement.
        /// </summary>
        public Int32 WRmax;
        /// <summary>
        ///     Minimum u-coordinate (fifth axis) values.
        /// </summary>
        public Int32 WUmin;
        /// <summary>
        ///     Maximum u-coordinate (fifth axis) values.
        /// </summary>
        public Int32 WUmax;
        /// <summary>
        ///     Minimum v-coordinate (sixth axis) values.
        /// </summary>
        public Int32 WVmin;
        /// <summary>
        ///     Maximum v-coordinate (sixth axis) values.
        /// </summary>
        public Int32 WVmax;
        /// <summary>
        ///     Joystick capabilities The following flags define individual capabilities that a joystick might have:
        /// </summary>

        public Int32 WCaps;
        /// <summary>
        ///     Maximum number of axes supported by the joystick.
        /// </summary>
        public Int32 WMaxAxes;
        /// <summary>
        ///     Number of axes currently in use by the joystick.
        /// </summary>
        public Int32 WNumAxes;
        /// <summary>
        ///     Maximum number of buttons supported by the joystick.
        /// </summary>
        public Int32 WMaxButtons;
        /// <summary>
        ///     Null-terminated string containing the registry key for the joystick.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public String SzRegKey;
        /// <summary>
        ///     Null-terminated string identifying the joystick driver OEM.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public String SzOemvxD;
    }

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

    enum JoyError : int
    {
        JOYERR_BASE = 160,
        JOYERR_PARMS = (JOYERR_BASE + 5),
        JOYERR_UNPLUGGED = (JOYERR_BASE + 7),
        MMSYSERR_BASE = 0,
        MMSYSERR_BADDEVICEID = (MMSYSERR_BASE + 2),
        MMSYSERR_INVALPARAM = (MMSYSERR_BASE + 11)
    }
	
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		
		[DllImport("winmm.dll")]
        internal static extern int joyGetPosEx(int uJoyID, ref JOYINFOEX pji);

        [DllImport("winmm.dll")]
        private static extern Int32 joyGetDevCaps(int uJoyID, ref Joycaps pjc, Int32 cbjc);
        
        [DllImport("winmm.dll")]
        private static extern short joyGetNumDevs();
		
        private static List<int> GetButtons()
        {
        	var btns = new List<int>();
        	
        	JOYINFOEX state = new JOYINFOEX();
                state.dwFlags = 128; // buttons!
                state.dwSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(JOYINFOEX));
                joyGetPosEx(0, ref state);

                var bList = new bool[16];

                for (int i = 0; i < 16; i++)
                {
                    var mask = (int)Math.Pow(2, i);

                    if ((state.dwButtons & mask) == mask)
                    	btns.Add(i);
                }

                return btns;
        }
		
		
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			
			string buttonsString = "";
			List<int> buttons = new List<int>();
			
			string execString = "";
			string paramsString = "";
			
			Process runningEmulator = null;
			
			var clString = Environment.CommandLine;
			var stringElements = clString.Split(new string[]{"--"}, StringSplitOptions.RemoveEmptyEntries);
			
			foreach(var s in stringElements)
			{
				if(s.Contains("="))
				{
					var lSide = s.Split('=')[0];
					var rSide = s.Split('=')[1];
					
					if(lSide == "buttons") buttonsString = rSide;
					if(lSide == "exec") execString = rSide;
					if(lSide == "params") paramsString = rSide;
					
				}
			}
			
			// should probably return something more usefull.
			if(buttonsString == string.Empty || execString == string.Empty || paramsString == string.Empty)
			{
				return;
			}
			
			foreach (var b in buttonsString.Split('+')) {
				int oVal = 0;
				if(int.TryParse(b, out oVal))
				{
					buttons.Add(oVal);
				}
				else{
					return;
				}
			}
			
			// create new instance of emulator
			runningEmulator = new Process()
			{
				StartInfo = new ProcessStartInfo(execString, paramsString),				
			};
			
			runningEmulator.StartInfo.WorkingDirectory = Path.GetDirectoryName(execString);
			runningEmulator.StartInfo.UseShellExecute = true;
			
			//runningEmulator.StartInfo.UseShellExecute = true;
			runningEmulator.Start();
			
			// wait for emulator to be active
			while(runningEmulator.Handle == IntPtr.Zero)
			{
				System.Threading.Thread.Sleep(100);
				runningEmulator.Refresh();
			}
			
			
			var thr = new System.Threading.Thread(()=>{
				var isRunning = true;
			    while(isRunning)
				{
				//System.Threading.Thread.Sleep(0);
				//runningEmulator.Refresh();
				
				
				var pressedButtons = GetButtons().ToList();
				pressedButtons.Sort();
				
				var expectedButtons = buttons;
				expectedButtons.Sort();
				
				if(pressedButtons.Count() != expectedButtons.Count())
				{
					continue;
				}
				
				var matched = true;
				for(int i = 0; i < pressedButtons.Count();i++)
				{
					if(pressedButtons[i] != expectedButtons[i])
					{
						matched = false;
						break;
					}
					
				}

				if(matched)
					isRunning = false;				
			}});
			
			thr.Start();
			thr.Join();
			
			
			//runningEmulator.Refresh();
			
				try{
					runningEmulator.Kill();
					
				}
				catch{}
			
			
			return;
			
		}
		
	}
}
