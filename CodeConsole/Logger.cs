using System;

namespace CodeConsole {
    public static class Logger {
        public static void Log(string message) {
            ConsoleUI.WriteLine(message);
        }

        public static void Info(string message) {
            ConsoleUI.WriteLine((message, ConsoleColor.DarkCyan));
        }

        public static void Task(string message) {
            ConsoleUI.WriteLine(("=== " + message, ConsoleColor.DarkCyan));
        }

        public static void Step(string message) {
            ConsoleUI.WriteLine(("--- " + message, ConsoleColor.Cyan));
        }

        public static void Warn(string message) {
            ConsoleUI.WriteLine(($"Warning: {message}", ConsoleColor.DarkYellow));
        }

        public static void Error(string message) {
            ConsoleUI.WriteLine(($"Error: {message}", ConsoleColor.Red));
        }
    }
}