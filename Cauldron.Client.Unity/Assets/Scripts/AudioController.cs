using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class AudioController : MonoBehaviour
    {
        public static AudioController CreateOrFind()
        {
            // DontDestroy化
            var controller = Find();
            if (controller == null)
            {
                var gameObject = new GameObject(nameof(AudioController));
                DontDestroyOnLoad(gameObject);

                // 接続
                controller = gameObject.AddComponent<AudioController>();
                controller.bgmAudioSource = gameObject.AddComponent<AudioSource>();
                controller.bgmAudioSource.loop = true;

                controller.seAudioSource = gameObject.AddComponent<AudioSource>();
            }

            return controller;
        }

        private static AudioController Find()
        {
            var obj = GameObject.Find(nameof(AudioController));
            return obj == null
                ? null
                : obj.GetComponent<AudioController>();
        }

        private AudioSource bgmAudioSource;
        private AudioSource seAudioSource;

        public async UniTask PlayBgm(BgmAudioCache.BgmAudioType bgmType)
        {
            var x = await BgmAudioCache.GetOrInit(bgmType);
            if (x.exists)
            {
                this.bgmAudioSource.clip = x.audio;
                this.bgmAudioSource.volume = 0.1f;
                this.bgmAudioSource.Play();
            }
        }

        public void StopBgm()
        {
            this.bgmAudioSource.Stop();
        }

        public void PlaySe(SeAudioCache.SeAudioType audioType)
        {
            var (b, a) = SeAudioCache.GetOrInit(audioType);
            if (b)
            {
                this.seAudioSource.PlayOneShot(a);
            }
        }

        public async UniTask PlaySe(string cardName, CardAudioCache.CardAudioType cardAudioType)
        {
            var (b, a) = await CardAudioCache.GetOrDefaultSe(cardName, cardAudioType);
            if (b)
            {
                this.seAudioSource.PlayOneShot(a);
            }
        }

        public async UniTask<bool> PlaySe2(string cardName, CardAudioCache.CardAudioType cardAudioType)
        {
            var (b, a) = await CardAudioCache.Get(cardName, cardAudioType);
            if (b)
            {
                this.seAudioSource.PlayOneShot(a);
                return true;
            }

            return false;
        }
    }
}
