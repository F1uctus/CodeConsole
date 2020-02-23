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
            Console.WriteLine(
                string.Join(
                    Environment.NewLine,
                    "┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐",
                    "│ S │ │ c │ │ r │ │ i │ │ p │ │ t │ │ B │ │ e │ │ n │ │ c │ │ h │",
                    "└───┘ └───┘ └───┘ └───┘ └───┘ └───┘ └───┘ └───┘ └───┘ └───┘ └───┘",
                    "┌─────────────────────────────────────────────────────────────────┐",
                    "│      ScriptBench - the best code editor for 19th century ;)     │",
                    "├─────────────────────────────────────────────────────────────────┤",
                    "│ Hotkey            Description                                   │",
                    "├─────────────────┬───────────────────────────────────────────────┤",
                    "│ [Esc] + [Esc]   │ Exit (it's just [Enter] in single-line mode.) │",
                    "│ [F1]            │ Copy all text                                 │",
                    "│ [F2]            │ Paste (Regular Ctrl-V works, but it's         │",
                    "│                 │     much slower because of implementation)    │",
                    "│ [Tab]           │ Insert 4 spaces to the line start.            │",
                    "│ [Shift] + [Tab] │ Remove 4 spaces from the line start.          │",
                    "└─────────────────┴───────────────────────────────────────────────┘"
                )
            );
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