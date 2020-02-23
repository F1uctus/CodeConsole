using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Web;

namespace CodeConsole {
    public interface ISyntaxHighlighter {
        List<ColoredValue> Highlight(
            List<string>        codeLines,
            ref Point           lastRenderEndPosition,
            out List<Exception> blames
        );

        List<ColoredValue> Highlight(string code);
    }

    [DebuggerDisplay("{" + nameof(debuggerDisplay) + ",nq}")]
    public class ColoredValue {
        public ConsoleColor Color { get; }
        public string       Value { get; set; }

        public ColoredValue(string value, ConsoleColor color) {
            Value = value;
            Color = color;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string debuggerDisplay =>
            $"{Color:G}: '{HttpUtility.JavaScriptStringEncode(Value)}'";
    }
}