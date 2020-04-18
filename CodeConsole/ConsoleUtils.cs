using System;
using System.Collections.Generic;
using System.Drawing;
using CodeConsole.ScriptBench;

namespace CodeConsole {
    /// <summary>
    ///     Extension methods for <see cref="Console"/> class,
    ///     to simplify console interaction.
    /// </summary>
    public static class ConsoleUtils {
        private static readonly Random rnd = new Random();

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
        public static string Read(string prompt, ConsoleColor fontColor = ConsoleColor.White) {
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
            List<ColoredValue> values = highlighter.Highlight(code);
            ClearLine();
            foreach (ColoredValue value in values) {
                Console.ForegroundColor = value.Color;
                if (value.Value.Contains("\n")) {
                    string[] valueLines = value.Value.Split('\n');
                    for (var j = 0; j < valueLines.Length - 1; j++) {
                        Console.WriteLine(valueLines[j]);
                    }

                    Console.Write(valueLines[^1]);
                    continue;
                }

                Console.Write(value.Value);
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        ///     See <see cref="Write(string,IScriptBenchSyntaxHighlighter)"/>.
        /// </summary>
        public static void WriteLine(string code, IScriptBenchSyntaxHighlighter highlighter) {
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
            for (var i = 0; i < messages.Length; i++) {
                // ReSharper disable once AccessToModifiedClosure
                WithFontColor(messages[i].color, () => Console.Write(messages[i].text));
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
            for (var i = 0; i < messages.Length; i++) {
                if (i == messages.Length - 1) {
                    Console.WriteLine(messages[i]);
                    return;
                }

                Console.Write(messages[i]);
            }
        }

        /// <summary>
        ///     Writes colored messages followed by last line terminator to the standard output stream.
        /// </summary>
        public static void WriteLine(params (string text, ConsoleColor color)[] messages) {
            for (var i = 0; i < messages.Length; i++) {
                if (i == messages.Length - 1) {
                    WithFontColor(messages[i].color, () => Console.WriteLine(messages[i].text));
                    return;
                }

                // ReSharper disable once AccessToModifiedClosure
                WithFontColor(messages[i].color, () => Console.Write(messages[i].text));
            }
        }

        #endregion

        #region Helpers

        public static void WithRandomFontColor(Action action) {
            // save color
            ConsoleColor prevColor = Console.ForegroundColor;
            // set new color
            Console.ForegroundColor = GetRandomColor();
            // do action
            action();
            // reset color
            Console.ForegroundColor = prevColor;
        }

        /// <summary>
        ///     Performs action in console, then returns
        ///     back to previous cursor position in console.
        /// </summary>
        public static void WithCurrentPosition(Action action) {
            // save position
            int sX = Console.CursorLeft;
            int sY = Console.CursorTop;
            // do action
            action();
            // reset cursor
            Console.SetCursorPosition(sX, sY);
        }

        /// <summary>
        ///     Moves to specified console position,
        ///     performs action, then returns back to previous
        ///     cursor position in console.
        /// </summary>
        public static void WithPosition(Point position, Action action) {
            // save position
            int sX = Console.CursorLeft;
            int sY = Console.CursorTop;
            // move cursor
            Console.SetCursorPosition(position.X, position.Y);
            // do action
            action();
            // reset cursor
            Console.SetCursorPosition(sX, sY);
        }

        /// <summary>
        ///     Sets <see cref="Console.ForegroundColor" /> to &lt;<see cref="color" />&gt;,
        ///     performs action, then returns back to previously used color.
        /// </summary>
        public static void WithFontColor(ConsoleColor color, Action action) {
            // save color
            ConsoleColor prevColor = Console.ForegroundColor;
            // set new color
            Console.ForegroundColor = color;
            // do action
            action();
            // reset color
            Console.ForegroundColor = prevColor;
        }

        /// <summary>
        ///     Sets <see cref="Console.BackgroundColor" /> to &lt;<see cref="color" />&gt;,
        ///     performs action, then returns back to previously used color.
        /// </summary>
        public static void WithBackColor(ConsoleColor color, Action action) {
            // save color
            ConsoleColor prevColor = Console.BackgroundColor;
            // set new color
            Console.BackgroundColor = color;
            // do action
            action();
            // reset color
            Console.BackgroundColor = prevColor;
        }

        private static ConsoleColor GetRandomColor() {
            string[]     colorNames = Enum.GetNames(typeof(ConsoleColor));
            ConsoleColor result;
            do {
                result = (ConsoleColor) Enum.Parse(
                    typeof(ConsoleColor),
                    colorNames[rnd.Next(colorNames.Length)]
                );
            } while (result == ConsoleColor.Black);
            return result;
        }

        #endregion
    }
}
