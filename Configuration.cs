using System;
using System.IO;
using System.Text.Json;

namespace OverlayKeyboard
{
    public class KeyboardConfiguration
    {
        public int DefaultWidth { get; set; } = 1200;
        public int DefaultHeight { get; set; } = 300;
        public int ResetWidth { get; set; } = 1200;
        public int ResetHeight { get; set; } = 300;
        public int KeyFontSize { get; set; } = 14;
        public int NumberRowHeight { get; set; } = 40;
        public int LetterRowHeight { get; set; } = 50;
        public int SpacebarHeight { get; set; } = 50;
        public int KeySpacing { get; set; } = 2;
        public bool ShowExit { get; set; } = false;
        public bool ShowResize { get; set; } = false;
        public bool ShowReset { get; set; } = false;
        public bool ShowRefresh { get; set; } = true;

        public string ConfigFileName { get; set; } = "keyboard_config.json";

        public static KeyboardConfiguration Load()
        {
            try
            {
                if (File.Exists("keyboard_config.json"))
                {
                    string json = File.ReadAllText("keyboard_config.json");
                    var config = JsonSerializer.Deserialize<KeyboardConfiguration>(json);
                    if (config != null)
                    {
                        // Ensure reset values are set if they don't exist in the loaded config
                        if (config.ResetWidth == 0) config.ResetWidth = 1200;
                        if (config.ResetHeight == 0) config.ResetHeight = 300;
                        System.Diagnostics.Debug.WriteLine($"Loaded config: ResetWidth={config.ResetWidth}, ResetHeight={config.ResetHeight}");
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error or show message box if needed
                System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
            }

            return new KeyboardConfiguration();
        }

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFileName, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }
    }
}
