using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Shared
{
    public static class Log
    {
        private static readonly object _syncLock = new();

        public static void Info(string message)
        {
            lock (_syncLock)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Print(message);
            }
        }

        public static void Success(string message)
        {
            lock (_syncLock)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Print(message);
            }
        }

        public static void Error(string message)
        {
            lock (_syncLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Print(message);
            }
        }

        private static void Print(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss.fff");
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"[{time}] [Thread {threadId}] {message}");
            Console.ResetColor();
        }
    }
}
