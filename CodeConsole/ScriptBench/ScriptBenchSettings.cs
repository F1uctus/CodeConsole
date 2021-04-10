using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CodeConsole.ScriptBench {
    [JsonObject(MemberSerialization.OptIn)]
    public class ScriptBenchSettings {
        public const string       DefaultConfigPath  = "ScriptBench.config.json";
        public const ConsoleColor DefaultFramesColor = ConsoleColor.DarkGray;

        /// <summary>
        ///     Default text to be shown in editor's header.
        /// </summary>
        [JsonProperty] public string DefaultHeader = "No errors found.";

        /// <summary>
        ///     Console color for editor UI.
        /// </summary>
        [JsonProperty] public ConsoleColor MainColor = DefaultFramesColor;

        /// <summary>
        ///     Console color for editor UI elements.
        /// </summary>
        [JsonProperty] public ConsoleColor AccentColor = ConsoleColor.Magenta;

        /// <summary>
        ///     User prompt used in single-line mode.
        /// </summary>
        [JsonProperty] public readonly string SingleLinePrompt;

        /// <summary>
        ///     Tab character used in editor.
        /// </summary>
        [JsonProperty] public int TabSize;

        /// <summary>
        ///     Tab character used in editor.
        /// </summary>
        public string Tabulation => new string(' ', TabSize);

        /// <summary>
        ///     If true, highlights leading whitespaces as unicode middle-dots.
        /// </summary>
        [JsonProperty] public bool ShowWhitespaces;

        [JsonProperty] public BoxDrawingCharactersCollection DrawingChars =
            new BoxDrawingCharactersCollection();

        public ScriptBenchSettings(string prompt = null) {
            SingleLinePrompt = prompt ?? "";
            TabSize          = 4;
        }

        public static ScriptBenchSettings FromConfigFile(string filePath = DefaultConfigPath) {
            if (!Path.IsPathRooted(filePath)) {
                filePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, filePath);
            }
            if (!File.Exists(filePath)) {
                return new ScriptBenchSettings();
            }
            string json = File.ReadAllText(filePath);
            try {
                return JsonConvert.DeserializeObject<ScriptBenchSettings>(json, serializerSettings);
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
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, serializerSettings));
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
            public char DownRight      = '┌';
            public char DownLeft       = '┐';
            public char UpRight        = '└';
            public char UpLeft         = '┘';
            public char Vertical       = '│';
            public char VerticalRight  = '├';
            public char VerticalLeft   = '┤';
            public char Horizontal     = '─';
            public char HorizontalDown = '┬';
            public char HorizontalUp   = '┴';
            public char Cross          = '┼';
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
