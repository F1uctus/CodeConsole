using System;
using System.Drawing;
using System.Linq;

namespace CodeConsole.ScriptBench {
    public partial class ScriptBench {
        int lastRenderedLinesCount = 1;
        Point newRenderStartPosition;

        /// <summary>
        ///     Clears every line in editor
        ///     starting with specified coordinates.
        ///     Does not modify code lines.
        /// </summary>
        void ClearLines(int fromX, int fromY) {
            ConsoleUtils.WithCurrentPosition(() => {
                cursorY = fromY;
                ClearLine(false, fromX);
                cursorY++;
                for (; cursorY < lines.Count; cursorY++) {
                    ClearLine();
                }
            });
        }

        /// <summary>
        ///     Clears current line.
        ///     Does not modify code lines.
        /// </summary>
        void ClearLine(bool fullClear = false, int fromRelativeX = 0) {
            if (fullClear) {
                ConsoleUtils.ClearLine();
            }
            else {
                if (!singleLineMode) {
                    Console.CursorLeft = 0;
                    DrawCurrentLineNumber();
                }
                ConsoleUtils.ClearLine(editBoxPoint.X + fromRelativeX);
            }
        }

        /// <summary>
        ///     Highlights current code,
        ///     starting from last edited position.
        /// </summary>
        void RenderCode() {
            var longestLineLen = lines.Max(l => l.Length);
            EnsureWindowSize(longestLineLen + editBoxPoint.X + 1);
            var linesCountDifference = lines.Count - lastRenderedLinesCount;
            ConsoleUtils.WithCurrentPosition(() => {
                if (syntaxHighlighting) {
                    HighlightSyntax();
                }
                else if (!singleLineMode && linesCountDifference != 0) {
                    // rewrite all lines from cursor
                    for (; cursorY < lines.Count; cursorY++) {
                        ClearLine();
                        Console.Write(line);
                    }
                }
                else {
                    ClearLine();
                    Console.Write(line);
                }

                if (linesCountDifference < 0) {
                    // clear last line
                    cursorY = lastRenderedLinesCount - 1;
                    ClearLine(true);
                    linesCountDifference++;
                }
            });
            lastRenderedLinesCount = lines.Count;
        }

        /// <summary>
        ///     If spaces highlighting is enabled in settings,
        ///     replaces \t with tab string from settings,
        ///     then replaces spaces with unicode middle-dots (·)
        /// </summary>
        string SpacesToDots(string input) {
            if (!settings.ShowWhitespaces) {
                return input;
            }
            return input.Replace("\t", settings.Tabulation).Replace(' ', '·');
        }

        /// <summary>
        ///     Invokes highlighter to get colored tokens from text,
        ///     then writes these tokens to editor.
        ///     After that, sets editor header to first blame found by highlighter.
        /// </summary>
        void HighlightSyntax() {
            var values = highlighter.Highlight(
                lines,
                ref newRenderStartPosition,
                out var blames
            );

            cursorX = newRenderStartPosition.X;
            cursorY = newRenderStartPosition.Y;
            ClearLines(cursorX, cursorY);

            foreach (var (color, value, isWhite) in values) {
                Console.ForegroundColor = color;
                if (value.Contains("\n")) {
                    var valueLines = value.Split('\n');
                    for (var j = 0; j < valueLines.Length; j++) {
                        Console.Write(isWhite
                            ? SpacesToDots(valueLines[j])
                            : valueLines[j]);
                        if (j < valueLines.Length - 1) {
                            MoveToNextLineStart();
                        }
                    }
                    continue;
                }
                Console.Write(isWhite ? SpacesToDots(value) : value);
            }

            Console.ForegroundColor = ConsoleColor.White;

            // fill message box
            EditorHeader = blames.Count == 0
                ? null
                : blames[0].ToString();
        }
    }
}
