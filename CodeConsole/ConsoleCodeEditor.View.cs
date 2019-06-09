using System;
using System.Collections.Generic;
using System.Drawing;

namespace CodeConsole {
    public partial class ConsoleCodeEditor {
        private const  string          defaultMessage = "No errors found.";
        private static List<Exception> blames;
        private        int             lastRenderLinesCount;
        private        Point           newRenderStartPosition;

        /// <summary>
        ///     Full rewriting and highlighting of current code.
        /// </summary>
        private void RenderCode() {
            int linesCountDifference = lines.Count - lastRenderLinesCount;
            ConsoleUI.WithCurrentPosition(
                () => {
                    if (syntaxHighlighting) {
                        HighlightSyntax();
                    }
                    else if (!singleLineMode && linesCountDifference != 0) {
                        // rewrite all lines from cursorY
                        for (; cursorY < lines.Count; cursorY++) {
                            ClearLine();
                            Console.Write(Line);
                        }
                    }
                    else {
                        ClearLine();
                        Console.Write(Line);
                    }

                    // if line arrived
                    if (linesCountDifference == 1) {
                        // change line numbers below
                        cursorY            = lines.Count - 1;
                        Console.CursorLeft = 0;
                        PrintCurrentLineNumber();
                        linesCountDifference--;
                    }
                    // if line removed
                    else if (linesCountDifference == -1) {
                        // clear last line
                        cursorY = lastRenderLinesCount - 1;
                        ClearLine(true);
                        linesCountDifference++;
                    }
                }
            );
            lastRenderLinesCount = lines.Count;
        }

        private void HighlightSyntax() {
            List<ColoredValue> values = highlighter.Highlight(
                lines,
                ref newRenderStartPosition,
                out blames
            );

            cursorX = newRenderStartPosition.X;
            cursorY = newRenderStartPosition.Y;
            ClearLines(newRenderStartPosition.X, newRenderStartPosition.Y);

            // TODO (UI) add render of whitespaces (as · with dark-gray color)
            foreach (ColoredValue value in values) {
                Console.ForegroundColor = value.Color;
                //if (renderWhitespaces) {
                //    values[i].Value = values[i].Value.Replace(' ', space);
                //}
                if (value.Value.Contains("\n")) {
                    string[] valueLines = value.Value.Split('\n');
                    for (var j = 0; j < valueLines.Length - 1; j++) {
                        Console.Write(valueLines[j]);
                        MoveToNextLineStart();
                    }

                    Console.Write(valueLines[valueLines.Length - 1]);
                    continue;
                }

                Console.Write(value.Value);
            }

            Console.ForegroundColor = ConsoleColor.White;

            // fill message box
            if (blames.Count != 0) {
                PrintBlame(blames[0]);
            }
            else {
                ResetMessageBox();
            }
        }

        private enum MoveDirection {
            Left,
            Right,
            Up,
            Down
        }

        #region Editor view properties

        private const int maxHighlightedLinesCount = 300;

        /// <summary>
        ///     Width of line number field at left side of code editor.
        /// </summary>
        public const int LineNumberWidth = 7; // " XXX | "

        /// <summary>
        ///     Editor's (X, Y) position in console.
        /// </summary>
        private Point editBoxPoint;

        /// <summary>
        ///     (X, Y) position of editor's message box.
        ///     (works only with syntax highlighting
        ///     and not in single line mode).
        /// </summary>
        private Point messageBoxPoint;

        #endregion

        #region Functions for drawing editor frames and simple GUI

        private void DrawTopFrame() {
            Console.WriteLine("Press [Esc] twice to exit code editor");
            if (syntaxHighlighting) {
                Console.BufferWidth = 400;
                // draw message box frame
                ConsoleUI.Write(
                    ("┌─────" + new string('─', Console.BufferWidth - editBoxPoint.X) + "\n" + "| ",
                     ConsoleColor.DarkGray)
                );
                // mark position of message box
                messageBoxPoint.X = Console.CursorLeft;
                messageBoxPoint.Y = Console.CursorTop;
                // draw editor frame
                ConsoleUI.WriteLine(
                    ("No errors\n" + "└────┬" + new string('─', Console.BufferWidth - editBoxPoint.X),
                     ConsoleColor.DarkGray)
                );
            }
            else {
                // else draw upper bound
                ConsoleUI.WriteLine(
                    ("─────┬" + new string('─', Console.BufferWidth - editBoxPoint.X),
                     ConsoleColor.DarkGray)
                );
            }
        }

        private void DrawBottomFrame() {
            if (singleLineMode) {
                return;
            }

            // move to editor lower bound and
            // render bottom frame
            cursorY            = lines.Count;
            Console.CursorLeft = 0;
            ConsoleUI.WriteLine(
                ("─────┴" + new string('─', Console.BufferWidth - editBoxPoint.X),
                 ConsoleColor.DarkGray)
            );
        }

