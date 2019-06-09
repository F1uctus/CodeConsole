using System;
using System.Collections.Generic;

namespace CodeConsole {
    /// <summary>
    ///     Embedded in console code editor with optional
    ///     syntax highlighting & handy keyboard shortcuts.
    ///     Count of lines in editor is limited to
    ///     <see cref="maxHighlightedLinesCount" /> to prevent performance hurting.
    /// </summary>
    public partial class ConsoleCodeEditor {
        /// <summary>
        ///     Creates new console editor instance.
        /// </summary>
        /// <param name="singleLineMode">
        ///     Editor uses only 1 editable line.
        /// </param>
        /// <param name="syntaxHighlighting">
        ///     Enable specified syntax highlighting.
        /// </param>
        /// <param name="renderWhitespaces">
        ///     Draw space characters in editor as semi-transparent dots. ( · )
        /// </param>
        /// <param name="prompt">
        ///     A permanent prompt to user to input something. Works only with
        ///     <paramref name="singleLineMode" />.
        /// </param>
        /// <param name="firstCodeLine">
        ///     Appends specified first line to the editor.
        /// </param>
        /// <param name="highlighter">
        ///     A code highlighter what is used in that editor.
        /// </param>
        public ConsoleCodeEditor(
            bool               singleLineMode,
            bool               syntaxHighlighting = false,
            bool               renderWhitespaces  = true,
            string             prompt             = "",
            string             firstCodeLine      = "",
            ISyntaxHighlighter highlighter        = null
        ) {
            if (syntaxHighlighting) {
                this.highlighter =
                    highlighter ?? throw new ArgumentNullException(nameof(highlighter));
            }

            this.singleLineMode     = singleLineMode;
            this.syntaxHighlighting = syntaxHighlighting;
            this.renderWhitespaces  = renderWhitespaces;
            space                   = renderWhitespaces ? '·' : ' ';
            if (singleLineMode && prompt.Length > 0) {
                this.prompt    = prompt;
                editBoxPoint.X = prompt.Length;
            }
            else {
                editBoxPoint.X = LineNumberWidth;
            }

            lines.Add(firstCodeLine);
        }

        /// <summary>
        ///     Starts the editor (draws bounds, highlights code, etc.)
        ///     and returns prepared code lines when user finished editing.
        /// </summary>
        public string[] RunSession() {
            ConsoleUI.ClearLine();
            if (singleLineMode) {
                Console.Write(prompt);
            }
            else {
                DrawTopFrame();
            }

            editBoxPoint.Y = Console.CursorTop;
            cursorX        = Line.Length;

            // highlight first line
            lastRenderLinesCount = 1;
            RenderCode();

            return ReadLines();
        }

        /// <summary>
        ///     Reads and processes user input.
        /// </summary>
        private string[] ReadLines() {
            // writing loop
            var exit = false;
            while (!exit) {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (HandleViewAction(key)) {
                    continue;
                }

                newRenderStartPosition.Y = cursorY;
                newRenderStartPosition.X = cursorX;

                exit = HandleEditAction(key);
            }

            DrawBottomFrame();
            return lines.Count == 0
                ? new[] {
                    ""
                }
                : lines.ToArray();
        }

        private bool HandleEditAction(ConsoleKeyInfo key) {
            switch (key.Key) {
            case ConsoleKey.Escape: {
                // use [Enter] in single line mode instead.
                if (singleLineMode) {
                    break;
                }

                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) {
                    return true;
                }

                break;
            }

            case ConsoleKey.Enter: {
                if (singleLineMode) {
                    return true;
                }

                SplitLine();
                break;
            }

            case ConsoleKey.Backspace: {
                EraseLeftChar();
                break;
            }

            case ConsoleKey.Delete: {
                EraseRightChar();
                break;
            }

            case ConsoleKey.Tab: {
                // Shift + Tab: outdent current tab
                if (key.Modifiers == ConsoleModifiers.Shift
                    && Line.StartsWith(tabulation)) {
                    Line = Line.Remove(0, tabulation.Length);
                }
                // if on the left side of cursor there are only whitespaces
                else if (string.IsNullOrWhiteSpace(Line.Substring(0, cursorX))) {
                    cursorX += tabulation.Length;
                    Line    =  tabulation + lines[cursorY];
                }
                else {
                    WriteValue(' ');
                }

                break;
            }

            case ConsoleKey.Insert: {
                // do not process insert key
                break;
            }

            // TODO (UI) add something for FN keys
            case ConsoleKey.F1:
            case ConsoleKey.F2:
            case ConsoleKey.F3:
            case ConsoleKey.F4:
            case ConsoleKey.F5:
            case ConsoleKey.F6:
            case ConsoleKey.F7:
            case ConsoleKey.F8:
            case ConsoleKey.F9:
            case ConsoleKey.F10:
            case ConsoleKey.F11:
            case ConsoleKey.F12: {
                break;
            }

            // any other char that should be passed in code
            default: {
                WriteValue(key.KeyChar);
                break;
            }
            }

            return false;
        }

