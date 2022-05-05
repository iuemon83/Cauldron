using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class ConnectionHolder : MonoBehaviour
    {
        public static async UniTask<ConnectionHolder> Create(string serverHost, string playerName)
        {
            // DontDestroy化
            var holder = Find();
            if (holder == null)
            {
                var gameObject = new GameObject(nameof(ConnectionHolder));
                DontDestroyOnLoad(gameObject);

                // 接続
                holder = gameObject.AddComponent<ConnectionHolder>();
            }

            await holder.Connect(serverHost, playerName);

            return holder;
        }

        public static ConnectionHolder Find() => GameObject.Find(nameof(ConnectionHolder))?.GetComponent<ConnectionHolder>();

        public IReadOnlyDictionary<CardDefId, CardDef> CardPool { get; private set; }
            = new Dictionary<CardDefId, CardDef>();

        public CauldronHubReceiver Receiver { get; } = new CauldronHubReceiver();

        private Grpc.Core.Channel channel;
        public Client Client { get; private set; }

        public async UniTask LoadCardPool()
        {
            if (this.Client == null)
            {
                throw new InvalidOperationException("カードプールの読み込みに失敗");
            }

            var cardpool = await this.Client.GetCardPool();
            this.CardPool = cardpool.ToDictionary(c => c.Id);
        }

        private async UniTask Connect(string serverHost, string playerName)
        {
            this.channel = new Grpc.Core.Channel(serverHost, Grpc.Core.ChannelCredentials.Insecure);

            await this.channel.ConnectAsync(DateTime.UtcNow.Add(TimeSpan.FromSeconds(5)));

            this.Client = await Client.Factory(this.channel, playerName, this.Receiver, Debug.Log, Debug.LogError);
        }

        private void OnDestroy()
        {
            this.Client?.Destroy();
            this.channel?.ShutdownAsync();
        }

        private void OnApplicationQuit()
        {
            LocalData.SaveToFile();
        }
    }
}
