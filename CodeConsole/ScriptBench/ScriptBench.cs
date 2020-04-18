using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TextCopy;

namespace CodeConsole.ScriptBench {
    /// <summary>
    ///     ScriptBench - console code editor with optional
    ///     syntax highlighting & handy keyboard shortcuts.
    /// </summary>
    public partial class ScriptBench {
        private static readonly Assembly coreAsm = Assembly.GetExecutingAssembly();
        public static readonly  string   Version = coreAsm.GetName().Version.ToString();

        /// <summary>
        ///     Editor's instance-specific settings.
        /// </summary>
        private readonly ScriptBenchSettings settings;

        /// <summary>
        ///     Use only 1 editable line.
        /// </summary>
        private readonly bool singleLineMode;

        /// <summary>
        ///     Current enabled syntax highlighter.
        /// </summary>
        private readonly IScriptBenchSyntaxHighlighter highlighter;

        /// <summary>
        ///     Highlight user input with specified <see cref="highlighter" />.
        ///     True if highlighter is not null.
        /// </summary>
        private bool syntaxHighlighting => highlighter != null;

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
        private string line {
            get => lines[cursorY];
            set {
                lines[cursorY] = value;
                // render when modified
                RenderCode();
            }
        }

        /// <summary>
        ///     A wrapper over <see cref="Console.CursorLeft" />.
        ///     Position is relative to <see cref="editBoxPoint" />.
        /// </summary>
        private int cursorX {
            // check that value is in bounds
            get => Console.CursorLeft - editBoxPoint.X;
            set => Console.CursorLeft = value + editBoxPoint.X;
        }

        /// <summary>
        ///     A wrapper over <see cref="Console.CursorTop" />.
        ///     Position is relative to <see cref="editBoxPoint" />.
        /// </summary>
        private int cursorY {
            get => Console.CursorTop - editBoxPoint.Y;
            set {
                int top = value + editBoxPoint.Y;
                if (Console.BufferHeight <= top) {
                    Console.BufferHeight = top + 1;
                }
                Console.CursorTop = top;
            }
        }

        /// <summary>
        ///     Initializes the editor background.
        ///     To actually launch it with UI,
        ///     use <see cref="Run"/> method.
        /// </summary>
        public ScriptBench(
            ScriptBenchSettings           settings       = null,
            string                        firstCodeLine  = "",
            bool                          singleLineMode = false,
            IScriptBenchSyntaxHighlighter highlighter    = null
        ) {
            this.settings       = settings ?? ScriptBenchSettings.FromConfigFile();
            this.singleLineMode = singleLineMode;
            this.highlighter    = highlighter;
            lines.Add(firstCodeLine);
        }

        /// <summary>
        ///     Starts the editor (does UI drawing, highlighting, etc).
        ///     Returns code lines when user finished editing.
        /// </summary>
        public string[] Run() {
            if (singleLineMode) {
                ConsoleUtils.ClearLine();
                Console.Write(settings.SingleLinePrompt);
            }

            EnsureWindowSize();
            DrawTopFrame();

            // mark editor's edit box coordinates
            if (singleLineMode) {
                editBoxPoint.X = settings.SingleLinePrompt.Length;
            }
            else {
                editBoxPoint.X = 8;
            }
            editBoxPoint.Y = Console.CursorTop;
            cursorX        = line.Length;

            // highlight first line
            RenderCode();

            var exit = false;
            while (!exit) {
                ConsoleKeyInfo key = Console.ReadKey(true);
                EnsureWindowSize();
                if (HandleCursorAction(key)) {
                    continue;
                }
                exit = HandleEditAction(key);
            }

            // remove empty editor box
            if (!singleLineMode && lines.Count == 1 && string.IsNullOrWhiteSpace(lines[0])) {
                ClearLine(true);
                for (var i = 0; i < 3; i++) {
                    Console.CursorTop--;
                    ClearLine(true);
                }
                return lines.ToArray();
            }

            DrawBottomFrame();

            settings.CreateMissingConfig();

            return lines.Count == 0
                ? new[] {
                    ""
                }
                : lines.ToArray();
        }

