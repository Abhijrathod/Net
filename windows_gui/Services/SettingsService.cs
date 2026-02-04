using System;
using System.IO;
using System.Text;
using DNSChanger.Models;
using Newtonsoft.Json;

namespace DNSChanger.Services
{
    public class SettingsService
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DNSChanger",
            "settings.json");

        private static readonly string ProfilesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DNSChanger",
            "profiles.json");

        private static readonly string BackupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DNSChanger",
            "backups");

        public static AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
            return new AppSettings();
        }

        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public static System.Collections.Generic.List<DnsProfile> LoadProfiles()
        {
            try
            {
                if (File.Exists(ProfilesPath))
                {
                    string json = File.ReadAllText(ProfilesPath);
                    return JsonConvert.DeserializeObject<System.Collections.Generic.List<DnsProfile>>(json) 
                        ?? new System.Collections.Generic.List<DnsProfile>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profiles: {ex.Message}");
            }
            return new System.Collections.Generic.List<DnsProfile>();
        }

        public static void SaveProfiles(System.Collections.Generic.List<DnsProfile> profiles)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ProfilesPath)!);
                string json = JsonConvert.SerializeObject(profiles, Formatting.Indented);
                File.WriteAllText(ProfilesPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving profiles: {ex.Message}");
            }
        }

        public static void CreateBackup(string backupName)
        {
            try
            {
                Directory.CreateDirectory(BackupPath);
                string backupFile = Path.Combine(BackupPath, $"{backupName}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                // Backup logic would go here
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating backup: {ex.Message}");
            }
        }
    }
}
