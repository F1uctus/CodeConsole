using System;

namespace CodeConsole.ScriptBench {
    public class ScriptBenchSettings {
        /// <summary>
        ///     Default text to be shown in editor's header.
        /// </summary>
        internal const string DefaultHeader = "No errors found.";

        /// <summary>
        ///     Console color for editor UI.
        /// </summary>
        internal const ConsoleColor FramesColor = ConsoleColor.DarkGray;

        /// <summary>
        ///     Current console highlighter.
        /// </summary>
        public readonly ISyntaxHighlighter Highlighter;

        /// <summary>
        ///     Highlight user input with specified <see cref="Highlighter" />.
        ///     True if highlighter is not null.
        /// </summary>
        public bool SyntaxHighlighting => Highlighter != null;

        /// <summary>
        ///     User prompt used in single-line mode.
        /// </summary>
        public readonly string Prompt;

        /// <summary>
        ///     Tab character used in editor.
        /// </summary>
        internal readonly string Tabulation;

        /// <summary>
        ///     Use only 1 editable line.
        /// </summary>
        public readonly bool SingleLineMode;

        public ScriptBenchSettings(
            bool               singleLineMode = false,
            string             prompt         = null,
            ISyntaxHighlighter highlighter    = null
        ) {
            SingleLineMode = singleLineMode;
            Prompt = SingleLineMode
                ? prompt ?? ""
                : prompt != null
                    ? throw new ArgumentException("Cannot use prompt for multiline editor.")
                    : "";
            Tabulation  = new string(' ', 4);
            Highlighter = highlighter;
        }
    }
}