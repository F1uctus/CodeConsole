using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CodeConsole.ScriptBench {
    public partial class ScriptBench {
        private int   lastRenderedLinesCount = 1;
        private Point newRenderStartPosition;

        /// <summary>
        ///     Clears every line in editor
        ///     starting with specified coordinates.
        ///     Does not modify code lines.
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
        ///     Does not modify code lines.
        /// </summary>
        private void ClearLine(bool fullClear = false, int fromRelativeX = 0) {
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
        private void RenderCode() {
            int longestLineLen = lines.Max(l => l.Length);
            EnsureWindowSize(longestLineLen + editBoxPoint.X + 1);
            int linesCountDifference = lines.Count           - lastRenderedLinesCount;
            ConsoleUtils.WithCurrentPosition(
                () => {
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
                }
            );
            lastRenderedLinesCount = lines.Count;
        }

        /// <summary>
        ///     If spaces highlighting is enabled in settings,
        ///     replaces \t with tab string from settings,
        ///     then replaces spaces with unicode middle-dots (·)
        /// </summary>
        private string SpacesToDots(string input) {
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
        private void HighlightSyntax() {
            List<ColoredValue> values = highlighter.Highlight(
                lines,
                ref newRenderStartPosition,
                out IReadOnlyList<Exception> blames
            );

            cursorX = newRenderStartPosition.X;
            cursorY = newRenderStartPosition.Y;
            ClearLines(cursorX, cursorY);

            foreach (ColoredValue value in values) {
                Console.ForegroundColor = value.Color;
                if (value.Value.Contains("\n")) {
                    string[] valueLines = value.Value.Split('\n');
                    for (var j = 0; j < valueLines.Length; j++) {
                        Console.Write(value.IsWhite ? SpacesToDots(valueLines[j]) : valueLines[j]);
                        if (j < valueLines.Length - 1) {
                            MoveToNextLineStart();
                        }
                    }
                    continue;
                }
                Console.Write(value.IsWhite ? SpacesToDots(value.Value) : value.Value);
            }

            Console.ForegroundColor = ConsoleColor.White;

            // fill message box
            if (blames.Count == 0) {
                EditorHeader = null;
            }
            else {
                EditorHeader = blames[0].ToString();
            }
        }
    }
}
