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
                return LoadPrivateLocalDataFromFile().ClientId;
            }
        }

        public static string ServerAddress
        {
            get
            {
                return LoadPublicLocalDataFromFile().ServerAddress;
            }

            set => LoadPublicLocalDataFromFile().ServerAddress = value;
        }

        public static string PlayerName
        {
            get
            {
                return LoadPublicLocalDataFromFile().PlayerName;
            }
            set => LoadPublicLocalDataFromFile().PlayerName = value;
        }

        public static string DeckListJson
        {
            get
            {
                return LoadPublicLocalDataFromFile().DeckListJson;
            }
            set => LoadPublicLocalDataFromFile().DeckListJson = value;
        }

        private static LocalDataCache publicCache = default;

        private static LocalDataCache LoadPublicLocalDataFromFile()
        {
            if (publicCache != default)
            {
                return publicCache;
            }

            try
            {

                return publicCache = LocalDataCache.LoadFromFile();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                return publicCache = new LocalDataCache();
            }
        }

        private static PrivateLocalDataCache privateCache = default;

        private static PrivateLocalDataCache LoadPrivateLocalDataFromFile()
        {
            if (privateCache != default)
            {
                return privateCache;
            }

            try
            {
                return privateCache = PrivateLocalDataCache.LoadFromFile();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                return privateCache = new PrivateLocalDataCache();
            }
        }

        public static void SaveToFile()
        {
            publicCache?.SaveToFile();
            privateCache?.SaveToFile();
        }
    }

    [Serializable]
    class PrivateLocalDataCache
    {
        private static readonly string filePath
            = Path.Combine(Application.persistentDataPath, "settings.json");

        public static PrivateLocalDataCache LoadFromFile()
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<PrivateLocalDataCache>(json)
                    ?? new PrivateLocalDataCache();
            }
            else
            {
                return new PrivateLocalDataCache();
            }
        }

        public string ClientId = Guid.NewGuid().ToString();

        public void SaveToFile()
        {
            var json = JsonUtility.ToJson(this);
            File.WriteAllText(filePath, json);
        }
    }

    [Serializable]
    class LocalDataCache
    {
        private static readonly string filePath = Path.Combine(Application.dataPath, "settings.json");

        public static LocalDataCache LoadFromFile()
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<LocalDataCache>(json)
                    ?? new LocalDataCache();
            }
            else
            {
                return new LocalDataCache();
            }
        }

        public string ServerAddress = "";
        public string PlayerName = "";
        public string DeckListJson = "";

        public void SaveToFile()
        {
            var json = JsonUtility.ToJson(this);
            File.WriteAllText(filePath, json);
        }
    }
}
