namespace Cauldron.Server.Models
{
    public record RuleBook(
        // プレイヤーHP の初期値
        int InitialPlayerHp = 20,
        // プレイヤーHP の最大値
        int MaxPlayerHp = 20,
        // プレイヤーHP の最小値
        int MinPlayerHp = 0,
        // デッキ枚数の上限
        int MaxNumDeckCards = 30,
        // デッキ枚数の下限
        int MinNumDeckCards = 30,
        // 手札の枚数の初期値
        int InitialNumHands = 5,
        // 手札の枚数の最大値
        int MaxNumHands = 9,
        // プレイヤーMP の初期値
        int InitialMp = 0,
        // プレイヤーMP の上限
        int MaxLimitMp = 10,
        // プレイヤーMP の最小値
        int MinMp = 0,
        // ターン毎に増加するプレイヤーMP
        int MpByStep = 1,
        // 場におけるカード枚数の上限
        int MaxNumFieldCars = 5,
        // 攻撃可能となるまでのターン数の初期値
        int DefaultNumTurnsToCanAttack = 1,
        // 1ターン中に攻撃可能な回数のデフォルト値
        int DefaultNumAttacksLimitInTurn = 1
        )
    {
        public RuleBook(Cauldron.Grpc.Models.RuleBook other)
            : this(
                 other.InitialPlayerHp,
                 other.MaxPlayerHp,
                 other.MinPlayerHp,
                 other.MaxNumDeckCards,
                 other.MinNumDeckCards,
                 other.InitialNumHands,
                 other.MaxNumHands,
                 other.InitialMp,
                 other.MaxMp,
                 other.MinMp,
                 other.MpByStep,
                 other.MaxNumFieldCars
                 )
        { }
    }
}
