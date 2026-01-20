using System;
using System.IO;
using System.Text.Json;

namespace ClassroomManagement.Services
{
    /// <summary>
    /// Local settings storage for connection preferences
    /// </summary>
    public class LocalSettings
    {
        private static LocalSettings? _instance;
        public static LocalSettings Instance => _instance ??= Load();

        private static string SettingsFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClassroomManagement",
            "settings.json"
        );

        public string SavedConnectionCode { get; set; } = string.Empty;
        public DateTime? LastConnectionAttempt { get; set; }

        private LocalSettings() { }

        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsFilePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                LogService.Instance.Warning("LocalSettings", $"Failed to save settings: {ex.Message}");
            }
        }

        private static LocalSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    return JsonSerializer.Deserialize<LocalSettings>(json) ?? new LocalSettings();
                }
            }
            catch (Exception ex)
            {
                LogService.Instance.Warning("LocalSettings", $"Failed to load settings: {ex.Message}");
            }

            return new LocalSettings();
        }
    }
}
