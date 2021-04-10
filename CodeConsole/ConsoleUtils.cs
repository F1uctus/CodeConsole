using System;
using System.Drawing;
using CodeConsole.ScriptBench;

namespace CodeConsole {
    /// <summary>
    ///     Extension methods for <see cref="Console"/> class,
    ///     to simplify console interaction.
    /// </summary>
    public static class ConsoleUtils {
        static readonly Random rnd = new Random();

        /// <summary>
        ///     Prompts user to write something to console.
        ///     Returns string written by user.
        ///     Uses standard ReadLine function.
        /// </summary>
        public static string ReadSimple(string prompt) {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        /// <summary>
        ///     Prompts user to write something to console.
        ///     Returns string written by user.
        ///     Uses console code editor as backend.
        /// </summary>
        public static string Read(
            string       prompt,
            ConsoleColor fontColor = ConsoleColor.White
        ) {
            var result = "";
            WithFontColor(
                fontColor,
                () => {
                    var editor = new ScriptBench.ScriptBench(
                        new ScriptBenchSettings(prompt),
                        singleLineMode: true
                    );
                    result = editor.Run()[0];
                }
            );
            return result;
        }

        /// <summary>
        ///     Clears current line.
        /// </summary>
        public static void ClearLine(int fromX = 0, int length = -1) {
            if (length < 0) {
                length = Console.BufferWidth - fromX - 1;
            }
            Console.CursorLeft = fromX;
            Console.Write(new string(' ', length));
            Console.CursorLeft = fromX;
        }

        /// <summary>
        ///     Writes specified value with highlighting to the console.
        /// </summary>
        public static void Write(string code, IScriptBenchSyntaxHighlighter highlighter) {
            var values = highlighter.Highlight(code);
            ClearLine();
            foreach (var (color, value, _) in values) {
                Console.ForegroundColor = color;
                if (value.Contains("\n")) {
                    var valueLines = value.Split('\n');
                    for (var j = 0; j < valueLines.Length - 1; j++) {
                        Console.WriteLine(valueLines[j]);
                    }
                    Console.Write(valueLines[^1]);
                    continue;
                }
                Console.Write(value);
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        ///     See <see cref="Write(string,IScriptBenchSyntaxHighlighter)"/>.
        /// </summary>
        public static void WriteLine(
            string                        code,
            IScriptBenchSyntaxHighlighter highlighter
        ) {
            Write(code, highlighter);
            WriteLine();
        }

        #region Basic write functions

        /// <summary>
        ///     Writes messages to the standard output stream.
        /// </summary>
        public static void Write(params string[] messages) {
            foreach (string message in messages) {
                Console.Write(message);
            }
        }

        /// <summary>
        ///     Writes colored messages to the standard output stream.
        /// </summary>
        public static void Write(params (string text, ConsoleColor color)[] messages) {
            foreach (var (text, color) in messages) {
                WithFontColor(color, () => Console.Write(text));
            }
        }

        /// <summary>
        ///     Writes line terminator to the standard output stream.
        /// </summary>
        public static void WriteLine() {
            Console.WriteLine();
        }

        /// <summary>
        ///     Writes messages followed by last line terminator to the standard output stream.
        /// </summary>
        public static void WriteLine(params string[] messages) {
            foreach (var m in messages) {
                Console.Write(m);
            }
            Console.WriteLine();
        }

        /// <summary>
        ///     Writes colored messages followed by last line terminator to the standard output stream.
        /// </summary>
        public static void WriteLine(
            params (string text, ConsoleColor color)[] messages
        ) {
            Write(messages);
            Console.WriteLine();
        }

        #endregion

        #region Helpers

        public static void WithRandomFontColor(Action action) {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = GetRandomColor();
            action();
            Console.ForegroundColor = prevColor;
        }

        /// <summary>
        ///     Performs action in console, then returns
        ///     back to previous cursor position in console.
        /// </summary>
        public static void WithCurrentPosition(Action action) {
            var sX = Console.CursorLeft;
            var sY = Console.CursorTop;
            action();
            Console.SetCursorPosition(sX, sY);
        }

        /// <summary>
        ///     Moves to specified console position,
        ///     performs action, then returns back to previous
        ///     cursor position in console.
        /// </summary>
        public static void WithPosition(Point position, Action action) {
            var sX = Console.CursorLeft;
            var sY = Console.CursorTop;
            Console.SetCursorPosition(position.X, position.Y);
            action();
            Console.SetCursorPosition(sX, sY);
        }

        /// <summary>
        ///     Sets <see cref="Console.ForegroundColor" /> to &lt;<see cref="color" />&gt;,
        ///     performs action, then returns back to previously used color.
        /// </summary>
        public static void WithFontColor(ConsoleColor color, Action action) {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            action();
            Console.ForegroundColor = prevColor;
        }

        /// <summary>
        ///     Sets <see cref="Console.BackgroundColor" /> to &lt;<see cref="color" />&gt;,
        ///     performs action, then returns back to previously used color.
        /// </summary>
        public static void WithBackColor(ConsoleColor color, Action action) {
            var prevColor = Console.BackgroundColor;
            Console.BackgroundColor = color;
            action();
            Console.BackgroundColor = prevColor;
        }

        static ConsoleColor GetRandomColor() {
            var values = Enum.GetValues<ConsoleColor>();
            ConsoleColor result;
            do {
                result = values[rnd.Next(values.Length)];
            } while (result == ConsoleColor.Black);
            return result;
        }

        #endregion
    }
}
