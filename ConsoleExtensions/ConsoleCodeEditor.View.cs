using System;
using System.Collections.Generic;
using System.Drawing;

namespace ConsoleExtensions {
    public partial class ConsoleCodeEditor {
        private const string defaultMessage = "No errors found.";

        private static readonly List<Exception> blames = new List<Exception>();

        private int   lastRenderLinesCount;
        private Point newRenderStartPosition;

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
                        for (; CursorY < lines.Count; CursorY++) {
                            ClearLine();
                            ConsoleUI.Write(Line);
                        }
                    }
                    else {
                        ClearLine();
                        ConsoleUI.Write(Line);
                    }
                    // if line arrived
                    if (linesCountDifference == 1) {
                        // change line numbers below
                        CursorY            = lines.Count - 1;
                        Console.CursorLeft = 0;
                        PrintCurrentLineNumber();
                        linesCountDifference--;
                    }
                    // if line removed
                    else if (linesCountDifference == -1) {
                        // clear last line
                        CursorY = lastRenderLinesCount - 1;
                        ClearLine(true);
                        linesCountDifference++;
                    }
                }
            );
            lastRenderLinesCount = lines.Count;
        }

        private void HighlightSyntax() {
            blames.Clear();
            List<ColoredValue> values = highlighter.Highlight(
                lines,
                out newRenderStartPosition,
                blames
            );

            CursorX = newRenderStartPosition.X;
            CursorY = newRenderStartPosition.Y;
            ClearLines(newRenderStartPosition.X, newRenderStartPosition.Y);

            // TODO: add render of whitespaces (as · with dark-gray color)
            foreach (ColoredValue value in values) {
                Console.ForegroundColor = value.Color;
                //if (renderWhitespaces) {
                //    values[i].Value = values[i].Value.Replace(' ', space);
                //} TODO add whitespaces rendering
                if (value.Value.Contains("\n")) {
                    string[] valueLines = value.Value.Split(
                        new[] { '\n' },
                        StringSplitOptions.None
                    );
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

        public const int MaxHighlightedLinesCount = 300;

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
        ///     (works only with <see cref="syntaxHighlighting" />
        ///     and not in <see cref="singleLineMode" />).
        /// </summary>
        private Point messageBoxPoint;

        #endregion

        #region Functions for drawing editor frames and simple GUI

        private void DrawTopFrame() {
            ConsoleUI.WriteLine("Press [Esc] twice to exit code editor");
            if (syntaxHighlighting) {
                // draw message box frame
                ConsoleUI.Write(
                    ("┌─────" + new string('─', Console.BufferWidth - editBoxPoint.X) + "\n" + "| ",
                     ConsoleColor.DarkGray)
                );
                // save position of message box
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
            if (!singleLineMode) {
                // move to editor lower bound and
                // render bottom frame
                CursorY            = lines.Count;
                Console.CursorLeft = 0;
                ConsoleUI.WriteLine(
                    ("─────┴" + new string('─', Console.BufferWidth - editBoxPoint.X),
                     ConsoleColor.DarkGray)
                );
            }
        }

        /// <summary>
        ///     In multiline mode, prints line
        ///     number on left side of editor.
        ///     That number not included in code.
        /// </summary>
        private void PrintCurrentLineNumber() {
            PrintLineNumber(CursorY + 1);

            if (syntaxHighlighting && lines.Count > MaxHighlightedLinesCount) {
                // file too large to display with syntax highlighting.
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

        public void ClearLinesFromCurrent() {
            ClearLines(0, CursorY);
        }

        public void ClearLines(int fromX, int fromY) {
            ConsoleUI.WithCurrentPosition(
                () => {
                    CursorY = fromY;
                    ClearLine(false, fromX);
                    CursorY++;
                    for (; CursorY < lines.Count; CursorY++) {
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
                     error.Message.ToLower().StartsWith("error")
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
                    if (CursorY == 0 && CursorX - count < 0) {
                        return;
                    }
                    // if fits in current line
                    if (CursorX - count >= 0) {
                        CursorX -= count;
                    }
                    // move line up
                    else if (!singleLineMode) {
                        CursorY--;
                        CursorX = Line.Length; // no - 1
                    }
                    break;
                }
                case MoveDirection.Right: {
                    // if reach last line end
                    if (CursorY == lines.Count - 1 && CursorX + count > Line.Length) {
                        return;
                    }
                    // if fits in current line
                    if (CursorX + count <= Line.Length) {
                        CursorX += count;
                    }
                    // move line down
                    else if (!singleLineMode) {
                        CursorY++;
                        CursorX = 0;
                    }
                    break;
                }
                case MoveDirection.Up: {
                    // if on first line
                    if (CursorY == 0 || singleLineMode) {
                        return;
                    }
                    CursorY--;
                    // if cursor moves at empty space upside
                    if (CursorX >= Line.Length) {
                        CursorX = Line.Length;
                    }
                    break;
                }
                case MoveDirection.Down: {
                    // if on last line
                    if (CursorY == lines.Count - 1 || singleLineMode) {
                        return;
                    }
                    CursorY++;
                    // if cursor moves at empty space downside
                    if (CursorX >= Line.Length) {
                        CursorX = Line.Length;
                    }
                    break;
                }
            }
        }

        public void SetCursor(int x, int y) {
            CursorX = x;
            CursorY = y;
        }

        public void MoveToNextLineStart() {
            CursorY++;
            if (CursorY == lines.Count) {
                try {
                    ClearLine();
                }
                catch (FileTooLargeException ex) {
                    PrintBlame(ex);
                }
            }
            CursorX = 0;
        }

        #endregion
    }
}