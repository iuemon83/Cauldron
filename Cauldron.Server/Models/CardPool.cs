using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cauldron.Server.Models
{
    public class CardPool
    {
        public CardDef[] Load()
        {
            var jsonFilePath = "cardset.json";
            var cardSet = this.LoadFromFile(jsonFilePath);

            //var fairy = CardDef.TokenCard(1, "フェアリー", "テストクリーチャー", 1, 1);

            //var goblin = CardDef.CreatureCard(1, "ゴブリン", "テストクリーチャー", 1, 2);

            //var mouse = CardDef.CreatureCard(1, $"{cardSet.Name}.ネズミ", "ネズミ", "テストクリーチャー", 1, 1, effects: new[]
            //{
            //    // 死亡時、相手に1ダメージ
            //    new CardEffect(CardEffectType.OnDestroy, (gameMaster, ownerCard) =>
            //    {
            //        var opponent = gameMaster.GetOpponent(ownerCard.OwnerId);
            //        gameMaster.HitPlayer(opponent.Id, 1);
            //    })
            //});

            //var ninja = CardDef.CreatureCard(1, "忍者", "テストクリーチャー", 1, 1, new[] { CreatureAbility.Stealth });

            //var waterFairy = CardDef.CreatureCard(1, $"{cardSet.Name}.ウォーターフェアリー", "ウォーターフェアリー", "テストクリーチャー", 1, 1, effects: new[]
            //{
            //    // 破壊時、フェアリー１枚を手札に加える
            //    new CardEffect(CardEffectType.OnDestroy, (gameMaster, ownerCard) =>
            //    {
            //        var owner = gameMaster.PlayersById[ownerCard.OwnerId];
            //        var newCard = gameMaster.GenerateNewCard($"{cardSet.Name}.フェアリー",owner.Id);
            //        //newCard.OwnerId = owner.Id;
            //        gameMaster.AddHand(owner, newCard);
            //    }),
            //});

            //CardDef slime = null;
            //slime = CardDef.CreatureCard(2, $"{cardSet.Name}.スライム", "スライム", "テストクリーチャー", 1, 1, effects: new[]
            //{
            //    // 召喚時、スライムを一体召喚
            //    new CardEffect(CardEffectType.OnPlay, (gameMaster, ownerCard) =>
            //    {
            //        var owner = gameMaster.PlayersById[ownerCard.OwnerId];
            //        var playingCard = gameMaster.GenerateNewCard(slime.Id,owner.Id);
            //        //playingCard.OwnerId = owner.Id;
            //        gameMaster.PlayDirect(ownerCard.OwnerId, playingCard.Id);
            //    }),
            //});

            //var knight = CardDef.CreatureCard(2, "ナイト", "テストクリーチャー", 1, 2, new[] { CreatureAbility.Cover });
            //var ninjaKnight = CardDef.CreatureCard(3, "忍者ナイト", "テストクリーチャー", 1, 2, new[] { CreatureAbility.Cover, CreatureAbility.Stealth });

            //var whiteGeneral = CardDef.CreatureCard(4, $"{cardSet.Name}.ホワイトジェネラル", "ホワイトジェネラル", "テストクリーチャー", 2, 2, effects: new[]
            //{
            //    // 召喚時、自分のクリーチャー一体を+2/+0
            //    new CardEffect(CardEffectType.OnPlay, (gameMaster, ownerCard) =>
            //    {
            //        var owner = gameMaster.PlayersById[ownerCard.OwnerId];
            //        var candidates = owner.Field.AllCards
            //            .Where(c => c.Type == CardType.Creature || c.Type == CardType.Token)
            //            .ToArray();
            //        var targetCreature = Program.RandomPick(candidates);
            //        if(targetCreature == default) return;

            //        gameMaster.Buff(targetCreature, 2, 0);
            //    }),
            //});

            //var commander = CardDef.CreatureCard(6, $"{cardSet.Name}.セージコマンダー", "セージコマンダー", "テストクリーチャー", 3, 3, effects: new[]
            //{
            //    // 召喚時、自分のクリーチャーすべてを+1/+1
            //    new CardEffect(CardEffectType.OnPlay, (gameMaster, ownerCard) =>
            //    {
            //        var owner = gameMaster.PlayersById[ownerCard.OwnerId];
            //        var candidates = owner.Field.AllCards
            //            .Where(c => c.Type == CardType.Creature || c.Type == CardType.Token);

            //        foreach(var targetCreature in candidates)
            //        {
            //            gameMaster.Buff(targetCreature, 1, 1);
            //        }
            //    }),
            //});

            var angel = CardDef.ArtifactCard(2, $"{cardSet.Name}.天使の像", "天使の像", "テストアーティファクト", new[]
            {
                // ターン開始時、カレントプレイヤーに1ダメージ
                new CardEffect(CardEffectType.OnStartTurn, (gameMaster, ownerCard) =>
                {
                    var player = gameMaster.CurrentPlayer;
                    gameMaster.HitPlayer(player.Id, 1);
                }),
            });

            var devil = CardDef.ArtifactCard(1, $"{cardSet.Name}.悪魔の像", "悪魔の像", "テストアーティファクト", new[]
            {
                // ターン終了時、相手クリーチャーに1ダメージ。その後このカードを破壊
                new CardEffect(CardEffectType.OnEndTurn, (gameMaster, ownerCard) =>
                {
                    var owner = gameMaster.PlayersById[ownerCard.OwnerId];
                    var opponent = gameMaster.GetOpponent(owner.Id);
                    var candidates =opponent.Field.AllCards
                        .Where(c=>c.Type == CardType.Creature)
                        .ToArray();

                    if(candidates.Any())
                    {
                        var targetCreatureCard = candidates[Program.Random.Next(candidates.Length)];
                        if(targetCreatureCard != null)
                        {
                            gameMaster.HitCreature(targetCreatureCard.Id, 1);
                        }
                    }

                    gameMaster.DestroyCard(ownerCard);
                }),
            });

            var fortuneSpring = CardDef.ArtifactCard(2, $"{cardSet.Name}.運命の泉", "運命の泉", "テストアーティファクト", new[]
            {
                // ターン終了時、自分のクリーチャーをランダムで+1/+0
                new CardEffect(CardEffectType.OnEndTurn, (gameMaster, ownerCard) =>
                {
                    // カードのオーナーのみ
                    if(gameMaster.CurrentPlayer.Id != ownerCard.OwnerId) return;

                    var candidates = gameMaster.CurrentPlayer.Field.AllCards
                        .Where(c => c.Type == CardType.Creature)
                        .ToArray();

                    if(!candidates.Any())return;

                    var card = candidates[Program.Random.Next(candidates.Length)];
                    if(card != null)
                    {
                        gameMaster.Buff(card,1,0);
                    }
                }),
            });

            //var flag = CardDef.ArtifactCard(4, $"{cardSet.Name}.王家の御旗", "王家の御旗", "テストソーサリー",
            //    effects: new[]
            //    {
            //        // 使用時、すべての自分クリーチャーを+1/+0
            //        new CardEffect(CardEffectType.OnPlay, (gameMaster, ownerCard) =>
            //        {
            //            var owner = gameMaster.PlayersById[ownerCard.OwnerId];
            //            var targets = owner.Field.AllCards
            //                .Where(card => card.Type == CardType.Creature);

            //            foreach(var target in targets)
            //            {
            //                gameMaster.Buff(target, 1, 0);
            //            }
            //        }),
            //        // 自分クリーチャーのプレイ時+1/+0
            //        new CardEffect(CardEffectType.OnEveryPlay, (gameMaster, playingCard) =>
            //        {
            //            if(playingCard.Type == CardType.Creature)
            //            {
            //                gameMaster.Buff(playingCard, 1, 0);
            //            }
            //        }),
            //    });

            var shock = CardDef.SorceryCard(1, $"{cardSet.Name}.ショック", "ショック", "テストソーサリー",
                effects: new[]
                {
                    // 使用時、相手かクリーチャーのランダムに2ダメージ
                    new CardEffect(CardEffectType.OnPlay, (gameMaster, ownerCard) =>
                    {
                        var opponent = gameMaster.GetOpponent(ownerCard.OwnerId);
                        var candidates = opponent.Field.AllCards
                            .Where(c => c.Type == CardType.Creature || c.Type == CardType.Token)
                            .ToArray();
                        var randIndex = Program.Random.Next(candidates.Length + 1);

                        if(randIndex <candidates.Length)
                        {
                            var creatureCard = candidates[randIndex];
                            gameMaster.HitCreature(creatureCard.Id, 2);
                        }
                        else
                        {
                            gameMaster.HitPlayer(opponent.Id, 2);
                        }
                    }),
                });

            var shippu = CardDef.SorceryCard(2, $"{cardSet.Name}.疾風怒濤", "疾風怒濤", "テストソーサリー",
                require: new CardRequireToPlay(environment =>
                {
                    return environment.Opponent.Field.AllCards
                        .Any(c => c.Type == CardType.Creature || c.Type == CardType.Token);
                }),
                effects: new[]
                {
                    // 使用時、対象の相手クリーチャー一体にxダメージ。x="自分の場のクリーチャーの数"
                    new CardEffect(CardEffectType.OnPlay, (gameMaster, ownerCard) =>
                    {
                        var owner = gameMaster.PlayersById[ownerCard.OwnerId];
                        var damage = owner.Field.AllCards
                            .Count(c => c.Type == CardType.Creature || c.Type == CardType.Token);

                        Card targetCreature = gameMaster.AskCard(owner.Id, TargetCardType.OpponentCreature);

                        gameMaster.HitCreature(targetCreature.Id, damage);
                    }),
                });

            var buf = CardDef.SorceryCard(3, $"{cardSet.Name}.武装強化", "武装強化", "テストソーサリー",
                require: new CardRequireToPlay(environment =>
                {
                    return environment.You.Field.AllCards
                        .Any(c => c.Type == CardType.Creature || c.Type == CardType.Token);
                }),
                effects: new[]
                {
                    // 使用時、対象の自分クリーチャーを+2/+2
                    new CardEffect(CardEffectType.OnPlay, (gameMaster, ownerCard) =>
                    {
                        var owner = gameMaster.PlayersById[ownerCard.OwnerId];
                        var targetCreature = gameMaster.AskCard(owner.Id, TargetCardType.YourCreature);

                        gameMaster.Buff(targetCreature, 2, 2);
                    }),
                });

            var manual = new[] {
                //fairy, goblin, slime, mouse, waterFairy,
                //ninja, knight, ninjaKnight, whiteGeneral,
                angel, devil, fortuneSpring, 
                //flag,
                //shock, shippu, buf
            };

            return cardSet.AsCardDefs().ToArray();

            return cardSet.AsCardDefs()
                .Concat(manual)
                .ToArray();
        }

        private CardSet LoadFromFile(string jsonFilePath)
        {
            var jsonString = File.ReadAllText(jsonFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            var cardSet = JsonSerializer.Deserialize<CardSet>(jsonString, options);
            return cardSet;
        }
    }
}
