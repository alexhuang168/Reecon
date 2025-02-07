﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis; // For message supression

namespace Reecon
{
    class Program
    {
        static string target = "";
        static readonly List<int> portList = new List<int>();
        static readonly List<Thread> threadList = new List<Thread>();
        static readonly List<string> postScanList = new List<string>();
        static void Main(string[] args)
        {
            DateTime startDate = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Reecon - Version 0.25c ( https://github.com/Reelix/Reecon )");
            Console.ForegroundColor = ConsoleColor.White;
            if (args.Length == 0)
            {
                Console.WriteLine("Usage");
                Console.WriteLine("-----");
                Console.WriteLine("Basic Scan:\tReecon IPHere (Optional: -noping to skip the online check)");
                Console.WriteLine("Display IP:\tReecon -ip");
                Console.WriteLine("NMap-Load Scan:\tReecon outfile.nmap (Requires -oG on a regular nmap scan)");
                Console.WriteLine("Binary Pwn:\tReecon -pwn FileName (Very buggy)");
                Console.WriteLine("Searchsploit:\tReecon -searchsploit nameHere (Beta)");
                Console.WriteLine("Shell Gen:\tReecon -shell");
                Console.WriteLine("SMB Brute:\tReecon -smb-brute (Linux Only)");
                Console.WriteLine("WinRM Brute:\tReecon -winrm-brute IP UserList PassList");
                Console.WriteLine("LFI Test:\tReecon -lfi (Very buggy)");
                Console.WriteLine("Web Info:\tReecon -web url (Very buggy)");
                Console.ResetColor();
                return;
            }

            // Check if it's anything custom
            if (args.Contains("-ip") || args.Contains("--ip"))
            {
                General.GetIP();
                Console.ResetColor();
                return;
            }
            else if (args.Contains("-lfi") || args.Contains("--lfi"))
            {
                LFI.Scan(args);
                Console.ResetColor();
                return;
            }
            else if (args.Contains("-pwn") || args.Contains("--pwn"))
            {
                Pwn.Scan(args);
                Console.ResetColor();
                return;
            }
            else if (args.Contains("-searchsploit") || args.Contains("--searchsploit"))
            {
                Searchsploit.Search(args);
                Console.ResetColor();
                return;
            }
            else if (args.Contains("-shell") || args.Contains("--shell"))
            {
                Shell.GetInfo(args);
                Console.ResetColor();
                return;
            }
            else if (args.Contains("-smb-brute"))
            {
                SMB.SMBBrute(args);
                Console.ResetColor();
                return;
            }
            else if (args.Contains("-winrm-brute"))
            {
                WinRM.WinRMBrute(args);
                Console.ResetColor();
                return;
            }
            else if (args.Contains("-web") || args.Contains("--web"))
            {
                Web.GetInfo(args);
                Console.ResetColor();
                return;
            }
            else if (args.Contains("-osint") || args.Contains("--osint"))
            {
                OSINT.GetInfo(args);
                Console.ResetColor();
                return;
            }

            // Check if you should check if the target is up
            bool mustPing = true;
            if (args.Contains("-noping") || args.Contains("--noping"))
            {
                mustPing = false;
                args = args.Where(x => !x.Contains("noping")).ToArray();
            }
            // A common typo
            if (args.Contains("-nopign"))
            {
                Console.WriteLine("You typo'd noping");
                Console.ResetColor();
                return;
            }

            // Everything below here has a maximum of 2 args
            if (args.Length > 2)
            {
                Console.WriteLine("You probably typo'd something");
                Console.ResetColor();
                return;
            }

            // Target
            if (args[0].EndsWith(".nmap"))
            {
                string fileName = args[0];
                var (Target, Ports) = Nmap.ParseFile(fileName, false);
                target = Target;
                if (!Ports.Any())
                {
                    Console.WriteLine("Error: Empty file - Bug Reelix!");
                }
                else
                {
                    portList.AddRange(Ports);
                }
            }
            else
            {
                target = args[0];
            }

            if (target.StartsWith("http"))
            {
                Console.WriteLine("Cannot do a standard scan on a URL - Try a -web scan");
                Console.ResetColor();
                return;
            }

            // Custom ports
            if (args.Length == 2)
            {
                string portArg = args[1];
                try
                {
                    portList.AddRange(portArg.Split(',').ToList().Select(x => int.Parse(x)));
                }
                catch
                {
                    // Not a list of ports - Probably a name
                }
            }

            // First check if it's actually up
            if (mustPing)
            {
                Console.WriteLine("Checking if target is online...");
                bool? isHostOnline = General.IsUp(target);
                General.ClearPreviousConsoleLine();

                if (isHostOnline == null)
                {
                    Console.WriteLine("Invalid target: " + target);
                    return;
                }
                if (!isHostOnline.Value)
                {
                    Console.WriteLine("Host is not responding to pings :(");
                    Console.WriteLine("If you are sure it's up and are specifying ports, you can use -noping");
                    return;
                }
            }

            if (portList.Count == 0)
            {
                // Scan the target
                string fileName = Nmap.DefaultScan(args, mustPing);
                fileName += ".nmap";

                // Parse the ports
                var (Target, Ports) = Nmap.ParseFile(fileName, false);
                target = Target;
                portList.AddRange(Ports);
            }

            // Everything parsed - Down to the scanning!
            PortInfo.LoadPortInfo();

            // Ports have been defined (Either nmap or custom)
            if (portList.Count != 0)
            {
                Console.Write("Scanning: " + target);
                Console.Write(" (Port");
                if (portList.Count > 1)
                {
                    Console.Write("s");
                }
                Console.WriteLine(": " + string.Join(",", portList) + ")");
                ScanPorts(portList);
            }
            else
            {
                // All parsing and scans done - But still no ports
                Console.WriteLine("No open ports found to scan :<");
                return;
            }

            // Everything done - Now for some helpful info!
            Console.WriteLine("Finished - Some things you probably want to do: ");
            if (portList.Count == 0)
            {
                // Something broke, or there are only UDP Ports :|
                Console.WriteLine("- nmap -sC -sV -p- " + target + " -oN nmap.txt");
                Console.WriteLine("- nmap -sU " + target + " -oN nmap-UDP.txt");
            }
            else
            {
                postScanList.Add($"- Nmap Script+Version Scan: nmap -sC -sV -p{string.Join(",", portList)} {target} -oN nmap.txt" + Environment.NewLine);
                postScanList.Add($"- Nmap UDP Scan: sudo nmap -sU {target}" + Environment.NewLine);
                foreach (string item in postScanList)
                {
                    // They already have newlines in them
                    Console.Write(item);
                }
            }
            DateTime endDate = DateTime.Now;
            TimeSpan t = endDate - startDate;
            Console.WriteLine("Done in " + string.Format("{0:0.00}s", t.TotalSeconds) + " - Have fun :)");
            Console.ResetColor();
        }

        static void ScanPorts(List<int> portList)
        {
            // Multi-threaded scan
            foreach (int port in portList)
            {
                Thread myThread = new Thread(() => ScanPort(port));
                threadList.Add(myThread);
                myThread.Start();
            }

            // Wait for the scans to finish
            foreach (Thread theThread in threadList)
            {
                theThread.Join();
            }

            // And clear the thread list
            threadList.Clear();
        }

        static void ScanPort(int port)
        {
            string toDo = PortInfo.ScanPort(target, port);
            if (toDo != "")
            {
                postScanList.Add(toDo);
            }
        }
    }
}
