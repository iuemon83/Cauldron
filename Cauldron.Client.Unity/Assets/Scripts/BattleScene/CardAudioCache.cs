﻿using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CardAudioCache
{
    public enum CardAudioType
    {
        Play,
        Destroy,
        Attack,
        Draw,
        Exclude,
        Damage,
        Heal,
        AddField,
    }

    private static readonly Dictionary<(string, CardAudioType), AudioClip> cache
        = new Dictionary<(string, CardAudioType), AudioClip>();

    private static readonly string[] fileExtensions = new[] { "mp3" };

    public static async UniTask<(bool, AudioClip)> GetOrDefaultSe(string cardName, CardAudioType cardAudioType)
    {
        var (exists, audioClip) = await Get(cardName, cardAudioType);

        if (exists)
        {
            return (true, audioClip);
        }

        var defaultAudioClip = GetDefaultAudioClip(cardAudioType);
        return (defaultAudioClip != null, defaultAudioClip);
    }

    public static async UniTask<(bool, AudioClip)> Get(string cardName, CardAudioType cardAudioType)
    {
        var key = (cardName, cardAudioType);
        if (!cache.TryGetValue(key, out var audioClip))
        {
            audioClip = await LoadAudioClip(cardName, cardAudioType);
            cache.Add(key, audioClip);
        }

        return (audioClip != null, audioClip);
    }

    private static string GetPath(string cardName, CardAudioType cardAudioType, string extension)
        => Path.Combine(Config.CardAudiosDirectoryPath, $"{cardName}_{cardAudioType.ToString().ToLower()}.{extension}");

    private static async UniTask<AudioClip> LoadAudioClip(string cardName, CardAudioType cardAudioType)
    {
        foreach (var extension in fileExtensions)
        {
            var s = await LoadAudioClip(cardName, cardAudioType, extension);
            if (s != null)
            {
                return s;
            }
        }

        return null;
    }

    private static async UniTask<AudioClip> LoadAudioClip(string cardName, CardAudioType cardAudioType, string extension)
    {
        var fullPath = GetPath(cardName, cardAudioType, extension);

        if (!File.Exists(fullPath))
        {
            return null;
        }

        using var www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.MPEG);
        await www.SendWebRequest();

        if (www.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
            return null;
        }
        else
        {
            return UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
        }
    }

    private static AudioClip GetDefaultAudioClip(CardAudioType cardAudioType)
    {
        var (_, a) = SeAudioCache.GetOrInit(SeAudioType(cardAudioType));
        return a;
    }

    private static SeAudioCache.SeAudioType SeAudioType(CardAudioType audioType) => audioType switch
    {
        CardAudioType.Play => SeAudioCache.SeAudioType.Play,
        CardAudioType.Destroy => SeAudioCache.SeAudioType.Destroy,
        CardAudioType.Attack => SeAudioCache.SeAudioType.Attack,
        CardAudioType.Draw => SeAudioCache.SeAudioType.Draw,
        CardAudioType.Exclude => SeAudioCache.SeAudioType.Exclude,
        CardAudioType.Damage => SeAudioCache.SeAudioType.Damage,
        CardAudioType.Heal => SeAudioCache.SeAudioType.Heal,
        CardAudioType.AddField => SeAudioCache.SeAudioType.AddField,
        _ => throw new System.NotImplementedException(),
    };
}
