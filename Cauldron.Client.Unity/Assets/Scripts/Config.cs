﻿using Cauldron.Shared.MessagePackObjects;
using System.IO;
using UnityEngine;

class Config
{
    /// <summary>
    /// カードの画像を格納するディレクトリのパス
    /// </summary>
    public static readonly string CardImagesDirectoryPath = Path.Combine(Application.dataPath, "CardImages");

    public static string ServerAddress { get; set; }
    public static int Port { get; set; }
    public static string PlayerName { get; set; }
    public static GameId GameId { get; set; }
}
