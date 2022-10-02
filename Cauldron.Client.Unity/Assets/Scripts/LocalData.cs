using System;
using System.IO;
using UnityEngine;

namespace Assets.Scripts
{
    class LocalData
    {
        public static string ClientId
        {
            get
            {
                return LoadFromFile().ClientId;
            }
        }

        public static string ServerAddress
        {
            get
            {
                return LoadFromFile().ServerAddress;
            }

            set => LoadFromFile().ServerAddress = value;
        }

        public static string PlayerName
        {
            get
            {
                return LoadFromFile().PlayerName;
            }
            set => LoadFromFile().PlayerName = value;
        }

        public static string DeckListJson
        {
            get
            {
                return LoadFromFile().DeckListJson;
            }
            set => LoadFromFile().DeckListJson = value;
        }

        private static readonly string SettingsFilePath = Path.Combine(Application.dataPath, "settings.json");

        private static ApplicationSettings applicationSettingsCache = default;
        private static ApplicationSettings LoadFromFile()
        {
            try
            {
                if (applicationSettingsCache == default)
                {
                    if (File.Exists(SettingsFilePath))
                    {
                        var json = File.ReadAllText(SettingsFilePath);
                        applicationSettingsCache =
                            JsonUtility.FromJson<ApplicationSettings>(json)
                            ?? new ApplicationSettings();
                    }
                    else
                    {
                        applicationSettingsCache = new ApplicationSettings();
                    }
                }
            }
            catch
            {
                applicationSettingsCache = new ApplicationSettings();
            }

            return applicationSettingsCache;
        }

        public static void SaveToFile()
        {
            if (applicationSettingsCache == default)
            {
                return;
            }

            var json = JsonUtility.ToJson(applicationSettingsCache);
            File.WriteAllText(SettingsFilePath, json);
        }
    }

    [Serializable]
    class ApplicationSettings
    {
        public string ClientId = Guid.NewGuid().ToString();

        public string ServerAddress = "";
        public string PlayerName = "";
        public string DeckListJson = "";
    }
}
