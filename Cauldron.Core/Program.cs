using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Core
{
    class Program
    {
        public static readonly Random Random = new Random();
        private static readonly Dictionary<Guid, Client> clients = new Dictionary<Guid, Client>();

        public static T RandomPick<T>(IReadOnlyList<T> source) => source.Any() ? source[Program.Random.Next(source.Count)] : default;

        static void Main(string[] args)
        {
            var logger = new Logger();

            var server = new Server(logger);

            var client1 = new Client(server, new RuleBook(), "リュウ");
            var client2 = new Client(server, new RuleBook(), "ケン");

            clients.Add(client1.PlayerId, client1);
            clients.Add(client2.PlayerId, client2);

            CommandResult result = null;
            server.StartTurnNotice(client1.PlayerId, () => result = client1.PlayTurn(server, logger));
            server.StartTurnNotice(client2.PlayerId, () => result = client2.PlayTurn(server, logger));

            server.Start(client1.PlayerId);

            //LoopTurn(server, logger);

            var winnerName = clients[result.GameEnvironment.WinnerPlayerId].PlayerName;
            logger.Information($"{winnerName}の勝ち!!!!");
        }

        //static void LoopTurn(Server server, Logger logger)
        //{
        //    CommandResult result = null;

        //    while (!result?.GameEnvironment?.GameOver ?? true)
        //    {
        //        result = clients[environment.Myself.Id].PlayTurn(server, logger);
        //    }

        //    var winnerName = clients[result.GameEnvironment.WinnerPlayerId].PlayerName;
        //    logger.Information($"{winnerName}の勝ち!!!!");
        //}
    }
}
