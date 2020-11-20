using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    class Server
    {
        private readonly GameMaster gameMaster;

        private readonly Dictionary<Guid, Action> startTurnNoticeListByPlayerId
            = new Dictionary<Guid, Action>();

        private readonly Dictionary<Guid, Func<IReadOnlyList<Guid>, Guid>> askCardActionsByPlayerId
            = new Dictionary<Guid, Func<IReadOnlyList<Guid>, Guid>>();

        public Server(ILogger logger)
        {
            var ruleBook = new RuleBook();
            var cardFactory = new CardFactory();

            var cardPool = new CardPool().Load();
            cardFactory.SetCardPool(cardPool);

            this.gameMaster = new GameMaster(ruleBook, cardFactory, logger, this.AskCard);
        }

        public Guid AskCard(Guid playerId, IReadOnlyList<Guid> candidates)
        {
            if (!this.askCardActionsByPlayerId.TryGetValue(playerId, out var action))
            {
                throw new Exception("");
            }

            return action(candidates);
        }

        public void StartTurnNotice(Guid playerId, Action action)
        {
            this.startTurnNoticeListByPlayerId.Add(playerId, action);
        }

        public IEnumerable<CardDef> GetDeckCandidates()
        {
            return this.gameMaster.CardPool
                .Where(c => c.Type != CardType.Token);
        }

        public Guid RegisterPlayer(string name, IEnumerable<Guid> deckGuidList, Func<IReadOnlyList<Guid>, Guid> askCardAction)
        {
            var playerId = this.gameMaster.CreateNewPlayer(name, deckGuidList);

            this.askCardActionsByPlayerId.Add(playerId, askCardAction);

            return playerId;
        }

        public void Start(Guid firstPlayerId)
        {
            this.gameMaster.Start(firstPlayerId);

            while (!this.gameMaster.GameOver)
            {
                this.startTurnNoticeListByPlayerId[this.gameMaster.CurrentPlayer.Id]();
            }
        }

        public CommandResult Resolve(Guid playerId, Func<CommandResult> action)
        {
            var result = action();

            //if (this.gameMaster.GameOver)
            //{
            //    var winner = this.gameMaster.GetWinner();
            //    this.logger.Information($"勝者：{winner.Name}");
            //}

            result.GameEnvironment = this.gameMaster.CreateEnvironment(playerId);

            return result;
        }

        public CommandResult GetEnvironment(Guid playerId)
        {
            //return this.gameMaster.GetEnvironment(playerId);

            return this.Resolve(playerId, () => CommandResult.Success());
        }

        public CommandResult StartTurn(Guid playerId)
        {
            return this.Resolve(playerId, () =>
            {
                var (isSucceeded, message) = this.gameMaster.StartTurn(playerId);
                return new CommandResult() { IsSucceeded = isSucceeded, ErrorMessage = message };
            });
        }

        public CommandResult EndTurn(Guid playerId)
        {
            return this.Resolve(playerId, () =>
            {
                var (isSucceeded, message) = this.gameMaster.EndTurn(playerId);
                return new CommandResult() { IsSucceeded = isSucceeded, ErrorMessage = message };
            });
        }

        public CommandResult PlayFromHand(Guid playerId, Guid handCardId)
        {
            return this.Resolve(playerId, () =>
            {
                var (isSucceeded, message) = this.gameMaster.PlayFromHand(playerId, handCardId);
                return new CommandResult() { IsSucceeded = isSucceeded, ErrorMessage = message };
            });
        }

        /// <summary>
        /// 新規に生成されるカードをプレイ（召喚時に効果で召喚とか）
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public CommandResult PlayDirect(Guid playerId, Guid cardId)
        {
            return this.Resolve(playerId, () =>
            {
                var (isSucceeded, message) = this.gameMaster.PlayDirect(playerId, cardId);
                return new CommandResult() { IsSucceeded = isSucceeded, ErrorMessage = message };
            });
        }

        public CommandResult AttackToPlayer(Guid playerId, Guid cardId, Guid damagePlayerId)
        {
            return this.Resolve(playerId, () =>
            {
                var (isSucceeded, message) = this.gameMaster.AttackToPlayer(playerId, cardId, damagePlayerId);
                return new CommandResult() { IsSucceeded = isSucceeded, ErrorMessage = message };
            });
        }

        public CommandResult HitPlayer(Guid playerId, Guid damagePlayer, int damage)
        {
            return this.Resolve(playerId, () =>
            {
                var (isSucceeded, message) = this.gameMaster.HitPlayer(playerId, damagePlayer, damage);
                return new CommandResult() { IsSucceeded = isSucceeded, ErrorMessage = message };
            });
        }

        public CommandResult AttackToCleature(Guid playerId, Guid attackCardId, Guid guardCardId)
        {
            return this.Resolve(playerId, () =>
            {
                var (isSucceeded, message) = this.gameMaster.AttackToCreature(playerId, attackCardId, guardCardId);
                return new CommandResult() { IsSucceeded = isSucceeded, ErrorMessage = message };
            });
        }

        public CommandResult HitCreature(Guid playerId, Guid creatureCardId, int damage)
        {
            return this.Resolve(playerId, () =>
            {
                var (isSucceeded, message) = this.gameMaster.HitCreature(playerId, creatureCardId, damage);
                return new CommandResult() { IsSucceeded = isSucceeded, ErrorMessage = message };
            });
        }
    }
}
