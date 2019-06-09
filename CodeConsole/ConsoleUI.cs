using System;
using System.Drawing;

namespace CodeConsole {
    /// <summary>
    ///     Extension methods for [console] class,
    ///     to simplify console interaction.
    /// </summary>
    public static class ConsoleUI {
        public static string Read(string prompt, ConsoleColor fontColor = ConsoleColor.White) {
            var result = "";
            WithFontColor(
                fontColor,
                () => {
                    var editor = new ConsoleCodeEditor(true, prompt: prompt);
                    result = editor.RunSession()[0];
                }
            );
            return result;
        }

        /// <summary>
        ///     Clears current line.
        /// </summary>
        public static void ClearLine(int fromX = 0) {
            Console.CursorLeft = fromX;
            Console.Write(new string(' ', Console.BufferWidth - fromX - 1));
            Console.CursorLeft = fromX;
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
            for (var i = 0;
                i < messages.Length;
                i++) // ReSharper disable once AccessToModifiedClosure
            {
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
        ///     Moves to specified console position,
        ///     performs action, then returns back to previous
        ///     cursor position in console.
        /// </summary>
        public static void WithPosition(int x, int y, Action action) {
            // save position
            int sX = Console.CursorLeft;
            int sY = Console.CursorTop;
            // move cursor
            Console.SetCursorPosition(x, y);
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
        ///     Sets <see cref="Console.ForegroundColor" /> to &lt;<see cref="color" />&gt;,
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

        #endregion
    }
}