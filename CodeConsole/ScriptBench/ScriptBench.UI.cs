using System;
using System.Collections.Generic;
using System.Drawing;
using static CodeConsole.ConsoleUtils;

namespace CodeConsole.ScriptBench {
    public partial class ScriptBench {
        /// <summary>
        ///     Editor's area top-left position in console.
        /// </summary>
        Point editBoxPoint;

        /// <summary>
        ///     Sets text in editor header.
        ///     Assigning it to null will write default header message.
        /// </summary>
        string EditorHeader {
            set => Console.Title = "ScriptBench v."
                                 + Version
                                 + ": "
                                 + (string.IsNullOrWhiteSpace(value)
                                       ? settings.DefaultHeader
                                       : value);
        }

        static readonly Dictionary<string, string> keys = new() {
            { "[Esc]x2", "exit" },
            { "[F1]", "copy" },
            { "[F2]", "paste" },
            { "[F3]x2", "clear all" }
        };

        /// <summary>
        ///     Draws box with help on current line start.
        ///     Don't invoke it during normal editing workflow.
        /// </summary>
        public static void DrawHelpBox() {
            Console.CursorLeft = 0;
            const string name = "ScriptBench";
            Console.Write(" ");
            // upper caps
            for (var i = 0; i < name.Length; i++) {
                Write(("┌───┐ ", ConsoleColor.DarkGray));
            }
            Console.WriteLine();
            Console.Write(" ");
            // letters
            foreach (var c in name) {
                Write(("│ ", ConsoleColor.DarkGray));
                WithRandomFontColor(() => {
                    Console.Write(c);
                });
                Write((" │ ", ConsoleColor.DarkGray));
            }
            Console.WriteLine();
            Console.Write(" ");
            // lower caps
            for (var i = 0; i < name.Length; i++) {
                Write(("└───┘ ", ConsoleColor.DarkGray));
            }
            Console.WriteLine();
            WriteLine(
                ("┌─────────────────────────────────────────────────────────────────┐",
                 ConsoleColor.DarkGray)
            );
            WriteLine(
                ("│", ConsoleColor.DarkGray),
                ("      ScriptBench - the best code editor for 19th century ;)     ",
                 ConsoleColor.White),
                ("│", ConsoleColor.DarkGray)
            );
            WriteLine(
                ("├─────────────────────────────────────────────────────────────────┤",
                 ConsoleColor.DarkGray)
            );
            WriteLine(
                ("│", ConsoleColor.DarkGray),
                (" Hotkey            Description                                   ",
                 ConsoleColor.White),
                ("│", ConsoleColor.DarkGray)
            );
            WriteLine(
                ("├─────────────────┬───────────────────────────────────────────────┤",
                 ConsoleColor.DarkGray)
            );
            WriteLine(
                ("│", ConsoleColor.DarkGray),
                (" [Esc] + [Esc]   ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray),
                (" Exit ([Enter] in single-line mode.)           ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray)
            );
            WriteLine(
                ("│", ConsoleColor.DarkGray),
                (" [F1]            ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray),
                (" Copy all text                                 ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray)
            );
            WriteLine(
                ("│", ConsoleColor.DarkGray),
                (" [F2]            ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray),
                (" Paste (Regular Ctrl-V works, but it's         ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray)
            );
            WriteLine(
                ("│", ConsoleColor.DarkGray),
                ("                 ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray),
                ("     much slower because of implementation)    ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray)
            );
            WriteLine(
                ("│", ConsoleColor.DarkGray),
                (" [F3] + [F3]     ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray),
                (" Clear all code                                ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray)
            );
            WriteLine(
                ("│", ConsoleColor.DarkGray),
                (" [Tab]           ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray),
                (" Insert 4 spaces to the line start.            ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray)
            );
            WriteLine(
                ("│", ConsoleColor.DarkGray),
                (" [Shift] + [Tab] ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray),
                (" Remove 4 spaces from the line start.          ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray)
            );
            WriteLine(
                ("└─────────────────┴───────────────────────────────────────────────┘",
                 ConsoleColor.DarkGray)
            );
        }

        int lastWidthToEnsure;
        int lastHeightToEnsure;

        /// <summary>
        ///     Ensures that console has minimal defined width & height
        ///     so editor could work without errors.
        /// </summary>
        public void EnsureWindowSize(int minW = 50, int minH = 30) {
            if (Console.WindowWidth < minW) {
                lastWidthToEnsure   = Math.Max(lastWidthToEnsure, minW);
                Console.WindowWidth = lastWidthToEnsure;
            }
            if (Console.BufferHeight < minH) {
                lastHeightToEnsure = Math.Max(lines.Count + 10,
                    Math.Max(lastHeightToEnsure, minW));
                Console.BufferHeight = lastHeightToEnsure;
            }
        }

        /// <summary>
        ///     Draws editor's top frame on current line start.
        ///     This should be called only when editor is created.
        ///     This function does nothing in single-line mode.
        /// </summary>
        void DrawTopFrame() {
            if (singleLineMode) {
                return;
            }

            Console.CursorLeft = 0;
            WriteLine(
                (settings.DrawingChars.DownRight
               + new string(
                     settings.DrawingChars.Horizontal,
                     Console.BufferWidth - 2
                 ),
                 settings.MainColor)
            );
            Write((settings.DrawingChars.Vertical + " ", settings.MainColor));
            foreach (var (key, value) in keys) {
                Write(
                    (key, settings.AccentColor),
                    (" ", ConsoleColor.White),
                    (value, ConsoleColor.Gray),
                    ("   ", ConsoleColor.White)
                );
            }
            WriteLine();
            // draw upper bound
            var prefix = settings.DrawingChars.UpRight
                       + new string(settings.DrawingChars.Horizontal, 5)
                       + settings.DrawingChars.HorizontalDown;
            WriteLine(
                (prefix
               + new string(
                     settings.DrawingChars.Horizontal,
                     Console.BufferWidth - prefix.Length - 1
                 ),
                 settings.MainColor)
            );
        }

        /// <summary>
        ///     Draws editor's lower bound.
        ///     Should be called only when user finished his work.
        ///     This function does nothing in single-line mode.
        /// </summary>
        void DrawBottomFrame() {
            if (singleLineMode) {
                return;
            }

            // move to editor lower bound
            cursorY            = lines.Count;
            Console.CursorLeft = 0;
            // render lower bound
            var prefix = new string(settings.DrawingChars.Horizontal, 6)
                       + settings.DrawingChars.HorizontalUp;
            WriteLine(
                (prefix
               + new string(
                     settings.DrawingChars.Horizontal,
                     Console.BufferWidth - prefix.Length - 1
                 ),
                 settings.MainColor)
            );
        }

        /// <summary>
        ///     Prints current line number on the left side of editor.
        /// </summary>
        void DrawCurrentLineNumber() {
            DrawLineNumber(cursorY + 1);
        }

        /// <summary>
        ///     Prints specified line number on the left side of editor.
        ///     <c>Format: ` XXXX | `</c>
        /// </summary>
        void DrawLineNumber(int lineNumber) {
            var strNum = lineNumber.ToString();
            var width = Math.Max(strNum.Length, 4);
            var view = strNum.PadLeft(width + 1).PadRight(width + 2)
                     + settings.DrawingChars.Vertical
                     + " ";
            Write((view, settings.MainColor));
        }
    }
}
