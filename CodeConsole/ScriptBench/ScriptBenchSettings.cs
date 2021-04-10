using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CodeConsole.ScriptBench {
    [JsonObject(MemberSerialization.OptIn)]
    public class ScriptBenchSettings {
        [JsonIgnore] public const string DefaultConfigPath = "ScriptBench.config.json";

        [JsonIgnore] public const ConsoleColor DefaultFramesColor = ConsoleColor.DarkGray;

        /// <summary>
        ///     Default text to be shown in editor's header.
        /// </summary>
        public string DefaultHeader { get; } = "No errors found.";

        /// <summary>
        ///     Console color for editor UI.
        /// </summary>
        public ConsoleColor MainColor { get; } = DefaultFramesColor;

        /// <summary>
        ///     Console color for editor UI elements.
        /// </summary>
        public ConsoleColor AccentColor { get; } = ConsoleColor.Magenta;

        /// <summary>
        ///     User prompt used in single-line mode.
        /// </summary>
        public string SingleLinePrompt { get; }

        /// <summary>
        ///     Tab character used in editor.
        /// </summary>
        public int TabSize { get; set; }

        /// <summary>
        ///     Tab character used in editor.
        /// </summary>
        [JsonIgnore]
        public string Tabulation => new string(' ', TabSize);

        /// <summary>
        ///     If true, highlights leading whitespaces as unicode middle-dots.
        /// </summary>
        public bool ShowWhitespaces { get; }

        public BoxDrawingCharactersCollection DrawingChars { get; } = new();

        public ScriptBenchSettings(string prompt = null) {
            SingleLinePrompt = prompt ?? "";
            TabSize          = 4;
        }

        public static ScriptBenchSettings FromConfigFile(
            string filePath = DefaultConfigPath
        ) {
            if (!Path.IsPathRooted(filePath)) {
                filePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, filePath);
            }
            if (!File.Exists(filePath)) {
                return new ScriptBenchSettings();
            }
            string json = File.ReadAllText(filePath);
            try {
                return JsonConvert.DeserializeObject<ScriptBenchSettings>(json,
                    serializerSettings);
            }
            catch (Exception ex) {
                Console.WriteLine();
                ConsoleUtils.WriteLine(
                    ("ScriptBench reported error while reading config:", ConsoleColor.Red)
                );
                Console.WriteLine(ex.Message);
                return new ScriptBenchSettings();
            }
        }

        public void CreateMissingConfig() {
            if (!File.Exists(DefaultConfigPath)) {
                SaveConfigFile();
            }
        }

        public void SaveConfigFile(string filePath = DefaultConfigPath) {
            File.WriteAllText(filePath,
                JsonConvert.SerializeObject(this, serializerSettings));
        }

        static JsonSerializerSettings serializerSettings = new JsonSerializerSettings {
            Formatting       = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = {
                new SafeEnumConverter<ConsoleColor>(ConsoleColor.DarkGray)
            }
        };

        /// <summary>
        ///     ┌  ┐  └  ┘  │  ├  ┤  ─  ┬  ┴  ┼
        /// </summary>
        [JsonObject]
        public class BoxDrawingCharactersCollection {
            public char DownRight { get; } = '┌';
            public char DownLeft { get; } = '┐';
            public char UpRight { get; } = '└';
            public char UpLeft { get; } = '┘';
            public char Vertical { get; } = '│';
            public char VerticalRight { get; } = '├';
            public char VerticalLeft { get; } = '┤';
            public char Horizontal { get; } = '─';
            public char HorizontalDown { get; } = '┬';
            public char HorizontalUp { get; } = '┴';
            public char Cross { get; } = '┼';
        }

        public class SafeEnumConverter<T> : StringEnumConverter
            where T : Enum {
            T DefaultValue { get; }

            public SafeEnumConverter(T defaultValue) {
                DefaultValue = defaultValue;
            }

            public override object ReadJson(
                JsonReader     reader,
                Type           objectType,
                object         existingValue,
                JsonSerializer serializer
            ) {
                try {
                    return base.ReadJson(
                        reader,
                        objectType,
                        existingValue,
                        serializer
                    );
                }
                catch (JsonSerializationException) {
                    return DefaultValue;
                }
            }
        }
    }
}
