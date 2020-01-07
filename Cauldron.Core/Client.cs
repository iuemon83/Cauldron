using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Core
{
    class Client
    {
        public Guid PlayerId { get; }

        public string PlayerName { get; }

        public Client(Server server, RuleBook ruleBook, string playerName)
        {
            this.PlayerName = playerName;

            var cardPool = server.GetDeckCandidates().ToArray();

            var deckGuidList = Enumerable.Range(0, ruleBook.MaxNumDeckCards)
                .Select(_ => cardPool[Program.Random.Next(cardPool.Length)].Id);

            this.PlayerId = server.RegisterPlayer(playerName, deckGuidList, this.CreatAskCardAction(server));
        }

        private Func<string, Guid> CreatAskCardAction(Server server)
        {
            return targetCardType =>
            {
                switch (targetCardType)
                {
                    case "YourCreature":
                        {
                            var environment = server.GetEnvironment(this.PlayerId).GameEnvironment;
                            var candidates = environment.You.Field.AllCards
                                .Where(c => c.Type == CardType.Creature || c.Type == CardType.Token)
                                .ToArray();

                            return Program.RandomPick(candidates).Id;
                        }

                    case "OpponentCreature":
                        {
                            var environment = server.GetEnvironment(this.PlayerId).GameEnvironment;
                            var candidates = environment.Opponent.Field.AllCards
                                .Where(c => c.Type == CardType.Creature || c.Type == CardType.Token)
                                .ToArray();

                            return Program.RandomPick(candidates).Id;
                        }

                    default:
                        throw new InvalidOperationException();
                }
            };
        }

        public CommandResult PlayTurn(Server server, Logger logger)
        {
            logger.Information("----------------------------------");

            var commands = OneTurn(server);
            var result = LoopCommand(commands, logger);

            logger.Information(result.GameEnvironment.You.Name);
            logger.Information($"フィールド: {string.Join(",", result.GameEnvironment.You.Field.AllCards.Select(c => logger.LogText(c)))}");
            logger.Information(result.GameEnvironment.Opponent.Name);
            logger.Information($"フィールド: {string.Join(",", result.GameEnvironment.Opponent.Field.AllCards.Select(c => logger.LogText(c)))}");

            return result;
        }

        public bool IsPutFieldCard(Card card)
        {
            return card.Type == CardType.Artifact
                || card.Type == CardType.Creature
                || card.Type == CardType.Token
                ;
        }

        public bool IsPlayable(GameEnvironment environment, Card card)
        {
            // フィールドに出すカードはフィールドに空きがないとプレイできない
            if (IsPutFieldCard(card) && environment.You.Field.Full)
            {
                return false;
            }

            // コストが払えないとプレイできない
            if (environment.You.UsableMp < card.Cost)
            {
                return false;
            }

            // 各カードごとの条件
            if (!card.Require.IsPlayable(environment))
            {
                return false;
            }

            return true;
        }

        public bool CanAttack(Card attackCard)
        {
            return attackCard != null
                // クリーチャーでなければ攻撃できない
                && attackCard.Type == CardType.Creature
                ;
        }

        public bool CanAttack(Card attackCard, PublicPlayerInfo guardPlayer)
        {
            var existsCover = guardPlayer.Field.AllCards
                .Any(c => c.Abilitiy == CreatureAbility.Cover && c.Abilitiy != CreatureAbility.Stealth);

            return
                // 攻撃可能なカード
                CanAttack(attackCard)
                // 持ち主には攻撃できない
                && attackCard.OwnerId != guardPlayer.Id
                // カバーされていない
                && !existsCover
                ;
        }

        public bool CanAttack(Card attackCard, Card guardCard, GameEnvironment environment)
        {
            var guardPlayer = environment.Opponent;
            var existsCover = guardPlayer.Field.AllCards
                .Any(c => c.Abilitiy == CreatureAbility.Cover && c.Abilitiy != CreatureAbility.Stealth);

            var coverCheck = guardCard.Abilitiy == CreatureAbility.Cover
                || !existsCover;

            return
                // 攻撃可能なカード
                CanAttack(attackCard)
                // 自分自信のカードには攻撃できない
                && attackCard.OwnerId != guardCard.OwnerId
                // クリーチャー以外には攻撃できない
                && guardCard.Type == CardType.Creature
                // ステルス状態は攻撃対象にならない
                && guardCard.Abilitiy != CreatureAbility.Stealth
                // カバー関連のチェック
                && coverCheck
                ;
        }

        public IEnumerable<Func<CommandResult>> OneTurn(Server server)
        {
            CommandResult result = null;

            // 開始
            yield return () => result = server.StartTurn(this.PlayerId);

            // ランダムな手札を使用する
            var candidateCards = result.GameEnvironment.You.Hands.AllCards
                .Where(handCard => IsPlayable(result.GameEnvironment, handCard))
                .ToArray();

            if (candidateCards.Any())
            {
                var playCard = candidateCards[Program.Random.Next(candidateCards.Length)];
                yield return () => result = server.PlayFromHand(result.GameEnvironment.You.Id, playCard.Id);
            }

            // フィールドのすべてのカードで敵に攻撃
            var attacs = result.GameEnvironment.You
                .Field.AllCards
                .Where(card => card.Type == CardType.Creature)
                .Select(attackCard =>
                {
                    var canTargetCards = result.GameEnvironment.Opponent.Field.AllCards
                        .Where(opponentCard => this.CanAttack(attackCard, opponentCard, result.GameEnvironment))
                        .ToArray();

                    var canAttackToPlayer = this.CanAttack(attackCard, result.GameEnvironment.Opponent);
                    var canAttackToCreature = canTargetCards.Any();

                    // 敵のモンスターがいる
                    if (canAttackToCreature && Program.Random.Next(100) > 50)
                    {
                        var opponentCardId = canTargetCards[0].Id;
                        return new Func<CommandResult>(() => result = server.AttackToCleature(this.PlayerId, attackCard.Id, opponentCardId));
                    }
                    else if (canAttackToPlayer)
                    {
                        return new Func<CommandResult>(() => result = server.AttackToPlayer(
                            result.GameEnvironment.You.Id,
                            attackCard.Id,
                            result.GameEnvironment.Opponent.Id));
                    }

                    return null;
                })
                .Where(command => command != null);

            foreach (var a in attacs)
            {
                yield return a;
            }

            // 終わり
            yield return () => result = server.EndTurn(this.PlayerId);
        }

        public CommandResult LoopCommand(IEnumerable<Func<CommandResult>> playings, Logger logger)
        {
            CommandResult result = null;
            foreach (var playing in playings)
            {
                result = playing();

                if (!result.IsSucceeded)
                {
                    throw new Exception($"コマンドの実行に失敗: {result.ErrorMessage}");
                }

                var myself = result.GameEnvironment.You;
                var opponent = result.GameEnvironment.Opponent;

                logger.Information($"HP: {myself.Name}({myself.Hp}), {opponent.Name}({opponent.Hp})");

                logger.Information("");

                if (result.GameEnvironment.GameOver) break;
            }

            return result;
        }
    }
}
