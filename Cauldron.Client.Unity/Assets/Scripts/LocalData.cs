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

        private static readonly string SettingsFilePath = Path.Combine(Application.persistentDataPath, "settings.json");

        private static LocalDataCache cache = default;
        private static LocalDataCache LoadFromFile()
        {
            try
            {
                if (cache == default)
                {
                    if (File.Exists(SettingsFilePath))
                    {
                        var json = File.ReadAllText(SettingsFilePath);
                        cache =
                            JsonUtility.FromJson<LocalDataCache>(json)
                            ?? new LocalDataCache();
                    }
                    else
                    {
                        cache = new LocalDataCache();
                    }
                }
            }
            catch
            {
                cache = new LocalDataCache();
            }

            return cache;
        }

        public static void SaveToFile()
        {
            if (cache == default)
            {
                return;
            }

            var json = JsonUtility.ToJson(cache);
            File.WriteAllText(SettingsFilePath, json);
        }
    }

    [Serializable]
    class LocalDataCache
    {
        public string ClientId = Guid.NewGuid().ToString();

        public string ServerAddress = "";
        public string PlayerName = "";
        public string DeckListJson = "";
    }
}
