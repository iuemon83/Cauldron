#nullable enable

using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    static class Utility
    {
        public static T RandomPick<T>(IReadOnlyList<T> source) => source.Any()
            ? source[UnityEngine.Random.Range(0, source.Count)]
            : default!;

        public static string GetPlayerName(GameContext gameContext, PlayerId playerId)
        {
            return gameContext.You.PublicPlayerInfo.Id == playerId
                ? gameContext.You.PublicPlayerInfo.Name
                : gameContext.Opponent.Name;
        }

        public static (string ownerPlayerName, Card? card) GetCard(GameContext gameContext, Zone zone, CardId cardId)
        {
            var zonePlayer = gameContext.You.PublicPlayerInfo.Id == zone.PlayerId
                ? gameContext.You.PublicPlayerInfo
                : gameContext.Opponent;

            var card = zone.ZoneName switch
            {
                ZoneName.Hand => gameContext.You.PublicPlayerInfo.Id == zone.PlayerId
                    ? gameContext.You.Hands.FirstOrDefault(c => c.Id == cardId)
                    : null,
                ZoneName.Field => zonePlayer.Field.FirstOrDefault(c => c?.Id == cardId),
                ZoneName.Cemetery => zonePlayer.Cemetery.FirstOrDefault(c => c.Id == cardId),
                _ => null
            };

            if (card == null) return ("", default);

            var ownerName = GetPlayerName(gameContext, card.OwnerId);

            return (ownerName, card);
        }

        public static Card? GetCard(GameContext gameContext, CardId cardId)
        {
            // 非公開領域に移動した場合は取れない
            return gameContext.You.Hands
                .Concat(gameContext.You.PublicPlayerInfo.Field)
                .Concat(gameContext.You.PublicPlayerInfo.Cemetery)
                .Concat(gameContext.Opponent.Field)
                .Concat(gameContext.Opponent.Cemetery)
                .Concat(gameContext.TemporaryCards)
                .FirstOrDefault(c => c?.Id == cardId);
        }

        public static (string ownerPlayerName, Card? card) GetCardAndOwner(GameContext gameContext, CardId cardId)
        {
            var card = GetCard(gameContext, cardId);

            if (card == default)
            {
                return ("", default);
            }

            var ownerName = GetPlayerName(gameContext, card.OwnerId);

            return (ownerName, card);
        }

        public static (string ownerPlayerName, string cardName) GetCardName(GameContext gameContext, Zone zone, CardId cardId)
        {
            var (p, c) = GetCard(gameContext, zone, cardId);
            return (p, c?.Name ?? "");
        }

        public static async UniTask LoadAsyncScene(SceneNames sceneName)
        {
            await LoadAsyncScene(sceneName, () => { });
        }

        public static async UniTask LoadAsyncScene(SceneNames sceneName, Action onLoadAction)
        {
            await SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Additive);

            onLoadAction?.Invoke();

            await SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }

        public static async UniTask LoadAsyncScene(SceneNames sceneName, Func<UniTask> onLoadActionAsync)
        {
            await SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Additive);

            if (onLoadActionAsync != null)
            {
                await onLoadActionAsync();
            }

            await SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }

        public static string DisplayText(CardType value) => value switch
        {
            CardType.Creature => "クリーチャー",
            CardType.Artifact => "アーティファクト",
            CardType.Sorcery => "魔法",
            _ => "",
        };

        public static string DisplayText(CreatureAbility value) => value switch
        {
            CreatureAbility.None => "なし",
            CreatureAbility.Cover => "守護",
            CreatureAbility.Stealth => "潜伏",
            CreatureAbility.Deadly => "必殺",
            CreatureAbility.Sealed => "封印",
            _ => "",
        };

        public static string DisplayText(ZoneName value) => value switch
        {
            ZoneName.None => "なし",
            ZoneName.CardPool => "カードプール",
            ZoneName.Field => "場",
            ZoneName.Hand => "手札",
            ZoneName.Deck => "デッキ",
            ZoneName.Cemetery => "墓地",
            ZoneName.Excluded => "除外",
            ZoneName.Temporary => "場",
            _ => "",
        };

        public static string DisplayTextForNumAttacksLimitInTurn(int value)
        {
            return value == 0
                ? "攻撃不能"
                : value == 1 ? "" : $"攻撃回数({value})";
        }

        public static string DisplayTextForNumTurnsToCanAttack(ICardDef cardDef)
        {
            return cardDef.NumTurnsToCanAttackToCreature == 0
                ? cardDef.NumTurnsToCanAttackToPlayer == 0
                    ? "速攻"
                    : "突進"
                : cardDef.NumTurnsToCanAttackToCreature == 1
                    ? ""
                    : $"鈍足({cardDef.NumTurnsToCanAttackToCreature})";
        }

        public static string DisplayTextForNumTurnsToCanAttack(Card card)
        {
            return card.NumTurnsToCanAttackToCreature == 0
                ? card.NumTurnsToCanAttackToPlayer == 0
                    ? "速攻"
                    : "突進"
                : card.NumTurnsToCanAttackToCreature == 1
                    ? ""
                    : $"鈍足({card.NumTurnsToCanAttackToCreature})";
        }

        public static string EffectDescription(CardDef cardDef)
        {
            return string.Join(Environment.NewLine,
                new[]
                {
                cardDef.IsToken ? "<color=\"red\">トークン</color>" : "",
                string.Join(",", cardDef.Annotations),
                string.Join(",",
                    cardDef.Abilities.Select(Utility.DisplayText)
                    .Concat(new[]
                    {
                        Utility.DisplayTextForNumAttacksLimitInTurn(cardDef.NumAttacksLimitInTurn ?? default),
                        Utility.DisplayTextForNumTurnsToCanAttack(cardDef),
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    ),
                cardDef.EffectDescription
                }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                );
        }

        public static string EffectDescription(Card card)
        {
            return string.Join(Environment.NewLine,
                new[]
                {
                card.IsToken ? "<color=\"red\">トークン</color>" : "",
                string.Join(",", card.Annotations),
                string.Join(",",
                    card.Abilities.Select(Utility.DisplayText)
                    .Concat(new[]
                    {
                        Utility.DisplayTextForNumAttacksLimitInTurn(card.NumAttacksLimitInTurn),
                        Utility.DisplayTextForNumTurnsToCanAttack(card),
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    ),
                card.EffectDescription
                }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                );
        }

    }
}
