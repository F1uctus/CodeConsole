using System;
using System.Drawing;
using static CodeConsole.ConsoleUtils;

namespace CodeConsole.ScriptBench {
    public partial class ScriptBench {
        // ┌ ┐ └ ┘ ├ ┤ ┬ ┴ ┼ ─ │

        /// <summary>
        ///     (X, Y) position of editor's header.
        ///     (works only with syntax highlighting
        ///     and in multiline mode).
        /// </summary>
        private Point headerPoint;

        /// <summary>
        ///     Editor's area top-left position in console.
        /// </summary>
        private Point editBoxPoint;

        /// <summary>
        ///     Sets text in editor header.
        ///     Assigning it to null will write default header message.
        /// </summary>
        private string EditorHeader {
            set {
                WithPosition(
                    headerPoint,
                    () => {
                        ConsoleUtils.ClearLine(2);
                        if (string.IsNullOrWhiteSpace(value)) {
                            Console.Write(ScriptBenchSettings.DefaultHeader);
                        }
                        else {
                            Write(
                                (value,
                                 value.ToUpper().StartsWith("ERROR")
                                     ? ConsoleColor.Red
                                     : ConsoleColor.Yellow)
                            );
                        }
                    }
                );
            }
        }

        /// <summary>
        ///     Draws box with help on current line start.
        ///     Don't invoke it during normal editing workflow.
        /// </summary>
        private void DrawHelpBox() {
            Console.CursorLeft = 0;
            const string name = "ScriptBench";
            Console.Write(" ");
            // upper caps
            for (int i = 0; i < name.Length; i++) {
                Write(("┌───┐ ", ConsoleColor.DarkGray));
            }
            Console.WriteLine();
            Console.Write(" ");
            // letters
            foreach (char c in name) {
                Write(("│ ", ConsoleColor.DarkGray));
                WithRandomFontColor(() => { Console.Write(c); });
                Write((" │ ", ConsoleColor.DarkGray));
            }
            Console.WriteLine();
            Console.Write(" ");
            // lower caps
            for (int i = 0; i < name.Length; i++) {
                Write(("└───┘ ", ConsoleColor.DarkGray));
            }
            Console.WriteLine();
            WriteLine(("┌─────────────────────────────────────────────────────────────────┐", ConsoleColor.DarkGray));
            WriteLine(
                ("│", ConsoleColor.DarkGray),
                ("      ScriptBench - the best code editor for 19th century ;)     ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray)
            );
            WriteLine(("├─────────────────────────────────────────────────────────────────┤", ConsoleColor.DarkGray));
            WriteLine(
                ("│", ConsoleColor.DarkGray),
                (" Hotkey            Description                                   ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray)
            );
            WriteLine(("├─────────────────┬───────────────────────────────────────────────┤", ConsoleColor.DarkGray));
            WriteLine(
                ("│", ConsoleColor.DarkGray),
                (" [Esc] + [Esc]   ", ConsoleColor.White),
                ("│", ConsoleColor.DarkGray),
                (" Exit (it's just [Enter] in single-line mode.) ", ConsoleColor.White),
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
            WriteLine(("└─────────────────┴───────────────────────────────────────────────┘", ConsoleColor.DarkGray));
        }

        /// <summary>
        ///     Draws editor's top frame on current line start.
        ///     This should be called only when editor is created.
        ///     This function does nothing in single-line mode.
        /// </summary>
        private void DrawTopFrame() {
            if (settings.SingleLineMode) {
                return;
            }

            Console.CursorLeft = 0;
            Console.WriteLine("[Esc]x2: exit, [F1]: copy, [F2]: paste.");
            if (settings.SyntaxHighlighting) {
                // draw header frame
                Write(
                    ("┌──────" + new string('─', Console.BufferWidth - editBoxPoint.X) + "\n" + "│ ",
                     ScriptBenchSettings.FramesColor)
                );
                // mark position of header
                headerPoint.X = Console.CursorLeft;
                headerPoint.Y = Console.CursorTop;
                // draw editor frame
                WriteLine(
                    (ScriptBenchSettings.DefaultHeader + "\n" + "└─────┬" + new string('─', Console.BufferWidth - editBoxPoint.X),
                     ScriptBenchSettings.FramesColor)
                );
            }
            else {
                // else draw upper bound
                WriteLine(
                    ("──────┬" + new string('─', Console.BufferWidth - editBoxPoint.X),
                     ScriptBenchSettings.FramesColor)
                );
            }
        }

        /// <summary>
        ///     Draws editor's lower bound.
        ///     Should be called only when user finished his work.
        ///     This function does nothing in single-line mode.
        /// </summary>
        private void DrawBottomFrame() {
            if (settings.SingleLineMode) {
                return;
            }

            // move to editor lower bound and
            // render bottom frame
            cursorY            = lines.Count;
            Console.CursorLeft = 0;
            WriteLine(
                ("──────┴" + new string('─', Console.BufferWidth - editBoxPoint.X),
                 ScriptBenchSettings.FramesColor)
            );
        }

        /// <summary>
        ///     Prints current line number on the left side of editor.
        /// </summary>
        private void DrawCurrentLineNumber() {
            DrawLineNumber(cursorY + 1);
        }

        /// <summary>
        ///     Prints specified line number on the left side of editor.
        ///     <c>
        ///         Format: ` XXXX | `
        ///     </c>
        /// </summary>
        public static void DrawLineNumber(int lineNumber) {
            var strNum = lineNumber.ToString();
            int width  = Math.Max(strNum.Length, 4);
            string view =
                strNum.PadLeft(width  + 1)
                      .PadRight(width + 2)
              + "│ ";
            Write((view, ScriptBenchSettings.FramesColor));
        }
    }
}