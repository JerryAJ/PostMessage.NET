﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace PostMessage.NET {
partial class Program {
    struct Key
    {
        public uint nKCode;
        public int nPress;
        public int nDelay;
    }
    public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
    private static extern IntPtr GetForegroundWindow();

    static void Main(string[] args) {
        // args = new string[] { "121680", "54,200,200", "--active" };
        // args = new string[] { "31212", "54,2000,2000" };
        // args = new string[] { "--help" };
        bool bEcho = false;
        bool bActive = false;
        int nProcessId = 0;
        string[] list = null;
        // Check arguments
        int argc = args.Length;
        for (int i = 0; i < argc; i++)
        {
            switch (args[i])
            {
                case "--help":
                    Console.WriteLine("Usage:");
                    Console.WriteLine("    PostMessage.exe <PID> <keylist>");
                    Console.WriteLine("      <PID>     Target Process ID");
                    Console.WriteLine("      <keylist> The key list which will send to the target process.");
                    Console.WriteLine("");
                    Console.WriteLine("Ex:");
                    Console.WriteLine("    PostMessage.exe 13123 32,50,200;49,200,200");
                    Console.WriteLine("      This command will start a loop to send keylist to process 13123.");
                    Console.WriteLine("      The keylist loop is PRESS SPACE_KEY(32) for 50ms and then wait for");
                    Console.WriteLine("      200ms, and PRESS KEY_1(49) for 200ms and then wait for 200ms.");
                    Console.WriteLine("");
                    Console.WriteLine("Source code:");
                    Console.WriteLine("    https://github.com/tinymins/PostMessage.NET");
                    return;

                case "--echo":
                    bEcho = true;
                    break;

                case "--active":
                    bActive = true;
                    break;

                default:
                    nProcessId = int.Parse(args[i]);
                    list = args[i + 1].Split(';');
                    i = i + 1;
                    break;
            }
        }
        if (list == null || nProcessId == 0)
        {
            Console.WriteLine("Wrong numbers of arguments! 2 expected, got " + args.Length + "!");
            Console.WriteLine("Command --help to view more information.");
            return;
        }
        // Parse keylist
        Key[] keylist = new Key[list.Length];
        for (int i = 0; i < list.Length; i++) {
            if (list[i] == "") continue;
            string[] line = list[i].Split(',');
            if (line.Length != 3) {
                Console.WriteLine("Error when parse keylist around \"" + list[i] + "\"!");
                Console.WriteLine("Command --help to view more information.");
                return;
            }
            keylist[i].nKCode = uint.Parse(line[0]);
            keylist[i].nPress = int.Parse(line[1]);
            keylist[i].nDelay = int.Parse(line[2]);
        }
        IntPtr hForeWndLast = IntPtr.Zero;
        // Try to post message
        try {
            Process p = Process.GetProcessById(nProcessId);
            IntPtr h = p.MainWindowHandle;
            while (true) {
                foreach (Key key in keylist) {
                    if (bActive)
                    {
                        IntPtr hForeWnd = GetForegroundWindow();
                        if (hForeWnd != hForeWndLast)
                        {
                            hForeWndLast = hForeWnd;
                            PostMessage(h, WM_ACTIVATEAPP, 1, 0);
                        }
                    }
                    PostMessage(h, WM_KEYDOWN, key.nKCode, 0);
                    if (bEcho)
                        Console.WriteLine("Process \"" + p.ProcessName + "\" post message KEYDOWN with key code " + key.nKCode + ".");
                    System.Threading.Thread.Sleep(key.nPress);
                    if (bEcho)
                        Console.WriteLine("Process \"" + p.ProcessName + "\" KEYPRESSING for " + key.nPress + "ms.");
                    PostMessage(h, WM_KEYUP, key.nKCode, 0);
                    if (bEcho)
                        Console.WriteLine("Process \"" + p.ProcessName + "\" post message KEYUP with key code " + key.nKCode + ".");
                    System.Threading.Thread.Sleep(key.nDelay);
                    if (bEcho)
                        Console.WriteLine("Process \"" + p.ProcessName + "\" DELAYING for " + key.nDelay + "ms.");
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return;
        }
    }
}
}
