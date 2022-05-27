﻿using MessagePack;
using System.IO;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class RuleBook
    {
        /// <summary>
        /// ゲーム開始時点のプレイヤーのHP
        /// </summary>
        public int InitialPlayerHp { get; }

        /// <summary>
        /// ゲーム中のプレイヤーのHPの上限値
        /// </summary>
        public int MaxPlayerHp { get; }

        /// <summary>
        /// ゲーム中のプレイヤーのHPの下限値
        /// </summary>
        public int MinPlayerHp { get; }

        /// <summary>
        /// デッキ枚数の上限値
        /// </summary>
        public int MaxNumDeckCards { get; }

        /// <summary>
        /// デッキ枚数の下限値
        /// </summary>
        public int MinNumDeckCards { get; }

        /// <summary>
        /// ゲーム開始時点の手札枚数
        /// </summary>
        public int InitialNumHands { get; }

        /// <summary>
        /// 手札の枚数の上限値
        /// </summary>
        public int MaxNumHands { get; }

        /// <summary>
        /// 1度のドロー枚数
        /// </summary>
        public int NumDraws { get; }

        /// <summary>
        /// 先攻1ターン目のドロー枚数
        /// </summary>
        public int FirstPlayerNumDrawsInFirstTurn { get; }

        /// <summary>
        /// 後攻1ターン目のドロー枚数
        /// </summary>
        public int SecondPlayerNumDrawsInFirstTurn { get; }

        /// <summary>
        /// 先攻のゲーム開始時点のMP
        /// </summary>
        public int FirstPlayerStartMp { get; }

        /// <summary>
        /// 後攻のゲーム開始時点のMP
        /// </summary>
        public int SecondPlayerStartMp { get; }

        /// <summary>
        /// ゲーム中のMPの上限値
        /// </summary>
        public int MaxLimitMp { get; }

        /// <summary>
        /// ゲーム中のMPの下限値
        /// </summary>
        public int MinMp { get; }

        /// <summary>
        /// ターン開始時に増加するMP
        /// </summary>
        public int LimitMpToIncrease { get; }

        /// <summary>
        /// 場に出せるカード枚数の上限値
        /// </summary>
        public int MaxNumFieldCards { get; }

        /// <summary>
        /// 攻撃可能となるまでのターン数の基本値
        /// </summary>
        public int DefaultNumTurnsToCanAttack { get; }

        /// <summary>
        /// 1ターン中に攻撃できる回数の基本値
        /// </summary>
        public int DefaultNumAttacksLimitInTurn { get; }

        /// <summary>
        /// デッキに含められる枚数の基本値
        /// </summary>
        public int DefaultLimitNumCardsInDeck { get; }

        public RuleBook(
            int InitialPlayerHp,
            int MaxPlayerHp,
            int MinPlayerHp,
            int MaxNumDeckCards,
            int MinNumDeckCards,
            int InitialNumHands,
            int MaxNumHands,
            int NumDraws,
            int FirstPlayerNumDrawsInFirstTurn,
            int SecondPlayerNumDrawsInFirstTurn,
            int FirstPlayerStartMp,
            int SecondPlayerStartMp,
            int MaxLimitMp,
            int MinMp,
            int LimitMpToIncrease,
            int MaxNumFieldCards,
            int DefaultNumTurnsToCanAttack,
            int DefaultNumAttacksLimitInTurn,
            int DefaultLimitNumCardsInDeck
            )
        {
            this.InitialPlayerHp = InitialPlayerHp;
            this.MaxPlayerHp = MaxPlayerHp;
            this.MinPlayerHp = MinPlayerHp;
            this.MaxNumDeckCards = MaxNumDeckCards;
            this.MinNumDeckCards = MinNumDeckCards;
            this.InitialNumHands = InitialNumHands;
            this.MaxNumHands = MaxNumHands;
            this.NumDraws = NumDraws;
            this.FirstPlayerNumDrawsInFirstTurn = FirstPlayerNumDrawsInFirstTurn;
            this.SecondPlayerNumDrawsInFirstTurn = SecondPlayerNumDrawsInFirstTurn;
            this.FirstPlayerStartMp = FirstPlayerStartMp;
            this.SecondPlayerStartMp = SecondPlayerStartMp;
            this.MaxLimitMp = MaxLimitMp;
            this.MinMp = MinMp;
            this.LimitMpToIncrease = LimitMpToIncrease;
            this.MaxNumFieldCards = MaxNumFieldCards;
            this.DefaultNumTurnsToCanAttack = DefaultNumTurnsToCanAttack;
            this.DefaultNumAttacksLimitInTurn = DefaultNumAttacksLimitInTurn;
            this.DefaultLimitNumCardsInDeck = DefaultLimitNumCardsInDeck;
        }
    }
}