        private bool HandleViewAction(ConsoleKeyInfo key) {
            var isViewFunction = true;
            switch (key.Key) {
            #region Move cursor with arrows

            case ConsoleKey.LeftArrow: {
                MoveCursor(MoveDirection.Left);
                break;
            }

            case ConsoleKey.RightArrow: {
                MoveCursor(MoveDirection.Right);
                break;
            }

            case ConsoleKey.UpArrow: {
                MoveCursor(MoveDirection.Up);
                break;
            }

            case ConsoleKey.DownArrow: {
                MoveCursor(MoveDirection.Down);
                break;
            }

            #endregion

            case ConsoleKey.Home: {
                // to line start
                cursorX = 0;
                break;
            }

            case ConsoleKey.End: {
                // to line end
                cursorX = Line.Length;
                break;
            }

            case ConsoleKey.PageUp: {
                if (cursorY - 10 >= 0) {
                    cursorY -= 10;
                }
                else {
                    cursorY = 0;
                }

                if (cursorX > Line.Length) {
                    cursorX = Line.Length;
                }

                break;
            }

            case ConsoleKey.PageDown: {
                if (cursorY + 10 < lines.Count) {
                    cursorY += 10;
                }
                else {
                    cursorY = lines.Count - 1;
                }

                if (cursorX > Line.Length) {
                    cursorX = Line.Length;
                }

                break;
            }

            default: {
                isViewFunction = false;
                break;
            }
            }

            return isViewFunction;
        }

        /// <summary>
        ///     Occurs when lines count in editor exceeds maximal allowed value.
        /// </summary>
        private class FileTooLargeException : Exception {
            public FileTooLargeException() : base(
                "File too large to display. Please use external editor."
            ) { }
        }

        #region Editor settings

        /// <summary>
        ///     Draw space characters in editor as semi-transparent dots. ( · )
        /// </summary>
        private readonly bool renderWhitespaces;

        /// <summary>
        ///     Space character in editor
        ///     (can be ' ' or '·' ).
        /// </summary>
        private readonly char space;

        /// <summary>
        ///     Tab character in editor.
        ///     Depends on <see cref="space" />
        /// </summary>
        private readonly string tabulation = new string(' ', 4);

        /// <summary>
        ///     Use only 1 editable line.
        /// </summary>
        private readonly bool singleLineMode;

        /// <summary>
        ///     User prompt used when editor
        ///     launched in single-line mode.
        /// </summary>
        private readonly string prompt;

        /// <summary>
        ///     Highlight user input with specified <see cref="highlighter" />.
        /// </summary>
        private readonly bool syntaxHighlighting;

        /// <summary>
        ///     Current console highlighter.
        /// </summary>
        private readonly ISyntaxHighlighter highlighter;

        #endregion

        #region Input text & cursor

        /// <summary>
        ///     Editor's code lines.
        /// </summary>
        private readonly List<string> lines = new List<string>();

        /// <summary>
        ///     Current editing line.
        ///     This property automatically calls
        ///     <see cref="RenderCode" /> when modified.
        ///     You should use <see cref="lines" />[<see cref="cursorY" />] when you
        ///     don't want to re-render input instead.
        /// </summary>
        private string Line {
            get => lines[cursorY];
            set {
                lines[cursorY] = value;
                // if line modified - it should be re-rendered
                RenderCode();
            }
        }

        /// <summary>
        ///     A wrapper over <see cref="Console.CursorLeft" />.
        ///     Position is relative to <see cref="editBoxPoint" />.
        ///     Automatically grows or reduces <see cref="Console.BufferWidth" />
        ///     based on longest line length.
        /// </summary>
        private int cursorX {
            // check that value is in bounds
            get => Console.CursorLeft - editBoxPoint.X;
            set {
                int length = value + editBoxPoint.X;
                // resize buffer
//                if (length >= Console.BufferWidth) {
//                    // grow buffer width if cursor out of it.
//                    Console.BufferWidth = length + 1;
//                }
//                else {
//                    // buffer width should be equal to the longest line length.
//                    int maxLength =
//                        lines.Max(x => x.Length) + editBoxPoint.X;
//                    if (maxLength > Console.WindowWidth) {
//                        Console.BufferWidth = maxLength + 1;
//                    }
//                }

                Console.CursorLeft = length;
            }
        }

        /// <summary>
        ///     A wrapper over <see cref="Console.CursorTop" />.
        ///     Position is relative to <see cref="editBoxPoint" />.
        /// </summary>
        private int cursorY {
            get => Console.CursorTop - editBoxPoint.Y;
            set => Console.CursorTop = value + editBoxPoint.Y;
        }

        #endregion

        #region Text edit actions

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
            if (cursorX == 0
                && cursorY == 0) {
                return;
            }

            // if cursor in current line
            if (cursorX > 0) {
                newRenderStartPosition.X--;
                // if erasing empty end of line
                if (cursorX == Line.Length
                    && Line[cursorX - 1] == ' ') {
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
            if (cursorY == lines.Count - 1
                && cursorX == Line.Length) {
                return;
            }

            // cursor doesn't move when removing right character
            ConsoleUI.WithCurrentPosition(
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
                if (syntaxHighlighting && !char.IsWhiteSpace(value)) {
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
                throw new ArgumentOutOfRangeException(
                    nameof(cursorX),
                    nameof(ConsoleCodeEditor) + ": cursor X went through end of line."
                );
            }

            cursorX++;
        }

        #endregion
    }
}