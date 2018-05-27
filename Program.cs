using System;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SqliteToCsv;
using System.IO;

namespace SqliteToCsv
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            args = new string[2]
            {
                @"C:\Temp\forums.db",
                @"C:\Temp\output"
            };
#endif
            string source = String.Empty;
            string destination = String.Empty;
            bool validArgs = false;

            if(args.Length != 2 && args.Length != 0)
            {
                Console.WriteLine("Invalid Arguments");
                return;
            }
            else if(args.Length == 2)
            {
                validArgs = ValidateArgs(args[0], args[1]);
                if (validArgs)
                {
                    source = args[0];
                    destination = args[1];
                }
            }
            else if(args.Length == 0)
            {
                source = GetSourceFromUser();
                destination = GetDestinationFromUser();
                validArgs = true;
            }

            if (validArgs)
            {
                using (ParallelProcessor processor = new ParallelProcessor(source, destination))
                using (ConsoleStats stats = new ConsoleStats(processor))
                {
                    Task.Run(async () =>
                    {
                        await processor.Start();
                    }).GetAwaiter().GetResult();
                    Console.ReadLine();
                }
            }
#if DEBUG
            Console.ReadLine();
#endif
        }

        private static bool ValidateArgs(string source, string destination)
        {
            if (!File.Exists(source))
            {
                WriteLineColor("Source File Not Found", ConsoleColor.Red);
                return false;
            }

            if (!Directory.Exists(destination))
            {
                WriteLineColor("Destination Directory Not Found", ConsoleColor.Red);
                return false;
            }
            return true;
        }

        private static string GetSourceFromUser()
        {
            int cachedtop = Console.CursorTop;
            Console.Write("Please enter the DB file path: ");
            int i = 0;
            while (true)
            {             
                string source = Console.ReadLine();
                if (File.Exists(source))
                {
                    return source;
                }
                ClearCurrentLine(cachedtop);
                WriteColor($"File not found, enter a valid path: ", ConsoleColor.DarkRed);
                i++;
            }
        }

        private static string GetDestinationFromUser()
        {
            int cachedtop = Console.CursorTop;
            Console.Write("Please enter the destiantion directory: ");
            int i = 0;
            while (true)
            {
                string destination = Console.ReadLine();
                if (Directory.Exists(destination))
                {
                    return destination;
                }
                else
                {
                    Console.Write("Directory not found. Do you wish to create this directory? [yes/no]");
                    while (true)
                    {
                        string answer = Console.ReadLine();
                        if (answer.ToLower() == "yes")
                        {
                            Directory.CreateDirectory(destination);
                            return destination;
                        }
                        else if(answer.ToLower() == "no")
                        {
                            Console.Write("Please enter a valid directory path");
                            break;
                        }
                    }
                }
                ClearCurrentAndBelowLine(cachedtop);
                WriteColor($"Please enter a valid directory path ", ConsoleColor.DarkRed);
                i++;
            }
        }

        private static void WriteColor(string value, ConsoleColor color)
        {
            ConsoleColor current = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ForegroundColor = current;
        }

        private static void WriteLineColor(string value, ConsoleColor color)
        {
            ConsoleColor current = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = current;
        }

        private static void ClearCurrentLine(int? top = null)
        {
            Console.SetCursorPosition(0, top.HasValue ? top.Value : Console.CursorTop);
            Console.Write(new String(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, top.HasValue ? top.Value : Console.CursorTop);
        }

        private static void ClearCurrentAndBelowLine(int? top = null)
        {
            int startingPosition = top.HasValue ? top.Value : Console.CursorTop;
            int num = Console.WindowHeight - Console.CursorTop;
            for(int i = 0; i < num; i++)
            {
                Console.SetCursorPosition(0, startingPosition + i);
                Console.Write(new String(' ', Console.WindowWidth));
            }
            Console.SetCursorPosition(0, top.HasValue ? top.Value : Console.CursorTop);
        }
    }
}
