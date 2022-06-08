using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts
{
    class LocalData
    {
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

        private static string SettingsFilePath => Path.Combine(Application.dataPath, "settings.json");

        private static readonly string localDataPath = Path.Combine(Application.persistentDataPath, "data.json");

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
                        applicationSettingsCache = JsonUtility.FromJson<ApplicationSettings>(json) ?? new ApplicationSettings();
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

        private static LocalData2 localDataCache = default;
        public static LocalData2 LoadLocalData()
        {
            Debug.Log(localDataPath);

            if (localDataCache == default)
            {
                if (File.Exists(localDataPath))
                {
                    var json = File.ReadAllText(localDataPath);
                    localDataCache = JsonUtility.FromJson<LocalData2>(json);
                }
            }

            localDataCache = localDataCache ?? new LocalData2();

            return localDataCache;
        }

        public static void SaveLocalData()
        {
            if (localDataCache == default)
            {
                return;
            }

            var json = JsonUtility.ToJson(localDataCache);
            File.WriteAllText(localDataPath, json);
        }
    }

    [Serializable]
    class LocalData2
    {
        public List<LocalBattleLog> BattleLogs = new List<LocalBattleLog>();

        public void AddBattleLog(LocalBattleLog log)
        {
            if (this.BattleLogs == null)
            {
                this.BattleLogs = new List<LocalBattleLog>();
            }

            this.BattleLogs.Add(log);
        }
    }

    [Serializable]
    class ApplicationSettings
    {
        public string ServerAddress = "";
        public string PlayerName = "";
        public string DeckListJson = "";
    }
}
