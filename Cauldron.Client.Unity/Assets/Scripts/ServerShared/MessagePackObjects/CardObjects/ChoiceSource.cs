#nullable enable

using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// 選択候補
    /// </summary>
    [MessagePackObject(true)]
    public class ChoiceSource
    {
        public enum HowValue
        {
            /// <summary>
            /// ランダム選択
            /// </summary>
            [DisplayText("ランダム")]
            Random,
            /// <summary>
            /// すべて
            /// </summary>
            [DisplayText("すべて")]
            All,
        }

        /// <summary>
        /// 抽出方法
        /// </summary>
        public HowValue How { get; }

        /// <summary>
        /// 抽出する数
        /// </summary>
        public NumValue NumPicks { get; }

        /// <summary>
        /// 候補となるプレイヤーのOR条件の一覧。
        /// </summary>
        public PlayerCondition[] OrPlayerConditions { get; }

        /// <summary>
        /// 候補となるカードのOR条件の一覧。
        /// </summary>
        public CardCondition[] OrCardConditions { get; }

        /// <summary>
        /// 候補となるゲーム外カードのOR条件の一覧。
        /// </summary>
        public CardDefCondition[] OrCardDefConditions { get; }

        public ChoiceSource(HowValue how = HowValue.All, NumValue? numPicks = null,
            PlayerCondition[]? orPlayerConditions = null,
            CardCondition[]? orCardConditions = null,
            CardDefCondition[]? OrCardDefConditions = null
            )
        {
            this.How = how;
            this.NumPicks = numPicks ?? new NumValue(1);
            this.OrPlayerConditions = orPlayerConditions ?? Array.Empty<PlayerCondition>();
            this.OrCardConditions = orCardConditions ?? Array.Empty<CardCondition>();
            this.OrCardDefConditions = OrCardDefConditions ?? Array.Empty<CardDefCondition>();
        }
    }
}
