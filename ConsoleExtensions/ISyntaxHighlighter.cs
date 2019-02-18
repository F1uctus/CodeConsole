using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace ConsoleExtensions {
    public interface ISyntaxHighlighter {
        ConsoleCodeEditor Editor { set; }

        List<ColoredValue> Highlight(
            IEnumerable<string> codeLines,
            out Point           lastRenderEndPosition,
            List<Exception>     blames
        );
    }

    [DebuggerDisplay("{" + nameof(debuggerDisplay) + ",nq}")]
    public class ColoredValue {
        public readonly ConsoleColor Color;

        public ColoredValue(string value, ConsoleColor color) {
            Value = value;
            Color = color;
        }

        public string Value { get; private set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string debuggerDisplay =>
            $"{Color:G}: '{Value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t")}'";

        public void AppendValue(string value) {
            Value += value;
        }
    }
}