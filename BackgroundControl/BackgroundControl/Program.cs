﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BackgroundControl
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

    internal sealed class BackgroundControl
    {
        [STAThread]
        private static void Main()
        {
            while (true)
            {
                ControlCombos control = new ControlCombos();
                System.Threading.Thread.Sleep(100);
            }
        }
    }

    internal sealed class ControlCombos
    {
        [DllImport("user32", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32")]
        static extern IntPtr GetConsoleWindow();

        const UInt32 WM_APPCOMMAND = 0x0319;
        const UInt32 APPCOMMAND_VOLUME_DOWN = 9;
        const UInt32 APPCOMMAND_VOLUME_UP = 10;

        public ControlCombos()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Controller controller;
            Stopwatch timer = new Stopwatch();
            int[] combos = { 0, 0, 0, 0 };
            string[] comboStrings = { "0", "1", "2", "3" };
            int combo = 0;


            foreach (string item in comboStrings)
            {
                foreach (string button in item.Split('+')) //Find Button Combo that is required
                {
                    if (int.TryParse(button, out int oVal))
                    {
                        combos[combo] += (int)Math.Pow(2, oVal);
                    }
                }
                combo += 1;
            }
            
            Console.WriteLine($"volumeUpCombo: {combos[0]}  | brightnessUpCombo: {combos[1]}" +
                              $"\nvolumeDownCombo: {combos[2]} | brightnessDownCombo: {combos[3]}");
            controller = new Controller(combos);

            while (true)
            {
                if (controller.ComboPressed() == -1)
                {
                    timer.Restart();
                }
                else if (controller.ComboPressed() == 1)
                {
                    SendMessage(GetConsoleWindow(), WM_APPCOMMAND, GetConsoleWindow(), new IntPtr(APPCOMMAND_VOLUME_UP << 16));
                    Console.WriteLine("volume up");
                    return;
                }
                else if (controller.ComboPressed() == 2)
                {
                    SendMessage(GetConsoleWindow(), WM_APPCOMMAND, GetConsoleWindow(), new IntPtr(APPCOMMAND_VOLUME_DOWN << 16));
                    Console.WriteLine("volume down");
                    return;
                }
                else if (controller.ComboPressed() == 4)
                {
                    Console.WriteLine("brightness up");
                    return;
                }
                else if (controller.ComboPressed() == 8)
                {
                    Console.WriteLine("brightness down");
                    return;
                }
                else
                {
                    Console.WriteLine(controller.ComboPressed());
                }
            }
        }
    }

    internal sealed class Controller
    {
        [DllImport("winmm.dll")]
        internal static extern int joyGetPosEx(int uJoyID, ref JOYINFOEX pji); //Get the state of a controller with their ID
        [DllImport("winmm.dll")]
        public static extern Int32 joyGetNumDevs(); //How many controllers are plugged in

        int[] combos;
        private JOYINFOEX state = new JOYINFOEX();

        public Controller(int[] c)
        {
            combos = c;
            state.dwFlags = 128;
            state.dwSize = Marshal.SizeOf(typeof(JOYINFOEX));
        }
        
        public int ComboPressed()
        {
            try
            {
                for (int controller = 0; controller < joyGetNumDevs(); controller++)
                {
                    joyGetPosEx(controller, ref state);
                    for (int combo = 0; combo < 4; combo++)
                    {
                        if (combos[combo] == state.dwButtons)
                        {
                            return combos[combo];
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to run command. Data was corrupt");
            }

            return -1;
        }
    }
}