        /// <summary>
        ///     Handles actions that causes cursor to move cursor somehow.
        ///     Doesn't mess with editing code at all.
        /// </summary>
        private bool HandleCursorAction(ConsoleKeyInfo key) {
            var handled = true;
            switch (key.Key) {
            #region Move cursor with arrows

            case ConsoleKey.LeftArrow:
            case ConsoleKey.RightArrow:
            case ConsoleKey.UpArrow:
            case ConsoleKey.DownArrow: {
                MoveCursor(key.Key);
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
                cursorX = line.Length;
                break;
            }

            case ConsoleKey.PageUp: {
                if (cursorY - 10 >= 0) {
                    cursorY -= 10;
                }
                else {
                    cursorY = 0;
                }

                if (cursorX > line.Length) {
                    cursorX = line.Length;
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

                if (cursorX > line.Length) {
                    cursorX = line.Length;
                }

                break;
            }

            default: {
                handled = false;
                break;
            }
            }

            if (handled) {
                newRenderStartPosition.Y = cursorY;
                newRenderStartPosition.X = cursorX;
            }

            return handled;
        }

        /// <summary>
        ///     Handles actions that cause code editing.
        ///     Returns true if user requested editor exit.
        /// </summary>
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
                if (key.Modifiers.HasFlag(ConsoleModifiers.Shift)
                 && line.StartsWith(settings.Tabulation)) {
                    line = line.Remove(0, settings.Tabulation.Length);
                }
                // if on the left side of cursor there are only whitespaces
                else if (string.IsNullOrWhiteSpace(line.Substring(0, cursorX))) {
                    cursorX += settings.Tabulation.Length;
                    line    =  settings.Tabulation + lines[cursorY];
                }
                else {
                    WriteValue(' ');
                }

                break;
            }

            case ConsoleKey.F1: {
                // copy
                Clipboard.SetText(string.Join("\n", lines));
                break;
            }

            case ConsoleKey.F2: {
                // paste
                List<string> linesToPaste = Clipboard.GetText()
                                                     .Replace("\t", settings.Tabulation)
                                                     .Split(
                                                         new[] {
                                                             Environment.NewLine
                                                         },
                                                         StringSplitOptions.None
                                                     )
                                                     .ToList();
                if (linesToPaste.Count > 0) {
                    int k = cursorY;
                    lines[k] += linesToPaste[0];
                    for (var i = 1; i < linesToPaste.Count; i++) {
                        k++;
                        if (k >= lines.Count) {
                            lines.Add(linesToPaste[i]);
                        }
                        else {
                            lines.Insert(k, linesToPaste[i]);
                        }
                    }
                    RenderCode();
                    cursorY = k;
                    cursorX = lines[k].Length;
                }
                break;
            }

            // TODO (UI) add something for FN keys
            case ConsoleKey.F3: {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.F3) {
                    ClearAll();
                }

                break;
            }
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

            case ConsoleKey.Insert: {
                // ignore insert key
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

        private int lastXPosition;

        /// <summary>
        ///     Moves cursor in edit box to specified <see cref="direction" /> by specified <see cref="length"/>.
        /// </summary>
        private void MoveCursor(ConsoleKey direction, int length = 1) {
            switch (direction) {
            case ConsoleKey.LeftArrow: {
                // if reach first line start
                if (cursorY == 0 && cursorX - length < 0) {
                    return;
                }

                // if fits in current line
                if (cursorX - length >= 0) {
                    cursorX -= length;
                }
                // move line up
                else if (!singleLineMode) {
                    cursorY--;
                    cursorX = line.Length; // no - 1
                }
                lastXPosition = cursorX;
                break;
            }

            case ConsoleKey.RightArrow: {
                // if reach last line end
                if (cursorY == lines.Count - 1 && cursorX + length > line.Length) {
                    return;
                }

                // if fits in current line
                if (cursorX + length <= line.Length) {
                    cursorX += length;
                }
                // move line down
                else if (!singleLineMode) {
                    cursorY++;
                    cursorX = 0;
                }
                lastXPosition = cursorX;
                break;
            }

            case ConsoleKey.UpArrow: {
                // if on first line
                if (cursorY == 0 || singleLineMode) {
                    return;
                }

                cursorY--;
                // if cursor moves at empty space upside
                if (cursorX > line.Length) {
                    cursorX = line.Length;
                }
                else if (line.Length >= lastXPosition) {
                    cursorX = lastXPosition;
                }

                break;
            }

            case ConsoleKey.DownArrow: {
                // if on last line
                if (cursorY == lines.Count - 1 || singleLineMode) {
                    return;
                }

                cursorY++;
                // if cursor moves at empty space downside
                if (cursorX > line.Length) {
                    cursorX = line.Length;
                }
                else if (line.Length >= lastXPosition) {
                    cursorX = lastXPosition;
                }

                break;
            }
            default: {
                throw new Exception($"Cannot move cursor with specified key: '{direction:G}'.");
            }
            }
        }

        /// <summary>
        ///     Moves cursor to next line start.
        ///     Automatically writes appropriate line number. 
        /// </summary>
        private void MoveToNextLineStart() {
            cursorY++;
            if (cursorY == lines.Count) {
                ClearLine();
            }

            cursorX = 0;
        }
    }
}
