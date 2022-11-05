using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BgmAudioCache
{
    public enum BgmAudioType
    {
        Default
    }

    private static readonly Dictionary<BgmAudioType, AudioClip> cache = new Dictionary<BgmAudioType, AudioClip>();

    public static async UniTask<(bool exists, AudioClip audio)> GetOrInit(BgmAudioType audioType)
    {
        if (!cache.TryGetValue(audioType, out var audioClip))
        {
            audioClip = await GetAudioClip(audioType);
            if (audioClip == null)
            {
                return (false, null);
            }

            cache.Add(audioType, audioClip);
        }

        return (true, audioClip);
    }

    private static string GetPath(BgmAudioType bgmType)
        => Path.Combine("Audios", "Bgm", $"{bgmType.ToString().ToLower()}");

    private static async UniTask<AudioClip> GetAudioClip(BgmAudioType bgmType)
    {
        return await Resources.LoadAsync<AudioClip>(GetPath(bgmType)) as AudioClip;
    }
}
