﻿using Grpc.Core;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class ConnectionHolder : MonoBehaviour
    {
        public static async Task<ConnectionHolder> Create(string serverHost, string playerName)
        {
            // DontDestroy化
            var holder = Find();
            if (holder == null)
            {
                var gameObject = new GameObject("ConnectionHolder");
                DontDestroyOnLoad(gameObject);

                // 接続
                holder = gameObject.AddComponent<ConnectionHolder>();
            }

            await holder.Connect(serverHost, playerName);

            return holder;
        }

        public static ConnectionHolder Find() => GameObject.Find("ConnectionHolder")?.GetComponent<ConnectionHolder>();

        public CauldronHubReceiver Receiver { get; } = new CauldronHubReceiver();

        private Channel channel;
        public Client Client { get; private set; }

        private async Task Connect(string serverHost, string playerName)
        {
            this.channel = new Channel(serverHost, ChannelCredentials.Insecure);

            await this.channel.ConnectAsync(DateTime.UtcNow.Add(TimeSpan.FromSeconds(5)));

            this.Client = new Client(this.channel, playerName, this.Receiver, Debug.Log, Debug.LogError);
        }

        private void OnDestroy()
        {
            this.Client?.Destroy();
            this.channel?.ShutdownAsync();
        }
    }
}