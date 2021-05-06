using UnityEngine;

namespace Assets.Scripts
{
    class LocalData
    {
        public static string ServerAddress
        {
            get => PlayerPrefs.GetString(nameof(ServerAddress));
            set => PlayerPrefs.SetString(nameof(ServerAddress), value);
        }

        public static string PlayerName
        {
            get => PlayerPrefs.GetString(nameof(PlayerName));
            set => PlayerPrefs.SetString(nameof(PlayerName), value);
        }

        public static string DeckListJson
        {
            get => PlayerPrefs.GetString(nameof(DeckListJson));
            set => PlayerPrefs.SetString(nameof(DeckListJson), value);
        }
    }
}
