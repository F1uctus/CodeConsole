using System;
using System.Collections.Generic;
using System.Drawing;

namespace CodeConsole.ScriptBench {
    public partial class ScriptBench {
        private int   lastRenderedLinesCount = 1;
        private Point newRenderStartPosition;

        /// <summary>
        ///     Highlights current code,
        ///     starting from last edited position.
        /// </summary>
        private void RenderCode() {
            int linesCountDifference = lines.Count - lastRenderedLinesCount;
            ConsoleUtils.WithCurrentPosition(
                () => {
                    if (settings.SyntaxHighlighting) {
                        HighlightSyntax();
                    }
                    else if (!settings.SingleLineMode && linesCountDifference != 0) {
                        // rewrite all lines from cursor
                        for (; cursorY < lines.Count; cursorY++) {
                            ClearLine();
                            Console.Write(Line);
                        }
                    }
                    else {
                        ClearLine();
                        Console.Write(Line);
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
        ///     Invokes highlighter to get colored tokens from text,
        ///     then writes these tokens to editor.
        ///     After that, sets editor header to first blame found by highlighter.
        /// </summary>
        private void HighlightSyntax() {
            List<ColoredValue> values =
                settings.Highlighter.Highlight(
                    lines,
                    ref newRenderStartPosition,
                    out List<Exception> blames
                );

            cursorX = newRenderStartPosition.X;
            cursorY = newRenderStartPosition.Y;
            ClearLines(cursorX, cursorY);

            foreach (ColoredValue value in values) {
                Console.ForegroundColor = value.Color;
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
            if (blames.Count == 0) {
                EditorHeader = null;
            }
            else {
                EditorHeader = blames[0].ToString();
            }
        }
    }
}