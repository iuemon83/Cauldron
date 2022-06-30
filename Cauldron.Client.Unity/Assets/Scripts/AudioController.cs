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
                controller.audioSource = gameObject.AddComponent<AudioSource>();
            }

            return controller;
        }

        public static AudioController Find() => GameObject.Find(nameof(AudioController))?.GetComponent<AudioController>();

        private AudioSource audioSource;

        public void PlayAudio(SeAudioCache.SeAudioType audioType)
        {
            var (b, a) = SeAudioCache.GetOrInit(audioType);
            if (b)
            {
                this.audioSource.PlayOneShot(a);
            }
        }

        public async UniTask PlayAudio(string cardName, CardAudioCache.CardAudioType cardAudioType)
        {
            var (b, a) = await CardAudioCache.GetOrDefaultSe(cardName, cardAudioType);
            if (b)
            {
                this.audioSource.PlayOneShot(a);
            }
        }

        public async UniTask<bool> PlayAudio2(string cardName, CardAudioCache.CardAudioType cardAudioType)
        {
            var (b, a) = await CardAudioCache.Get(cardName, cardAudioType);
            if (b)
            {
                this.audioSource.PlayOneShot(a);
                return true;
            }

            return false;
        }
    }
}
