using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Web;

namespace CodeConsole {
    public interface IScriptBenchSyntaxHighlighter {
        List<ColoredValue> Highlight(
            IEnumerable<string>          codeLines,
            ref Point                    lastRenderEndPosition,
            out IReadOnlyList<Exception> blames
        );

        List<ColoredValue> Highlight(string code);
    }

    [DebuggerDisplay("{" + nameof(debuggerDisplay) + ",nq}")]
    public class ColoredValue {
        public ConsoleColor Color   { get; }
        public string       Value   { get; set; }
        public bool         IsWhite { get; }

        public ColoredValue(string value, ConsoleColor color, bool isWhite = false) {
            Value   = value;
            Color   = color;
            IsWhite = isWhite;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string debuggerDisplay =>
            $"{Color:G}: '{HttpUtility.JavaScriptStringEncode(Value)}'";
    }
}