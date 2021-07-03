using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects;
using MagicOnion;
using System.Threading.Tasks;

namespace Cauldron.Shared.Services
{
    public interface ICauldronHub : IStreamingHub<ICauldronHub, ICauldronHubReceiver>
    {
        Task<CardDef[]> GetCardPool();

        Task<RuleBook> GetRuleBook();

        Task<GameOutline[]> ListOpenGames();

        Task<OpenNewGameReply> OpenNewGame(OpenNewGameRequest request);

        Task<CardDef[]> GetCardPoolByGame(GameId gameId);

        Task<EnterGameReply> EnterGame(EnterGameRequest request);

        Task<bool> LeaveGame(GameId gameId);

        Task ReadyGame(ReadyGameRequest request);

        Task<StartTurnReply> StartTurn(StartTurnRequest request);

        Task<EndTurnReply> EndTurn(EndTurnRequest request);

        Task<PlayFromHandReply> PlayFromHand(PlayFromHandRequest request);

        Task<AttackToCreatureReply> AttackToCreature(AttackToCreatureRequest request);

        Task<AttackToPlayerReply> AttackToPlayer(AttackToPlayerRequest request);

        Task<(GameMasterStatusCode, CardId[])> ListPlayableCardId(GameId gameId);

        Task<(GameMasterStatusCode, (PlayerId[], CardId[]))> ListAttackTargets(GameId gameId, CardId cardId);
    }
}