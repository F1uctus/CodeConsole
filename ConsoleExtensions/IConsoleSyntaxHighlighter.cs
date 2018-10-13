using System;
using System.Collections.Generic;

namespace ConsoleExtensions {
    public interface IConsoleSyntaxHighlighter {
        void Highlight(IEnumerable<string> codeLines, ref List<Exception> errors, ref List<Exception> warnings);
    }
}
