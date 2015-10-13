using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace LoginServer
{
    class ConsoleTools
    {

        public static void WriteConsole(String level, string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            switch (level)
            {
                case "ERROR":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case "INFO":
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case "DEBUG":
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case "CONNECT":
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case "WARNING":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }
            Console.Write("[" + level + "] -- ");
            Console.ForegroundColor = originalColor;
            Console.WriteLine(text);
           
        }
    }
}
