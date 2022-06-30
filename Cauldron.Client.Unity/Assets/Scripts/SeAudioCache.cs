using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SeAudioCache
{
    public enum SeAudioType
    {
        Play,
        Destroy,
        Attack,
        Draw,
        Exclude,
        Damage,
        Heal,
        AddField,
        Ok,
        Cancel,
        Lose,
        Win,
    }

    private static readonly Dictionary<SeAudioType, AudioClip> cache = new Dictionary<SeAudioType, AudioClip>();


    public static (bool, AudioClip) GetOrInit(SeAudioType audioType)
    {
        if (!cache.TryGetValue(audioType, out var audioClip))
        {
            audioClip = GetAudioClip(audioType);
            if (audioClip == null)
            {
                return (false, null);
            }

            cache.Add(audioType, audioClip);
        }

        return (true, audioClip);
    }

    private static string GetPath(SeAudioType cardAudioType)
        => Path.Combine("Audios", $"_{cardAudioType.ToString().ToLower()}");

    private static AudioClip GetAudioClip(SeAudioType audioType)
    {
        return Resources.Load<AudioClip>(GetPath(audioType));
    }
}