        /// <summary>
        ///     In multiline mode, prints line
        ///     number on left side of editor.
        ///     That number not included in code.
        /// </summary>
        private void PrintCurrentLineNumber() {
            PrintLineNumber(cursorY + 1);

            if (syntaxHighlighting && lines.Count > maxHighlightedLinesCount) {
                throw new FileTooLargeException();
            }
        }

        /// <summary>
        ///     Prints specified line number
        ///     on left side of console.
        /// </summary>
        public static void PrintLineNumber(int lineNumber) {
            var view = "";

            // left align line number
            //    X |
            if (lineNumber < 10) {
                view += "   ";
            }
            //   XX |
            else if (lineNumber < 100) {
                view += "  ";
            }
            //  XXX |
            else {
                view += " ";
            }

            // append line number and right aligner
            view += lineNumber + " | ";
            ConsoleUI.Write((view, ConsoleColor.DarkGray));
        }

        #endregion

        #region Clear functions

        private void ClearLines(int fromX, int fromY) {
            ConsoleUI.WithCurrentPosition(
                () => {
                    cursorY = fromY;
                    ClearLine(false, fromX);
                    cursorY++;
                    for (; cursorY < lines.Count; cursorY++) {
                        ClearLine();
                    }
                }
            );
        }

        /// <summary>
        ///     Clears current line.
        /// </summary>
        private void ClearLine(bool fullClear = false, int fromRelativeX = 0) {
            if (fullClear) {
                ConsoleUI.ClearLine();
            }
            else if (singleLineMode) {
                ConsoleUI.ClearLine(prompt.Length + fromRelativeX);
            }
            else {
                Console.CursorLeft = 0;
                PrintCurrentLineNumber();
                ConsoleUI.ClearLine(LineNumberWidth + fromRelativeX);
            }
        }

        #endregion

        #region Message box functions

        private void PrintBlame(Exception error) {
            ClearMessageBox();
            ConsoleUI.WithPosition(
                messageBoxPoint,
                () => ConsoleUI.Write(
                    (error.Message,
                     error.Message.ToUpper().StartsWith("ERROR")
                         ? ConsoleColor.Red
                         : ConsoleColor.Yellow)
                )
            );
        }

        /// <summary>
        ///     In multiline mode with syntax highlighting,
        ///     erases current displaying message in editor message box.
        /// </summary>
        private void ClearMessageBox() {
            ConsoleUI.WithPosition(messageBoxPoint, () => ConsoleUI.ClearLine(2));
        }

        /// <summary>
        ///     In multiline mode with syntax highlighting,
        ///     erases current displaying message in editor message box.
        /// </summary>
        private void ResetMessageBox() {
            ConsoleUI.WithPosition(
                messageBoxPoint,
                () => {
                    ConsoleUI.ClearLine(2);
                    Console.Write(defaultMessage);
                }
            );
        }

        #endregion

        #region Move cursor

        /// <summary>
        ///     Moves cursor in edit box to specified <see cref="direction" />.
        /// </summary>
        /// <param name="direction">Direction of cursor move.</param>
        /// <param name="count">Count of characters to move cursor by.</param>
        private void MoveCursor(MoveDirection direction, int count = 1) {
            switch (direction) {
            case MoveDirection.Left: {
                // if reach first line start
                if (cursorY == 0
                    && cursorX - count < 0) {
                    return;
                }

                // if fits in current line
                if (cursorX - count >= 0) {
                    cursorX -= count;
                }
                // move line up
                else if (!singleLineMode) {
                    cursorY--;
                    cursorX = Line.Length; // no - 1
                }

                break;
            }

            case MoveDirection.Right: {
                // if reach last line end
                if (cursorY == lines.Count - 1
                    && cursorX + count > Line.Length) {
                    return;
                }

                // if fits in current line
                if (cursorX + count <= Line.Length) {
                    cursorX += count;
                }
                // move line down
                else if (!singleLineMode) {
                    cursorY++;
                    cursorX = 0;
                }

                break;
            }

            case MoveDirection.Up: {
                // if on first line
                if (cursorY == 0 || singleLineMode) {
                    return;
                }

                cursorY--;
                // if cursor moves at empty space upside
                if (cursorX >= Line.Length) {
                    cursorX = Line.Length;
                }

                break;
            }

            case MoveDirection.Down: {
                // if on last line
                if (cursorY == lines.Count - 1 || singleLineMode) {
                    return;
                }

                cursorY++;
                // if cursor moves at empty space downside
                if (cursorX >= Line.Length) {
                    cursorX = Line.Length;
                }

                break;
            }
            }
        }

        private void MoveToNextLineStart() {
            cursorY++;
            if (cursorY == lines.Count) {
                try {
                    ClearLine();
                }
                catch (FileTooLargeException ex) {
                    PrintBlame(ex);
                }
            }

            cursorX = 0;
        }

        #endregion
    }
}