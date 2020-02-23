using System;

namespace CodeConsole.ScriptBench {
    public partial class ScriptBench {
        // This part contains complex editing logic.
        // IDK now, how it works, but it means not everything works
        // as expected, at the moment as you came here to fix it.

        /// <summary>
        ///     Splits line at cursor position into 2 lines.
        /// </summary>
        private void SplitLine() {
            newRenderStartPosition.X = 0;
            string tailPiece = Line.Substring(cursorX);
            if (tailPiece.Length > 0) {
                lines[cursorY] = lines[cursorY].Substring(0, cursorX);
            }

            // add tail to next line
            if (cursorY + 1 == lines.Count) {
                lines.Add(tailPiece);
            }
            else {
                lines.Insert(cursorY + 1, tailPiece);
            }

            // re-render lower lines
            RenderCode();
            MoveToNextLineStart();
        }

        /// <summary>
        ///     Erases character on the left side of cursor.
        /// </summary>
        private void EraseLeftChar() {
            // if on first line start
            if (cursorX == 0 && cursorY == 0) {
                return;
            }

            // if cursor in current line
            if (cursorX > 0) {
                newRenderStartPosition.X--;
                // if erasing empty end of line
                if (cursorX == Line.Length && Line[cursorX - 1] == ' ') {
                    lines[cursorY] = lines[cursorY].Substring(0, Line.Length - 1);
                }
                else {
                    Line = Line.Remove(cursorX - 1, 1);
                }

                cursorX--;
            }
            // cursor X == 0
            else {
                string removingLine = Line;
                lines.RemoveAt(cursorY);
                cursorY--;
                // append removing line if it is not empty
                if (removingLine.Length > 0) {
                    newRenderStartPosition.Y--;
                    lines[cursorY] += removingLine;
                    cursorX        =  Line.Length - removingLine.Length;
                }
                else {
                    cursorX = Line.Length;
                }

                // re-render lower lines
                RenderCode();
            }
        }

        /// <summary>
        ///     Erases character on the right side of cursor.
        /// </summary>
        private void EraseRightChar() {
            // if on last line end
            if (cursorY == lines.Count - 1 && cursorX == Line.Length) {
                return;
            }

            // cursor doesn't move when removing right character
            ConsoleUtils.WithCurrentPosition(
                () => {
                    if (cursorX == Line.Length) {
                        // remove current line
                        // append next line to current
                        lines[cursorY] += lines[cursorY + 1];
                        // remove next line
                        lines.RemoveAt(cursorY + 1);
                        RenderCode();
                    }
                    // if cursor inside the current line
                    else if (Line.Substring(cursorX).TrimEnd().Length == 0) {
                        // don't redraw line when at the right
                        // side of cursor are only whitespaces.
                        lines[cursorY] = lines[cursorY].Substring(0, Line.Length - 1);
                    }
                    else {
                        Line = Line.Remove(cursorX, 1);
                    }
                }
            );
        }

        /// <summary>
        ///     Writes specified <see cref="value" /> to editor.
        /// </summary>
        private void WriteValue(char value) {
            // write value
            if (cursorX == Line.Length) {
                // at end of line
                if (settings.SyntaxHighlighting && !char.IsWhiteSpace(value)) {
                    Line += value;
                }
                else {
                    lines[cursorY] += value;
                    Console.Write(value);
                    return;
                }
            }
            else if (cursorX < Line.Length) {
                Line = Line.Insert(cursorX, value.ToString());
            }
            else {
                // actually, this should never happen ;)
                throw new ArgumentOutOfRangeException(
                    nameof(cursorX),
                    "Cursor somehow went through end of line. Deal with it by yourself."
                );
            }

            cursorX++;
        }

        /// <summary>
        ///     Clears every line in editor
        ///     starting with specified coordinates.
        /// </summary>
        private void ClearLines(int fromX, int fromY) {
            ConsoleUtils.WithCurrentPosition(
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
                ConsoleUtils.ClearLine();
            }
            else {
                if (!settings.SingleLineMode) {
                    Console.CursorLeft = 0;
                    DrawCurrentLineNumber();
                }
                ConsoleUtils.ClearLine(editBoxPoint.X + fromRelativeX);
            }
        }
    }
}