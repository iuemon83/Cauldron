using Cauldron.Shared.MessagePackObjects;
using System.IO;
using UnityEngine;

class Config
{
    /// <summary>
    /// カードの画像を格納するディレクトリのパス
    /// </summary>
    public static readonly string CardImagesDirectoryPath = Path.Combine(Application.dataPath, "CardImages");

    /// <summary>
    /// カードの画像を格納するディレクトリのパス
    /// </summary>
    public static readonly string DeckListFielPath = Path.Combine(Application.dataPath, "deck_list.json");
}
